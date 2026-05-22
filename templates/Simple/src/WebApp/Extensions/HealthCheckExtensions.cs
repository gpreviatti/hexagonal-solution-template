using HealthChecks.UI.Client;
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
        services
            .AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy())
            .AddNpgSql(
                configuration.GetConnectionString("OrderDb")!,
                name: "PostgreSQL",
                tags: ["services"]
            )
            .AddRedis(
                configuration.GetConnectionString("Redis")!,
                name: "Redis",
                tags: ["services"]
            );

        return services;
    }

    public static WebApplication UseCustomHealthChecks(
        this WebApplication app
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
