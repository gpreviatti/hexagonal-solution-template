using Application.Common.Services;
using Infrastructure.Messaging.Producers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Messaging;

internal static class MessagingDependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddMessaging()
        {
            services.AddScoped<IProduceService, ProducerService>();

            RegisterConsumers(services);

            return services;
        }
    }

    private static void RegisterConsumers(IServiceCollection services)
    {
        var infrastructureAssembly = typeof(MessagingDependencyInjection).Assembly;

        var consumerTypes = infrastructureAssembly.GetTypes()
            .Where(t => t.IsClass
                && !t.IsAbstract
                && t.Namespace?.Contains("Messaging.Consumers", StringComparison.Ordinal) == true
                && typeof(IHostedService).IsAssignableFrom(t))
            .ToList();

        foreach (var consumerType in consumerTypes)
        {
            var addHostedServiceMethod = typeof(ServiceCollectionHostedServiceExtensions)
                .GetMethods()
                .First(m => string.Equals(m.Name, nameof(ServiceCollectionHostedServiceExtensions.AddHostedService), StringComparison.Ordinal) && m.IsGenericMethodDefinition
                    && m.GetGenericArguments().Length == 1
                    && m.GetParameters().Length == 1
                )
                .MakeGenericMethod(consumerType);

            addHostedServiceMethod.Invoke(null, [services]);
        }
    }
}
