using System.Diagnostics;
using Application.Common.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Messaging.Consumers;

public abstract class BaseConsumer<TMessage, TConsumer>(ILogger<BaseConsumer<TMessage, TConsumer>> logger) : IConsumer<TMessage> where TMessage : BaseMessage where TConsumer : BaseConsumer<TMessage, TConsumer>
{
    private readonly string _className = typeof(TConsumer).Name;

    public async Task Consume(ConsumeContext<TMessage> context)
    {
        var stopWatch = Stopwatch.StartNew();

        logger.LogInformation("[{ClassName}] | [Consume] | CorrelationId: {CorrelationId} | Received message: {MessageType}", _className, context.Message.CorrelationId, typeof(TMessage).Name);

        await HandleMessageAsync(context.Message, context.CancellationToken);

        logger.LogInformation(
            "[{ClassName}] | [Consume] | CorrelationId: {CorrelationId} | Message processed: {MessageType} | Elapsed time: {ElapsedMilliseconds} ms",
            _className, context.Message.CorrelationId, typeof(TMessage).Name, stopWatch.ElapsedMilliseconds
        );
    }

    protected abstract Task HandleMessageAsync(TMessage message, CancellationToken cancellationToken);
}