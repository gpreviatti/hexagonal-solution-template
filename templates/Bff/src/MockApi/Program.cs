using MockApi.Endpoints;
using MockApi.Extensions;
using MockApi.GrpcServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddResponseCompression();
builder.Services.AddCustomHealthChecks();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapEndpoints()
    .MapGrpcServices()
    .UseResponseCompression()
    .UseCustomHealthChecks();

await app.RunAsync();
