using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
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
using Infrastructure.Grpc;
using Infrastructure.Common;
using GrpcPayment;
using Pyroscope.OpenTelemetry;

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

            var serviceConfiguration = configuration.GetSection("Services").Get<List<ServiceConfiguration>>()
                ?? throw new ArgumentNullException(configuration.GetSection("Services").Path, "Services configuration is not configured.");

            builder.Services
                .AddCache(configuration)
                .AddHttp(serviceConfiguration)
                .AddGrpc(serviceConfiguration);

            builder.AddOpenTelemetry();

            return builder;
        }

        internal WebApplicationBuilder AddOpenTelemetry()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (string.Equals(environment, "IntegrationTests", StringComparison.OrdinalIgnoreCase))
                return builder;

            var serviceName = DefaultConfigurations.ApplicationName;
            var serviceVersion = DefaultConfigurations.Version;
            var resourceBuilder = ResourceBuilder
                .CreateDefault()
                .AddService(serviceName, serviceVersion: serviceVersion, serviceNamespace: environment);

            builder.Services.AddOpenTelemetry()
                .WithMetrics(metrics => metrics
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddOtlpExporter()
                )
                .WithTracing(tracing => tracing
                    .AddSource(serviceName)
                    .SetResourceBuilder(resourceBuilder)
                    .AddRedisInstrumentation()
                    .AddGrpcClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter()
                    .AddProcessor(new PyroscopeSpanProcessor())
                );

            builder.Logging.AddOpenTelemetry(options =>
            {
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
                options.ParseStateValues = true;
                options
                    .SetResourceBuilder(resourceBuilder)
                    .AttachLogsToActivityEvent()
                    .AddOtlpExporter();
            });

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

        internal IServiceCollection AddHttp(List<ServiceConfiguration> serviceConfiguration)
        {

            var serviceKeys = Enum.GetValues<ServicesKey>();

            foreach (var serviceKey in serviceKeys)
            {
                var serviceName = serviceKey.ToString();

                var serviceConfig = serviceConfiguration.FirstOrDefault(x =>
                    string.Equals(x.Name, serviceName, StringComparison.OrdinalIgnoreCase))
                    ?? throw new ArgumentNullException($"{serviceName} service configuration is not configured.");

                services.AddHttpClient(serviceName, client =>
                {
                    client.BaseAddress = new Uri(serviceConfig.BaseAddress)
                        ?? throw new ArgumentNullException($"{serviceName} service address is not configured.");

                    client.DefaultRequestVersion = new(serviceConfig.ProtocolVersion, 0);

                    if (serviceConfig.Headers is Dictionary<string, string> headers && headers.Count > 0)
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

                    return new(client, logger, serviceConfig.ProtocolVersion);
                });
            }

            return services;
        }

        internal IServiceCollection AddGrpc(List<ServiceConfiguration> serviceConfiguration)
        {
            services.AddGrpc();
            services
                .AddGrpcClient<PaymentService.PaymentServiceClient>(nameof(PaymentsService), o =>
                {
                    var paymentsConfiguration = serviceConfiguration.FirstOrDefault(x =>
                        string.Equals(x.Name, ServicesKey.Payments.ToString(), StringComparison.OrdinalIgnoreCase))
                        ?? throw new ArgumentNullException($"{ServicesKey.Payments} gRPC service configuration is not configured.");

                    o.Address = new Uri(paymentsConfiguration.BaseAddress);
                })
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddPolicyHandler(GetRetryPolicy());

            services.AddScoped<PaymentsService>();

            return services;
        }

        internal static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() => HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}
