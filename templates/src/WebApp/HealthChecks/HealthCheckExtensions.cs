using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;

namespace WebApp.HealthChecks;

internal static class HealthCheckExtensions
{
    public static IServiceCollection AddCustomHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddSingleton(sp =>
            {
                var factory = new ConnectionFactory
                {
                    Uri = new(configuration.GetConnectionString("RabbitMq")!),
                };
                return factory.CreateConnectionAsync().GetAwaiter().GetResult();
            })
            .AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy())
            .AddSqlServer(
                configuration.GetConnectionString("OrderDb")!,
                name: "SqlServer",
                tags: ["services"]
            )
            .AddRedis(
                configuration.GetConnectionString("Redis")!,
                name: "Redis",
                tags: ["services"]
            )
            .AddRabbitMQ(
                name: "RabbitMQ",
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