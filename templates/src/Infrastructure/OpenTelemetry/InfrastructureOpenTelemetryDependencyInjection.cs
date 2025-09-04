using Application.Common.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Infrastructure.OpenTelemetry;

internal static class InfrastructureOpenTelemetryDependencyInjection
{
    public static WebApplicationBuilder AddOpenTelemetry(this WebApplicationBuilder builder)
    {
        builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource.AddEnvironmentVariableDetector())
        .WithMetrics(metrics => metrics
            .AddAspNetCoreInstrumentation()
            .AddMeter(DefaultConfigurations.Meter.Name)
            .AddHttpClientInstrumentation()
            .AddOtlpExporter()
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
            .AddOtlpExporter()
        )
        .WithLogging(logging => logging
            .AddConsoleExporter()
            .AddOtlpExporter());
        
        return builder;
    }
}
