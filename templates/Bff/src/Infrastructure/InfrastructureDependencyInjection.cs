using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Infrastructure.Cache;
using Infrastructure.Http;
using Infrastructure.Common;

namespace Infrastructure;

public static class InfrastructureDependencyInjection
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder AddInfrastructure()
        {
            var configuration = builder.Configuration;

            builder.Services
                .AddCache(configuration)
                .AddHttpServices(configuration);

            builder.AddOpenTelemetry();

            return builder;
        }

        internal WebApplicationBuilder AddOpenTelemetry()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var exporterProtocol = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_PROTOCOL")?.ToLower() == "grpc"
                ? OtlpExportProtocol.Grpc
                : OtlpExportProtocol.HttpProtobuf;
            var exporterMetricsEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_METRICS_ENDPOINT");
            var exporterTracesEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_TRACES_ENDPOINT");
            var exporterLogsEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_LOGS_ENDPOINT");

            if (
                string.Equals(environment, "IntegrationTests", StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(exporterLogsEndpoint) ||
                string.IsNullOrWhiteSpace(exporterMetricsEndpoint) ||
                string.IsNullOrWhiteSpace(exporterTracesEndpoint)
            )
            {
                return builder;
            }

            builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddEnvironmentVariableDetector())
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(options =>
                {
                    options.Protocol = exporterProtocol;
                    options.Endpoint = new Uri(exporterMetricsEndpoint);
                })
            )
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation(options =>
                {
                    options.RecordException = true;
                })
                .AddEntityFrameworkCoreInstrumentation(
                    options =>
                    {
                        options.SetDbStatementForText = true;
                        options.SetDbStatementForStoredProcedure = true;
                    }
                )
                .AddRedisInstrumentation()
                .AddGrpcClientInstrumentation()
                .AddOtlpExporter(options =>
                {
                    options.Protocol = exporterProtocol;
                    options.Endpoint = new Uri(exporterTracesEndpoint!);
                })
            )
            .WithLogging(logging => logging
                .AddOtlpExporter(options =>
                {
                    options.Protocol = exporterProtocol;
                    options.Endpoint = new Uri(exporterLogsEndpoint!);
                })
            );

            builder.Services.AddLogging(logging => logging.AddOpenTelemetry(openTelemetryLoggerOptions =>
            {
                openTelemetryLoggerOptions.IncludeScopes = true;
                openTelemetryLoggerOptions.IncludeFormattedMessage = true;
            }));

            return builder;
        }
    }

    extension(IServiceCollection services)
    {
        internal IServiceCollection AddCache(IConfiguration configuration)
        {
            services
            .AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("Redis") ?? throw new NullReferenceException("Redis connection string is not configured.");
                options.Configuration += ",abortConnect=false,connectTimeout=5000,syncTimeout=5000";
            })
            .AddHybridCache(options =>
            {
                options.DefaultEntryOptions = new()
                {
                    Expiration = TimeSpan.FromMinutes(30),
                    LocalCacheExpiration = TimeSpan.FromMinutes(30)
                };
            });

            services.AddSingleton<HybridCacheService>();

            return services;
        }

        internal IServiceCollection AddHttpServices(IConfiguration configuration)
        {

            services.AddKeyedScoped<BaseHttpService>(ServicesKeys.Orders, (serviceProvider, cancellationToken) =>
            {
                var serviceConfiguration = configuration.GetSection("Services:Orders").Get<ServiceConfigurations>() ?? throw new NullReferenceException("Orders service configuration is not configured.");

                var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                var logger = serviceProvider.GetRequiredService<ILogger<BaseHttpService>>();
                var client = httpClientFactory.CreateClient(ServicesKeys.Orders);
                client.BaseAddress = new Uri(serviceConfiguration.Url) ?? throw new NullReferenceException("Orders service address is not configured.");
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                return new(client, logger);
            });

            services.AddKeyedScoped<BaseHttpService>(ServicesKeys.Payments, (serviceProvider, cancellationToken) =>
            {
                var serviceConfiguration = configuration.GetSection("Services:Payments").Get<ServiceConfigurations>() ?? throw new NullReferenceException("Payments service configuration is not configured.");

                var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                var logger = serviceProvider.GetRequiredService<ILogger<BaseHttpService>>();
                var client = httpClientFactory.CreateClient(ServicesKeys.Payments);
                client.BaseAddress = new Uri(serviceConfiguration.Url) ?? throw new NullReferenceException("Payments service address is not configured.");
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                return new(client, logger);
            });

            return services;
        }
    }
}
