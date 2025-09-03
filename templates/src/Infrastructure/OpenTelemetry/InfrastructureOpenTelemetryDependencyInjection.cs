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
        var configuration = builder.Configuration;
        var provider = configuration["OpenTelemetry:Provider"]!.ToLowerInvariant();

        var openTelemetryBaseEndpoint = configuration["OpenTelemetry:Endpoint"]!;
        var openTelemetryLogsEndpoint = openTelemetryBaseEndpoint;
        var openTelemetryTracesEndpoint = openTelemetryBaseEndpoint;

        if (provider == "seq")
        {
            openTelemetryLogsEndpoint = openTelemetryBaseEndpoint + "logs";
            openTelemetryTracesEndpoint = openTelemetryBaseEndpoint + "traces";
        }

        if (string.IsNullOrWhiteSpace(openTelemetryBaseEndpoint.ToString()))
        {
            throw new ArgumentNullException("OpenTelemetry:Endpoint configuration is missing.");
        }

        var openTelemetryProtocol = configuration["OpenTelemetry:Protocol"]!.ToLowerInvariant() switch
        {
            "http/protobuf" => OtlpExportProtocol.HttpProtobuf,
            "grpc" => OtlpExportProtocol.Grpc,
            _ => OtlpExportProtocol.Grpc
        };

        var openTelemetryHeaders = configuration["OpenTelemetry:Headers"]!;

        builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource.AddService(DefaultConfigurations.ApplicationName))
        .WithMetrics(metrics =>
        {
            metrics
            .AddConsoleExporter()
            .AddAspNetCoreInstrumentation()
            .AddMeter(DefaultConfigurations.Meter.Name)
            .AddHttpClientInstrumentation();
            metrics.AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(openTelemetryBaseEndpoint);
                options.Protocol = openTelemetryProtocol;
                options.Headers = openTelemetryHeaders;
            });
        })
        .WithTracing(tracing =>
        {
            tracing
                .AddConsoleExporter()
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation(options =>
                {
                    options.RecordException = true;
                })
                .AddEntityFrameworkCoreInstrumentation(options =>
                {
                    options.SetDbStatementForText = true;
                    options.SetDbStatementForStoredProcedure = true;
                })
                .AddRedisInstrumentation()
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(openTelemetryTracesEndpoint);
                    options.Protocol = openTelemetryProtocol;
                    options.Headers = openTelemetryHeaders;
                });
        });

        builder.Services.AddLogging(logging => logging.AddOpenTelemetry(openTelemetryLoggerOptions =>
        {
            openTelemetryLoggerOptions.SetResourceBuilder(ResourceBuilder.CreateEmpty()
            .AddService(DefaultConfigurations.ApplicationName)
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
            }));

            openTelemetryLoggerOptions.IncludeScopes = true;
            openTelemetryLoggerOptions.IncludeFormattedMessage = true;

            openTelemetryLoggerOptions.AddOtlpExporter(exporter =>
            {
                exporter.Endpoint = new Uri(openTelemetryLogsEndpoint);
                exporter.Protocol = openTelemetryProtocol;
                exporter.Headers = openTelemetryHeaders;
            });
        }));


        return builder;
    }
}
