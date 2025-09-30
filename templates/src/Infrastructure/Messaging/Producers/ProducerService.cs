using Application.Common.Services;

namespace Infrastructure.Messaging.Producers;

public class ProducerService : IProduceService
{
    public async ValueTask HandleAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }

    public async ValueTask HandleAsync<TMessage>(IEnumerable<TMessage> messages, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}
