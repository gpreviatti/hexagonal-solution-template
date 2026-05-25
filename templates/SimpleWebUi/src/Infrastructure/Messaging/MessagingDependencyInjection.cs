using System.Threading.Channels;
using Core.Common.Messages;
using Core.Common.Services;
using Infrastructure.Messaging.Consumers;
using Infrastructure.Messaging.Producers;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Messaging;

internal static class MessagingDependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddMessaging() => services
            .AddChannels()
            .AddProducers()
            .AddConsumers();
        IServiceCollection AddChannels() => services
            .AddSingleton(_ => Channel.CreateUnbounded<CreateNotificationMessage>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            }));

        IServiceCollection AddProducers() => services.AddScoped<IProduceService, ProducerService>();

        IServiceCollection AddConsumers() => services
            .AddHostedService<CreateNotificationConsumer>();
    }
}
