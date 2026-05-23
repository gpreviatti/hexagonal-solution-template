using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Core.Common.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace Core;

[ExcludeFromCodeCoverage]
public static class CoreDependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        var CoreAssembly = typeof(CoreDependencyInjection).Assembly;

        RegisterUseCases(services, CoreAssembly);

        return services;
    }

    private static void RegisterUseCases(IServiceCollection services, Assembly CoreAssembly)
    {
        var useCaseTypes = CoreAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("UseCase", StringComparison.Ordinal))
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
