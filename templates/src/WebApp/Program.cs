using Application;
using Domain;
using Infrastructure;
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
            .AddApplication()
            .AddInfrastructure(builder.Configuration);

        var app = builder.Build();

        app.UseHttpsRedirection();

        app.MapOrderEndpoints();

        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.Run();
    }
}
