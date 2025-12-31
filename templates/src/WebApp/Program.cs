using System.Text.Json;
using System.Text.Json.Serialization;
using Application;
using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using WebApp.Endpoints;
using WebApp.GrpcServices;
using WebApp.HealthChecks;
using WebApp.Middlewares;

namespace WebApp;

#pragma warning disable S1118 // Utility classes should not have public constructors
public sealed class Program
#pragma warning restore S1118 // Utility classes should not have public constructors
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

        builder.Services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.SerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
            options.SerializerOptions.PropertyNameCaseInsensitive = true;
        });

        builder.Services
            .AddDomain()
            .AddApplication();

        builder.AddInfrastructure();

        builder.WebHost.ConfigureKestrel(options =>
            options.ConfigureEndpointDefaults(listenOptions =>
                listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3
        ));

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
