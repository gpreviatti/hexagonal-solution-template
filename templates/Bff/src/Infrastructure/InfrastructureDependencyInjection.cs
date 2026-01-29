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
using Polly;
using Polly.Extensions.Http;
using System.Globalization;
using Infrastructure.Grpc;

namespace Infrastructure;

#pragma warning disable CA1708 // Identifiers should differ by more than case
public static class InfrastructureDependencyInjection
#pragma warning restore CA1708 // Identifiers should differ by more than case
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder AddInfrastructure()
        {
            var configuration = builder.Configuration;

            builder.Services
                .AddCache(configuration)
                .AddHttp(configuration);

            builder.AddOpenTelemetry();

            return builder;
        }

        internal WebApplicationBuilder AddOpenTelemetry()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var exporterProtocol = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_PROTOCOL")?.ToLower(CultureInfo.CurrentCulture) == "grpc"
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
                options.Configuration = configuration.GetConnectionString("Redis") ?? throw new ArgumentNullException(configuration.GetConnectionString("Redis"),"Redis connection string is not configured.");
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

        internal IServiceCollection AddHttp(IConfiguration configuration)
        {
            var httpConfigurations = configuration.GetSection("Http").Get<List<ServiceConfigurations>>()
                ?? throw new ArgumentNullException(configuration.GetSection("Http").Path, "Http services configuration is not configured.");

            var serviceKeys = Enum.GetValues<ServicesKeys>();

            foreach (var serviceKey in serviceKeys)
            {
                var serviceName = serviceKey.ToString();

                var serviceConfiguration = httpConfigurations.FirstOrDefault(x =>
                    string.Equals(x.Name, serviceName, StringComparison.OrdinalIgnoreCase))
                    ?? throw new ArgumentNullException($"{serviceName} service configuration is not configured.");

                services.AddHttpClient(serviceName, client =>
                {
                    client.BaseAddress = new Uri(serviceConfiguration.BaseAddress)
                        ?? throw new ArgumentNullException($"{serviceName} service address is not configured.");

                    if (serviceConfiguration.Headers is Dictionary<string, string> headers && headers.Count > 0)
                        foreach (var header in headers)
                            client.DefaultRequestHeaders.Add(header.Key, header.Value);
                })
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddPolicyHandler(GetRetryPolicy());

                services.AddKeyedScoped<BaseHttpService>(serviceKey, (serviceProvider, _) =>
                {
                    var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                    var logger = serviceProvider.GetRequiredService<ILogger<BaseHttpService>>();
                    var client = httpClientFactory.CreateClient(serviceName);

                    return new(client, logger);
                });
            }

            return services;
        }

        internal IServiceCollection AddGrpc(this IServiceCollection services)
        {
            services.AddKeyedScoped<PaymentsService>(ServicesKeys.Payments, (serviceProvider, _) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<PaymentsService>>();

                return new PaymentsService(logger);
            });

            return services;
        }

        internal static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() => HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}
