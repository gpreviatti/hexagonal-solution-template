using Application;
using Domain;
using HealthChecks.UI.Client;
using Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using WebApp.Endpoints;
using WebApp.HealthChecks;
using WebApp.Middlewares;

namespace WebApp;

public sealed class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddCustomHealthChecks(builder.Configuration);

        builder.Services
            .AddDomain()
            .AddApplication();

        builder.AddInfrastructure();

        var app = builder.Build();

        app.UseHttpsRedirection();

        app.MapOrderEndpoints();

        app.UseCustomHealthChecks();

        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.Run();
    }
}
