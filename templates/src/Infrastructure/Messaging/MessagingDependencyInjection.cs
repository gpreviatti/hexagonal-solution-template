using Application.Common.Services;
using Infrastructure.Messaging.Producers;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Messaging;

internal static class MessagingDependencyInjection
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumers(typeof(MessagingDependencyInjection).Assembly);

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration.GetConnectionString("RabbitMq") ?? throw new NullReferenceException("RabbitMq connection string is not configured."));
                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<IProduceService, ProducerService>();

        return services;
    }
}