using Application.Orders;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;
public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        //Add validators from assembly
        services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();

        // Orders
        services.AddScoped<IGetOrderUserCase, GetOrderUseCase>();
        services.AddScoped<ICreateOrderUseCase, CreateOrderUseCase>();

        return services;
    }
}
