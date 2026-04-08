using Domain.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Pyroscope.OpenTelemetry;

namespace Infrastructure.OpenTelemetry;

internal static class InfrastructureOpenTelemetryDependencyInjection
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder AddOpenTelemetry()
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
                    .AddMeter(
                        DefaultConfigurations.Meter.Name,
                        "System.Diagnostics.Metrics",
                        "Microsoft.AspNetCore.Hosting",
                        "Microsoft.AspNetCore.Server.Kestrel",
                        "System.Net.Http"
                    )
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
                    .AddRabbitMQInstrumentation()
                    .AddGrpcClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
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
}
