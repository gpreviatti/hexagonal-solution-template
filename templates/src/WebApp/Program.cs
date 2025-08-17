using System.Net.Mime;
using System.Text.Json;
using Application;
using Domain;
using HealthChecks.UI.Client;
using Infrastructure;
using Infrastructure.Data;
using Infrastructure.OpenTelemetry;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using WebApp.Endpoints;
using WebApp.Middlewares;

namespace WebApp;

public sealed class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();

        builder.Services
            .AddHealthChecks()
            .AddSqlServer(builder.Configuration.GetConnectionString("OrderDb")!, name: "SqlServer")
            .AddRedis(builder.Configuration.GetConnectionString("RedisConnectionString")!, name: "Redis");

        builder.Services
            .AddDomain()
            .AddApplication();

        builder.AddInfrastructure();

        var app = builder.Build();

        app.UseHttpsRedirection();

        app.MapOrderEndpoints();

        app.UseHealthChecks("/health", new HealthCheckOptions()
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.Run();
    }
}
