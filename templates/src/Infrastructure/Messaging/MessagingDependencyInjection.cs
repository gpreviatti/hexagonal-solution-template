using System.Reflection;
using Application.Common.Services;
using Infrastructure.Messaging.Producers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Messaging;

internal static class MessagingDependencyInjection
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IProduceService, ProducerService>();
        
        RegisterConsumers(services);

        return services;
    }

    private static void RegisterConsumers(IServiceCollection services)
    {
        var infrastructureAssembly = typeof(MessagingDependencyInjection).Assembly;

        var consumerTypes = infrastructureAssembly.GetTypes()
            .Where(t => t.IsClass 
                && !t.IsAbstract 
                && t.Namespace?.Contains("Messaging.Consumers") == true
                && typeof(IHostedService).IsAssignableFrom(t))
            .ToList();

        foreach (var consumerType in consumerTypes)
        {
            var addHostedServiceMethod = typeof(ServiceCollectionHostedServiceExtensions)
                .GetMethods()
                .First(m => m.Name == nameof(ServiceCollectionHostedServiceExtensions.AddHostedService) 
                    && m.IsGenericMethodDefinition 
                    && m.GetGenericArguments().Length == 1
                    && m.GetParameters().Length == 1
                )
                .MakeGenericMethod(consumerType);

            addHostedServiceMethod.Invoke(null, [services]);
        }
    }
}