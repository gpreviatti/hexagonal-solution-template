using Application.Common.Constants;
using Application.Common.Repositories;
using Domain.Notifications;
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
            .AddDbContext<MyDbContext>(context =>
                context.UseSqlServer(configuration.GetConnectionString("OrderDb") ?? throw new NullReferenceException("OrderDb connection string is not configured."))
            );

        services.AddScoped<IBaseRepository<Order>, BaseRepository<Order>>();
        services.AddScoped<IBaseRepository<Notification>, BaseRepository<Notification>>();

        return services;
    }
}
