using Application.Common.Services;
using Infrastructure.Messaging.Consumers;
using Infrastructure.Messaging.Producers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Messaging;

internal static class MessagingDependencyInjection
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddScoped<IProduceService, ProducerService>();
        services.AddHostedService<CreateNotificationConsumer>();

        return services;
    }
}