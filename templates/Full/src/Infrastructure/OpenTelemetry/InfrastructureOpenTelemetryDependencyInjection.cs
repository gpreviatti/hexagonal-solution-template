using Application.Common.Constants;
using Grafana.OpenTelemetry;
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

            builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddEnvironmentVariableDetector())
            .WithMetrics(metrics =>
            {
                metrics.AddMeter(
                    DefaultConfigurations.Meter.Name,
                    "System.Diagnostics.Metrics",
                    "Microsoft.AspNetCore.Hosting",
                    "Microsoft.AspNetCore.Server.Kestrel",
                    "System.Net.Http"
                );

                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddSqlClientInstrumentation()
                    .AddPrometheusExporter()
                    .AddOtlpExporter(options =>
                    {
                        options.Protocol = exporterProtocol;
                        options.Endpoint = new Uri(exporterMetricsEndpoint);
                    })
                    .UseGrafana();
            }
            )
            .WithTracing(tracing => tracing
                .AddSqlClientInstrumentation()
                .AddRedisInstrumentation()
                .AddRabbitMQInstrumentation()
                .AddGrpcClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .UseGrafana()
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

            builder.Services.AddLogging(logging => logging.AddOpenTelemetry(options =>
            {
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
                options.ParseStateValues = true;
                options.AttachLogsToActivityEvent();
                options.UseGrafana();
            }));

            return builder;
        }
    }
}
