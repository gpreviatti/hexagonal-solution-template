using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MockApi.Endpoints;
using MockApi.GrpcServices;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

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
builder.Services
    .AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy());

builder.WebHost.ConfigureKestrel(options => options
    .ConfigureEndpointDefaults(listenOptions => listenOptions.Protocols = HttpProtocols.Http2
));

var serviceName = "Hexagonal.Solution.Template.Bff.MockApi";
var serviceVersion = typeof(Program).Assembly.GetName().Version!.ToString();
var resourceBuilder = ResourceBuilder
    .CreateDefault()
    .AddService(serviceName, serviceVersion: serviceVersion);

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .SetResourceBuilder(resourceBuilder)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
        .AddOtlpExporter()
    )
    .WithTracing(tracing => tracing
        .AddSource(serviceName)
        .SetResourceBuilder(resourceBuilder)
        .AddGrpcClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter()
    );

builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeFormattedMessage = true;
    options.IncludeScopes = true;
    options.ParseStateValues = true;
    options
        .SetResourceBuilder(resourceBuilder)
        .AttachLogsToActivityEvent()
        .AddOtlpExporter();
});

var app = builder.Build();

app.UseHttpsRedirection();

app.MapEndpoints()
    .MapGrpcServices()
    .UseResponseCompression();

app.UseHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseHealthChecks("/live", new HealthCheckOptions
{
    Predicate = r => r.Name.Contains("self")
});

await app.RunAsync();
