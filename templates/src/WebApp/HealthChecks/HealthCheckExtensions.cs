using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WebApp.HealthChecks;

internal static class HealthCheckExtensions
{
    public static IServiceCollection AddCustomHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy())
            .AddSqlServer(
                configuration.GetConnectionString("OrderDb")!,
                name: "SqlServer",
                tags: ["services"]
            )
            .AddRedis(
                configuration.GetConnectionString("RedisConnectionString")!,
                name: "Redis",
                tags: ["services"]
            );

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