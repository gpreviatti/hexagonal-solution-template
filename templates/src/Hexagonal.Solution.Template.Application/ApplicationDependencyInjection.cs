using FluentValidation;
using Hexagonal.Solution.Template.Application.Orders.Create;
using Microsoft.Extensions.DependencyInjection;

namespace Hexagonal.Solution.Template.Application;
public static class ApplicationDependencyInjection
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        // Add Validators from assembly
        services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();

        // Orders
        services
            .AddScoped<ICreateOrderUseCase, CreateOrderUseCase>();
    }

}
