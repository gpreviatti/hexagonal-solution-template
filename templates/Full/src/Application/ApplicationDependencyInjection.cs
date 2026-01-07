using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Application.Common.UseCases;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

[ExcludeFromCodeCoverage]
public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var applicationAssembly = typeof(ApplicationDependencyInjection).Assembly;
        services.AddValidatorsFromAssembly(applicationAssembly);

        RegisterUseCases(services, applicationAssembly);

        return services;
    }

    private static void RegisterUseCases(IServiceCollection services, Assembly applicationAssembly)
    {
        var useCaseTypes = applicationAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("UseCase"))
            .ToList();

        foreach (var useCaseType in useCaseTypes)
        {
            var baseType = useCaseType.BaseType;

            if (baseType is null || !baseType.IsGenericType)
                continue;

            var baseTypeDefinition = baseType.GetGenericTypeDefinition();

            if (baseTypeDefinition == typeof(BaseInOutUseCase<,>))
            {
                var genericArgs = baseType.GetGenericArguments();
                var interfaceType = typeof(IBaseInOutUseCase<,>).MakeGenericType(genericArgs);
                services.AddScoped(interfaceType, useCaseType);
            }

            else if (baseTypeDefinition == typeof(BaseInUseCase<>))
            {
                var genericArgs = baseType.GetGenericArguments();
                var interfaceType = typeof(IBaseInUseCase<>).MakeGenericType(genericArgs);
                services.AddScoped(interfaceType, useCaseType);
            }

            else if (baseTypeDefinition == typeof(BaseOutUseCase<>))
            {
                var genericArgs = baseType.GetGenericArguments();
                var interfaceType = typeof(IBaseOutUseCase<>).MakeGenericType(genericArgs);
                services.AddScoped(interfaceType, useCaseType);
            }
        }
    }
}
