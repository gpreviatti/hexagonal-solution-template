using Application.Common.Constants;
using Application.Common.Repositories;
using Domain.Orders;
using Infrastructure.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Data;
internal static class InfrastructureDataDependencyInjection
{
    public static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddDbContextPool<MyDbContext>(context =>
                context.UseSqlServer(configuration.GetConnectionString("OrderDb"))
            );

        services.AddScoped<IBaseRepository<Order>, BaseRepository<Order>>();

        return services;
    }
}
