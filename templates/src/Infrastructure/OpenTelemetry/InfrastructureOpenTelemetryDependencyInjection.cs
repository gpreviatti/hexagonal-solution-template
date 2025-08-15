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

        var serviceName = configuration["ServiceName"];
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            throw new ArgumentNullException("ServiceName configuration is missing.");
        }

        var openTelemetryEndpoint = new Uri(configuration["OpenTelemetry:Endpoint"]!);
        if (string.IsNullOrWhiteSpace(openTelemetryEndpoint.ToString()))
        {
            throw new ArgumentNullException("OpenTelemetry:Endpoint configuration is missing.");
        }

        var openTelemetryProtocol = configuration["OpenTelemetry:Protocol"]!.ToLowerInvariant() switch
        {
            "http/protobuf" => OtlpExportProtocol.HttpProtobuf,
            "grpc" => OtlpExportProtocol.Grpc,
            _ => OtlpExportProtocol.Grpc
        };

        builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource.AddService(serviceName))
        .WithMetrics(metrics =>
        {
            metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();
            metrics.AddOtlpExporter(options =>
            {
                options.Endpoint = openTelemetryEndpoint;
                options.Protocol = openTelemetryProtocol;
            });
        })
        .WithTracing(tracing =>
        {
            tracing
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
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = openTelemetryEndpoint;
                    options.Protocol = openTelemetryProtocol;
                });
        });

        builder.Logging.AddOpenTelemetry(logging => logging.AddOtlpExporter(
            options =>
            {
                options.Endpoint = openTelemetryEndpoint;
                options.Protocol = openTelemetryProtocol;
            }
        ));
        
        return builder;
    }
}
