using Microsoft.AspNetCore.Server.Kestrel.Core;
using MockApi.Endpoints;
using MockApi.GrpcServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddResponseCompression();

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
