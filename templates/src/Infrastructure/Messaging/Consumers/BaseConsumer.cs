using MassTransit;

namespace Infrastructure.Messaging.Consumers;

public abstract class BaseConsumer<TMessage> : IConsumer<TMessage> where TMessage : class
{
    public async Task Consume(ConsumeContext<TMessage> context)
    {
        await HandleMessageAsync(context.Message, context.CancellationToken);
    }

    protected abstract Task HandleMessageAsync(TMessage message, CancellationToken cancellationToken);
}