using System.Diagnostics;
using System.Text.Json;
using Application.Common.Messages;
using Application.Common.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Infrastructure.Messaging.Producers;

public class ProducerService : IProduceService
{
    private readonly string _className = nameof(ProducerService);
    private readonly ILogger<ProducerService> _logger;
    private readonly ConnectionFactory _factory;

    public ProducerService(ILogger<ProducerService> logger, IConfiguration configuration)
    {
        _logger = logger;

        var connectionString = configuration.GetConnectionString("RabbitMQ");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Invalid RabbitMQ connection string.");
        }
        _factory = new() { Uri = new(connectionString) };
    }

    public async ValueTask HandleAsync<TMessage>(
        TMessage message,
        CancellationToken cancellationToken,
        string queue = "",
        string exchange = ""
    ) where TMessage : BaseMessage
    {
        var stopWatch = Stopwatch.StartNew();

        using var connection = await _factory.CreateConnectionAsync(cancellationToken);
        using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: queue,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken
        );

        _logger.LogInformation(
            "[{ClassName}] | [HandleAsync] | CorrelationId: {CorrelationId} | Publishing message: {MessageType}",
            _className, message.CorrelationId, typeof(TMessage).Name
        );

        await channel.BasicPublishAsync(
            exchange: queue,
            routingKey: queue,
            mandatory: true,
            body: JsonSerializer.SerializeToUtf8Bytes(message),
            cancellationToken: cancellationToken
        );

        _logger.LogInformation(
            "[{ClassName}] | [HandleAsync] | CorrelationId: {CorrelationId} | Message published: {MessageType} | Elapsed time: {ElapsedMilliseconds} ms",
            _className, message.CorrelationId, typeof(TMessage).Name, stopWatch.ElapsedMilliseconds
        );
    }

    public async ValueTask HandleAsync<TMessage>(
        IEnumerable<TMessage> messages,
        CancellationToken cancellationToken,
        string queue = "",
        string exchange = ""
    ) where TMessage : BaseMessage
    {
        var stopWatch = Stopwatch.StartNew();

        using var connection = await _factory.CreateConnectionAsync(cancellationToken);
        using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: queue,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken
        );

        _logger.LogInformation(
            "[{ClassName}] | [HandleAsync] | CorrelationId: {CorrelationId} | Publishing batch of messages: {MessageType}",
            _className, messages.Select(m => m.CorrelationId), typeof(TMessage).Name
        );

        foreach (var message in messages)
            await channel.BasicPublishAsync(
                exchange: exchange,
                routingKey: queue,
                mandatory: true,
                body: JsonSerializer.SerializeToUtf8Bytes(message),
                cancellationToken: cancellationToken
            );

        _logger.LogInformation(
            "[{ClassName}] | [HandleAsync] | CorrelationId: {CorrelationId} | Batch of messages published: {MessageType} | Elapsed time: {ElapsedMilliseconds} ms",
             _className, messages.Select(m => m.CorrelationId), typeof(TMessage).Name, stopWatch.ElapsedMilliseconds
        );
    }
}
