using Hexagonal.Solution.Template.Application.Orders.Create;
using Hexagonal.Solution.Template.Infrastructure.Data.Orders.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hexagonal.Solution.Template.Infrastructure.Data;
public static class InfrastructureDataDependencyInjection
{
    public static void AddInfrastructureDataServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<MyDbContext>(
            context => context.UseSqlServer(configuration.GetConnectionString("MyDbContext"))
        );

        // Orders
        services.AddScoped<IOrderRepository, OrderRepository>();
    }
}
