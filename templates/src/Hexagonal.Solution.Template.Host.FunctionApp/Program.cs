using Hexagonal.Solution.Template.Application;
using Hexagonal.Solution.Template.Domain;
using Hexagonal.Solution.Template.Infrastructure.Data;
using Hexagonal.Solution.Template.Infrastructure.Log;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Hexagonal.Solution.Template.Host.FunctionApp;

public sealed class Program
{
    private static void Main(string[] args)
    {
        var host = new HostBuilder()
        .ConfigureFunctionsWorkerDefaults()
        .ConfigureServices((context, services) =>
        {
            AddDependencies(services, context.Configuration);

            services.AddApplicationInsightsTelemetryWorkerService();
            services.ConfigureFunctionsApplicationInsights();
        })
        .Build();

        host.Run();
    }

    public static void AddDependencies(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddDomainServices()
            .AddApplicationServices()
            .AddInfrastructureLogServices()
            .AddInfrastructureDataServices(configuration);
    }
}