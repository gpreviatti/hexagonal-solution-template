using Infrastructure.Common;
using Infrastructure.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;

namespace Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        var serviceConfiguration = builder.Configuration
            .GetSection("Services")
            .Get<List<ServiceConfiguration>>()
            ?? throw new ArgumentNullException(builder.Configuration.GetSection("Services").Path, "Services configuration is not configured.");

        builder.Services
            .AddHttp(serviceConfiguration)
            .AddScoped<IBaseHttpService, BaseHttpService>();

        builder.AddOpenTelemetry();

        return builder;
    }

    private static WebApplicationBuilder AddOpenTelemetry(this WebApplicationBuilder builder)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var serviceName = DefaultConfigurations.ApplicationName;
        var serviceVersion = DefaultConfigurations.Version;

        var resourceBuilder = ResourceBuilder
            .CreateDefault()
            .AddService(serviceName, serviceVersion: serviceVersion, serviceNamespace: environment);

        builder.Services
            .AddOpenTelemetry()
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
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter()
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

    private static IServiceCollection AddHttp(this IServiceCollection services, List<ServiceConfiguration> serviceConfiguration)
    {
        var ordersConfiguration = serviceConfiguration.FirstOrDefault(x =>
            string.Equals(x.Name, ServicesKey.Orders.ToString(), StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentNullException(nameof(serviceConfiguration), "Orders service configuration is not configured.");

        services.AddHttpClient(ordersConfiguration.Name, client =>
        {
            client.BaseAddress = new Uri(ordersConfiguration.BaseAddress);
            client.DefaultRequestVersion = new Version(ordersConfiguration.ProtocolVersion, 0);

            if (ordersConfiguration.Headers is Dictionary<string, string> headers && headers.Count > 0)
            {
                foreach (var header in headers)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
        })
        .SetHandlerLifetime(TimeSpan.FromMinutes(5))
        .AddPolicyHandler(GetRetryPolicy());

        services.AddKeyedScoped<IBaseHttpService, BaseHttpService>(ServicesKey.Orders.ToString(), (serviceProvider, _) =>
        {
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var logger = serviceProvider.GetRequiredService<ILogger<BaseHttpService>>();
            var client = httpClientFactory.CreateClient(ordersConfiguration.Name);
            return new BaseHttpService(client, logger, ordersConfiguration.ProtocolVersion);
        });

        return services;
    }

    private static AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy() => HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}
