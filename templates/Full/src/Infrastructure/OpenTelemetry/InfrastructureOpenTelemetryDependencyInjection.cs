using Application.Common.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
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
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var exporterProtocol = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_PROTOCOL")?.ToLower(System.Globalization.CultureInfo.InvariantCulture) == "grpc"
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

            var serviceName = DefaultConfigurations.ApplicationName;
            var serviceVersion = DefaultConfigurations.Version;
            var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(serviceName, serviceVersion: serviceVersion);

            builder.Services.AddOpenTelemetry()
                .WithMetrics(metrics =>
                {
                    metrics.AddView(
                        "http.server.request.duration",
                        new ExplicitBucketHistogramConfiguration()
                        {
                            Boundaries = [0, 0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10]
                        }
                    )
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
                    .AddPrometheusExporter()
                    .AddOtlpExporter(options =>
                    {
                        options.Protocol = exporterProtocol;
                        options.Endpoint = new Uri(exporterMetricsEndpoint);
                    });
                })
                .WithTracing(tracing => tracing
                    .AddSource(serviceName)
                    .SetResourceBuilder(resourceBuilder)
                    .AddRedisInstrumentation()
                    .AddRabbitMQInstrumentation()
                    .AddGrpcClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Protocol = exporterProtocol;
                        options.Endpoint = new Uri(exporterTracesEndpoint);
                    })
                );

            builder.Logging.AddOpenTelemetry(options =>
            {
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
                options.ParseStateValues = true;
                options
                    .SetResourceBuilder(resourceBuilder)
                    .AttachLogsToActivityEvent()
                    .AddOtlpExporter(exporterOptions =>
                    {
                        exporterOptions.Protocol = exporterProtocol;
                        exporterOptions.Endpoint = new Uri(exporterLogsEndpoint!);
                    });
            });

            return builder;
        }
    }
}
