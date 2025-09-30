using System.Diagnostics;
using Application.Common.Messages;
using Application.Common.Services;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Messaging.Producers;

public class ProducerService(IBus bus, ILogger<ProducerService> logger) : IProduceService
{
    private readonly string _className = nameof(ProducerService);

    public async ValueTask HandleAsync<TMessage>(TMessage message, CancellationToken cancellationToken) where TMessage : BaseMessage
    {
        var stopWatch = Stopwatch.StartNew();
        
        logger.LogInformation(
            "[{ClassName}] | [HandleAsync] | CorrelationId: {CorrelationId} | Publishing message: {MessageType}",
            _className, message.CorrelationId, typeof(TMessage).Name
        );

        await bus.Publish(message, cancellationToken);

        logger.LogInformation(
            "[{ClassName}] | [HandleAsync] | CorrelationId: {CorrelationId} | Message published: {MessageType} | Elapsed time: {ElapsedMilliseconds} ms",
            _className, message.CorrelationId, typeof(TMessage).Name, stopWatch.ElapsedMilliseconds
        );
    }

    public async ValueTask HandleAsync<TMessage>(IEnumerable<TMessage> messages, CancellationToken cancellationToken) where TMessage : BaseMessage
    {
        var stopWatch = Stopwatch.StartNew();

        logger.LogInformation(
            "[{ClassName}] | [HandleAsync] | CorrelationId: {CorrelationId} | Publishing batch of messages: {MessageType}",
            _className, messages.Select(m => m.CorrelationId), typeof(TMessage).Name
        );

        await bus.PublishBatch(messages, cancellationToken);

        logger.LogInformation(
            "[{ClassName}] | [HandleAsync] | CorrelationId: {CorrelationId} | Batch of messages published: {MessageType} | Elapsed time: {ElapsedMilliseconds} ms",
             _className, messages.Select(m => m.CorrelationId), typeof(TMessage).Name, stopWatch.ElapsedMilliseconds
        );
    }
}
