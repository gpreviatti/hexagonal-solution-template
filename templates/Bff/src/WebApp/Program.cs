using System.Text.Json;
using System.Text.Json.Serialization;
using Infrastructure;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Scalar.AspNetCore;
using WebApp.Endpoints;
using WebApp.Extensions;
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
        builder.Services.AddOpenApi();
        builder.Services.AddCustomHealthChecks(builder.Configuration);
        builder.Services.AddRateLimiting(builder.Configuration);
        builder.Services.AddResponseCompression(
            options =>
            {
                options.EnableForHttps = true;
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes;
                options.Providers.Add<GzipCompressionProvider>();
            }
        );
        builder.Services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.SerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
            options.SerializerOptions.PropertyNameCaseInsensitive = true;
        });

        builder.AddInfrastructure();

        builder.WebHost.ConfigureKestrel(options =>
            options.ConfigureEndpointDefaults(listenOptions =>
                listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3
        ));

        var app = builder.Build();

        app.MapOpenApi();
        app.MapScalarApiReference();

        app.UseHttpsRedirection();

        app.UseRateLimiter();

        app.MapEndpoints()
            .UseCustomHealthChecks()
            .UseResponseCompression()
            .UseMiddleware<ExceptionHandlingMiddleware>();

        await app.RunAsync();
    }
}
