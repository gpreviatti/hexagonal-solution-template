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
    public static WebApplicationBuilder AddOpenTelemetry(this WebApplicationBuilder builder)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var exporterProtocol = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_PROTOCOL")?.ToLower() == "grpc"
            ? OtlpExportProtocol.Grpc
            : OtlpExportProtocol.HttpProtobuf;
        var exporterMetricsEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_METRICS_ENDPOINT");
        var exporterTracesEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_TRACES_ENDPOINT");
        var exporterLogsEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_LOGS_ENDPOINT");

        if (
            string.Compare(environment, "IntegrationTests", true) == 0 ||
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
            .AddMeter(DefaultConfigurations.Meter.Name)
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
