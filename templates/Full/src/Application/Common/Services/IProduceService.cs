using Application.Common.Messages;

namespace Application.Common.Services;

public interface IProduceService
{
    Task HandleAsync<TMessage>(TMessage message, CancellationToken cancellationToken, string queue = "", string exchange = "") where TMessage : BaseMessage;
    Task HandleAsync<TMessage>(IEnumerable<TMessage> messages, CancellationToken cancellationToken, string queue = "", string exchange = "") where TMessage : BaseMessage;
}
