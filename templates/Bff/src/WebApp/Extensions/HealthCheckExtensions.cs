using HealthChecks.UI.Client;
using Infrastructure.Common;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WebApp.Extensions;

internal static class HealthCheckExtensions
{
    public static IServiceCollection AddCustomHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var healthChecksBuilder = services
            .AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy())
            .AddRedis(
                configuration.GetConnectionString("Redis")!,
                name: "Redis",
                tags: ["services"]
            );

        var serviceKeys = Enum.GetValues<ServicesKey>();
        foreach (var serviceKey in serviceKeys)
        {
            var baseAddress = configuration.GetSection("Http")
                .GetChildren()
                .FirstOrDefault(x => x["Name"] == serviceKey.ToString())?["BaseAddress"];

            if (!string.IsNullOrEmpty(baseAddress))
            {
                healthChecksBuilder.AddUrlGroup(
                    new Uri($"{baseAddress}/health"),
                    name: serviceKey.ToString(),
                    tags: ["services"]
                );
            }
        }

        return services;
    }

    public static IApplicationBuilder UseCustomHealthChecks(
        this IApplicationBuilder app
    )
    {
        app.UseHealthChecks("/health", new HealthCheckOptions()
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.UseHealthChecks("/live", new HealthCheckOptions
        {
            Predicate = r => r.Name.Contains("self")
        });

        app.UseHealthChecks("/ready", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("services")
        });

        return app;
    }
}
