using Domain.Orders.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Domain;
public static class DomainDependencyInjection
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        // Orders
        services.AddScoped<ICreateOrderService, CreateOrderService>();

        return services;
    }
}