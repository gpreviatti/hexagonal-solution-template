using System.Text.Json;
using Application.Common.Messages;
using Application.Common.Services;
using Application.Common.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Application.Common.Constants;

namespace Infrastructure.Messaging.Producers;

public sealed class ProducerService : IProduceService
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

    public async Task HandleAsync<TMessage>(
        TMessage message,
        CancellationToken cancellationToken,
        string queue = "",
        string exchange = ""
    ) where TMessage : BaseMessage
    {
        await Task.Yield();

        using var activity = DefaultConfigurations.ActivitySource.StartActivity($"{_className}.{nameof(HandleAsync)}")!;

        using var connection = await _factory.CreateConnectionAsync(cancellationToken);
        using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        Logs.DebugStartingOperation(_logger, _className, nameof(HandleAsync), message.CorrelationId, typeof(TMessage).Name + " publishing started.");

        await channel.BasicPublishAsync(
            exchange: exchange,
            routingKey: queue,
            body: JsonSerializer.SerializeToUtf8Bytes(message),
            cancellationToken: cancellationToken
        );

        Logs.DebugFinishedOperation(_logger, _className, nameof(HandleAsync), message.CorrelationId, typeof(TMessage).Name + " published.");

        activity?.SetTag("correlationId", message.CorrelationId);
    }

    public async Task HandleAsync<TMessage>(
        IEnumerable<TMessage> messages,
        CancellationToken cancellationToken,
        string queue = "",
        string exchange = ""
    ) where TMessage : BaseMessage
    {
        await Task.Yield();

        using var activity = DefaultConfigurations.ActivitySource.StartActivity($"{_className}.{nameof(HandleAsync)}")!;

        Logs.Debug(_logger, _className, nameof(HandleAsync), messages.FirstOrDefault()?.CorrelationId ?? Guid.Empty, typeof(TMessage).Name + " batch publishing started.");

        using var connection = await _factory.CreateConnectionAsync(cancellationToken);
        using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        foreach (var message in messages)
        {
            Logs.DebugStartingOperation(_logger, _className, nameof(HandleAsync), message.CorrelationId, typeof(TMessage).Name + " batch publishing started.");

            await channel.BasicPublishAsync(
                exchange: exchange,
                routingKey: queue,
                body: JsonSerializer.SerializeToUtf8Bytes(message),
                cancellationToken: cancellationToken
            );

            Logs.DebugFinishedOperation(_logger, _className, nameof(HandleAsync), message.CorrelationId, typeof(TMessage).Name + " batch published.");
            activity?.SetTag("correlationId", message.CorrelationId);
        }

        Logs.Debug(_logger, _className, nameof(HandleAsync), messages.FirstOrDefault()?.CorrelationId ?? Guid.Empty, typeof(TMessage).Name + " batch publishing finished.");

        activity?.SetTag("correlationId", messages.FirstOrDefault()?.CorrelationId ?? Guid.Empty);
    }
}
