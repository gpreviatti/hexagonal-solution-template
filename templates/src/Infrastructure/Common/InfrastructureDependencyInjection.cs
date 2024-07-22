using Infrastructure.Data;
using Infrastructure.Logs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection;
public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddInfrastructureLogServices()
            .AddInfrastructureDataServices(configuration);

        return services;
    }
}
