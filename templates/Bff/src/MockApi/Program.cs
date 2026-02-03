using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using MockApi.Endpoints;
using MockApi.Extensions;
using MockApi.GrpcServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddResponseCompression(
    options =>
    {
        options.EnableForHttps = true;
        options.MimeTypes = ResponseCompressionDefaults.MimeTypes;
        options.Providers.Add<GzipCompressionProvider>();
    }
);
builder.Services.AddCustomHealthChecks();

builder.WebHost.ConfigureKestrel(options => options
    .ConfigureEndpointDefaults(listenOptions => listenOptions.Protocols = HttpProtocols.Http2
));

var app = builder.Build();

app.UseHttpsRedirection();

app.MapEndpoints()
    .MapGrpcServices()
    .UseResponseCompression()
    .UseCustomHealthChecks();

await app.RunAsync();
