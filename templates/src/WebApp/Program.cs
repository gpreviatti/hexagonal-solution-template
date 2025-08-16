using Application;
using Domain;
using Infrastructure;
using Infrastructure.OpenTelemetry;
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
            .AddDomain()
            .AddApplication();

        builder.AddInfrastructure();

        var app = builder.Build();

        app.UseHttpsRedirection();

        app.MapOrderEndpoints();

        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.Run();
    }
}
