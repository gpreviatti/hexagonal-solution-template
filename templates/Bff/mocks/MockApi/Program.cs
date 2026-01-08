using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using MockApi.Endpoints;
using MockApi.GrpcServices;

namespace MockApi;

public sealed class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddGrpc();

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

        builder.WebHost.ConfigureKestrel(options =>
            options.ConfigureEndpointDefaults(listenOptions =>
                listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3
        ));

        var app = builder.Build();

        app.UseHttpsRedirection();

        app.MapEndpoints()
            .MapGrpcServices()
            .UseResponseCompression();

        await app.RunAsync();
    }
}
