using Application.Common.Messages;
using Application.Orders.Create;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;
public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        //Add validators from assembly
        services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();

        // Orders
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(BaseResponse).Assembly));

        return services;
    }
}