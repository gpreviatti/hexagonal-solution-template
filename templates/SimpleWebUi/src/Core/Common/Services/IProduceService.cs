using Core.Common.Messages;

namespace Core.Common.Services;

public interface IProduceService
{
    Task HandleAsync<TMessage>(TMessage message, CancellationToken cancellationToken) where TMessage : BaseMessage;
    Task HandleAsync<TMessage>(IEnumerable<TMessage> message, CancellationToken cancellationToken) where TMessage : BaseMessage;
}
