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
        var exporterProtocol = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_PROTOCOL")?.ToLower() == "grpc"
            ? OtlpExportProtocol.Grpc
            : OtlpExportProtocol.HttpProtobuf;

        builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource.AddEnvironmentVariableDetector())
        .WithMetrics(metrics => metrics
            .AddAspNetCoreInstrumentation()
            .AddMeter(DefaultConfigurations.Meter.Name)
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(options =>
            {
                options.Protocol = exporterProtocol;
                options.Endpoint = new Uri(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_METRICS_ENDPOINT")!);
            })
        )
        .WithTracing(tracing => tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation(
                options =>
                {
                    options.RecordException = true;
                }
            )
            .AddEntityFrameworkCoreInstrumentation(
                options =>
                {
                    options.SetDbStatementForText = true;
                    options.SetDbStatementForStoredProcedure = true;
                }
            )
            .AddRedisInstrumentation()
            .AddConsoleExporter()
            .AddOtlpExporter(options =>
            {
                options.Protocol = exporterProtocol;
                options.Endpoint = new Uri(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_TRACES_ENDPOINT")!);
            })
        );

        builder.Services.AddLogging(logging => logging.AddOpenTelemetry(openTelemetryLoggerOptions =>
        {
            openTelemetryLoggerOptions.IncludeScopes = true;
            openTelemetryLoggerOptions.IncludeFormattedMessage = true;
            openTelemetryLoggerOptions.AddOtlpExporter(options =>
            {
                options.Protocol = exporterProtocol;
                options.Endpoint = new Uri(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_LOGS_ENDPOINT")!);
            });
        }));

        return builder;
    }
}
