using Application.Common.Constants;
using Grafana.OpenTelemetry;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Infrastructure.OpenTelemetry;

internal static class InfrastructureOpenTelemetryDependencyInjection
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder AddOpenTelemetry()
        {
            var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService("Hexagonal.Solution.Template.WebApp");

            builder.Services.AddOpenTelemetry()
                .WithMetrics(metrics =>
                {
                    metrics.AddView(
                        "http.server.request.duration",
                        new ExplicitBucketHistogramConfiguration()
                        {
                            Boundaries = [0, 0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10]
                        }
                    );
                    metrics.AddMeter(
                        DefaultConfigurations.Meter.Name,
                        "System.Diagnostics.Metrics",
                        "Microsoft.AspNetCore.Hosting",
                        "Microsoft.AspNetCore.Server.Kestrel",
                        "System.Net.Http"
                    );

                    metrics
                        .SetResourceBuilder(resourceBuilder)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation()
                        .AddProcessInstrumentation()
                        .AddPrometheusExporter()
                        .UseGrafana();
                })
                .WithTracing(tracing => tracing
                    .AddSource("Hexagonal.Solution.Template.WebApp")
                    .SetResourceBuilder(resourceBuilder)
                    .AddRedisInstrumentation()
                    .AddRabbitMQInstrumentation()
                    .AddGrpcClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .UseGrafana()
                );

            builder.Logging.AddOpenTelemetry(logging =>
            {
                logging.SetResourceBuilder(resourceBuilder);
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;
                logging.ParseStateValues = true;
                logging
                    .AttachLogsToActivityEvent()
                    .UseGrafana();
            });

            return builder;
        }
    }
}
