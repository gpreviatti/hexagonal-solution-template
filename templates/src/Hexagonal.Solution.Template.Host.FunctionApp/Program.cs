using Hexagonal.Solution.Template.Application;
using Hexagonal.Solution.Template.Domain;
using Hexagonal.Solution.Template.Infrastructure.Data;
using Hexagonal.Solution.Template.Infrastructure.Log;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Hexagonal.Solution.Template.Host.FunctionApp;

sealed class Program
{
    private static void Main(string[] args)
    {
        var host = new HostBuilder()
        .ConfigureFunctionsWorkerDefaults()
        .ConfigureServices((context, services) =>
        {
            services
                .AddDomainServices()
                .AddApplicationServices()
                .AddInfrastructureLogServices()
                .AddInfrastructureDataServices(context.Configuration);

            services.AddApplicationInsightsTelemetryWorkerService();
            services.ConfigureFunctionsApplicationInsights();
        })
        .Build();

        host.Run();
    }
}