using Hexagonal.Solution.Template.Application;
using Hexagonal.Solution.Template.Domain;
using Hexagonal.Solution.Template.Infrastructure.Data;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {

        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(context.Configuration)
            .CreateLogger();

        services.AddSingleton<ILogger>(logger);

        services
            .AddDomainServices()
            .AddApplicationServices()
            .AddInfrastructureDataServices(context.Configuration);

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();
