using System.Diagnostics;
using System.Text.Json;
using Application.Common.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Infrastructure.Messaging.Consumers;

public abstract class BaseConsumer<TMessage, TConsumer> : BackgroundService where TMessage : BaseMessage
{
    private readonly string _className = typeof(TConsumer).Name;
    private readonly ILogger<BaseConsumer<TMessage, TConsumer>> _logger;
    private readonly string _queueName;
    private readonly ConnectionFactory _factory;
    protected IServiceScopeFactory serviceScopeFactory;

    public BaseConsumer(
        ILogger<BaseConsumer<TMessage, TConsumer>> logger,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        string queueName
    )
    {
        _logger = logger;
        this.serviceScopeFactory = serviceScopeFactory;

        var connectionString = configuration.GetConnectionString("RabbitMQ");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Invalid RabbitMQ connection string.");
        }

        _queueName = queueName;
        _factory = new() { Uri = new(connectionString) };
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var stopWatch = Stopwatch.StartNew();

        using var connection = await _factory.CreateConnectionAsync(cancellationToken);
        using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: _queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken
        );

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (model, eventArguments) =>
        {
            var body = eventArguments.Body.ToArray();
            var message = JsonSerializer.Deserialize<TMessage>(body);
            if (message == null || message.GetType() != typeof(TMessage))
            {
                _logger.LogDebug(
                    "[{ClassName}] | [Consume] | CorrelationId: {CorrelationId} | Received null message of type {MessageType}",
                    _className, eventArguments.BasicProperties.CorrelationId, typeof(TMessage).Name
                );
                return;
            }

            _logger.LogInformation(
                "[{ClassName}] | [Consume] | CorrelationId: {CorrelationId} | Received message: {MessageType}",
                _className, message.CorrelationId, typeof(TMessage).Name
            );

            using IServiceScope scope = serviceScopeFactory.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            await HandleMessageAsync(serviceProvider, message, cancellationToken);

            _logger.LogInformation(
                "[{ClassName}] | [Consume] | CorrelationId: {CorrelationId} | Processed message in {ElapsedMilliseconds} ms",
                _className, message.CorrelationId, stopWatch.ElapsedMilliseconds
            );
        };

        await channel.BasicConsumeAsync(
            queue: _queueName,
            autoAck: true,
            consumer: consumer,
            cancellationToken: cancellationToken
        );
    }

    protected abstract Task HandleMessageAsync(IServiceProvider serviceProvider, TMessage message, CancellationToken cancellationToken);
}