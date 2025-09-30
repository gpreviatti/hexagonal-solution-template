using Application.Common.Messages;

namespace Application.Common.Services;

public interface IProduceService
{
    ValueTask HandleAsync<TMessage>(TMessage message, CancellationToken cancellationToken) where TMessage : BaseMessage;
    ValueTask HandleAsync<TMessage>(IEnumerable<TMessage> message, CancellationToken cancellationToken) where TMessage : BaseMessage;
}
