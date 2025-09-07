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
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddGrpc();

        builder.Services.AddCustomHealthChecks(builder.Configuration);

        builder.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
        });

        builder.Services
            .AddDomain()
            .AddApplication();

        builder.AddInfrastructure();

        var app = builder.Build();

        app.UseHttpsRedirection();

        app.MapEndpoints()
            .MapGrpcServices()
            .UseCustomHealthChecks()
            .UseResponseCompression()
            .UseMiddleware<ExceptionHandlingMiddleware>();

        await app.RunAsync();
    }
}
