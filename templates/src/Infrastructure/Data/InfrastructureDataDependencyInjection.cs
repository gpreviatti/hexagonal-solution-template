using Application.Orders;
using Infrastructure.Data.Orders.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Data;
internal static class InfrastructureDataDependencyInjection
{
    public static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddDbContext<MyDbContext>(context => context.UseSqlServer(configuration.GetConnectionString("OrderDb")));

        services.AddScoped<IOrderRepository, OrderRepository>();

        return services;
    }
}
