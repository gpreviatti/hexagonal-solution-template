using Application;
using Domain;
using Infrastructure;
using WebApp.Endpoints;
using WebApp.GrpcServices;
using WebApp.HealthChecks;
using WebApp.Middlewares;

namespace WebApp;

public sealed class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddGrpc();

        builder.Services.AddCustomHealthChecks(builder.Configuration);

        builder.Services
            .AddDomain()
            .AddApplication();

        builder.AddInfrastructure();

        var app = builder.Build();

        app.UseHttpsRedirection();

        app.MapEndpoints();
        app.MapGrpcServices();

        app.UseCustomHealthChecks();

        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.Run();
    }
}
