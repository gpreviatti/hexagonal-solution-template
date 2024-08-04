using FluentValidation;
using Hexagonal.Solution.Template.Application.Common.Messages;
using Hexagonal.Solution.Template.Application.Orders.Create;
using Microsoft.Extensions.DependencyInjection;

namespace Hexagonal.Solution.Template.Application;
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
