namespace Application.Common.Services;

public interface IProduceService
{
    ValueTask HandleAsync<TMessage>(TMessage message, CancellationToken cancellationToken);
    ValueTask HandleAsync<TMessage>(IEnumerable<TMessage> message, CancellationToken cancellationToken);
}
