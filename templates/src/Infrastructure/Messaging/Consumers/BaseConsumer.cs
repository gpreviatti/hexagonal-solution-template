using System.Diagnostics;
using System.Text.Json;
using Application.Common.Messages;
using Application.Common.Services;
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
    private readonly IDictionary<string, object?> _arguments;
    private readonly ConnectionFactory _factory;
    protected IServiceScopeFactory serviceScopeFactory;

    public BaseConsumer(
        ILogger<BaseConsumer<TMessage, TConsumer>> logger,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        string queueName,
        IDictionary<string, object?> arguments = null!
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
        _arguments = arguments;
        _factory = new() { Uri = new(connectionString) };
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var connection = await _factory.CreateConnectionAsync(cancellationToken);
        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: _queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: _arguments,
            cancellationToken: cancellationToken
        );

        var consumer = new AsyncEventingBasicConsumer(channel);
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        consumer.ReceivedAsync += async (model, eventArguments) =>
        {
            var body = eventArguments.Body.ToArray();
            var message = JsonSerializer.Deserialize<TMessage>(body);
            try
            {
                var stopWatch = Stopwatch.StartNew();
                if (message == null || message.GetType() != typeof(TMessage))
                {
                    _logger.LogDebug(
                        "[{ClassName}] | [HandleMessageAsync] | CorrelationId: {CorrelationId} | Received null message of type {MessageType}",
                        _className, eventArguments.BasicProperties.CorrelationId, typeof(TMessage).Name
                    );
                    return;
                }

                _logger.LogInformation(
                    "[{ClassName}] | [HandleMessageAsync] | CorrelationId: {CorrelationId} | Received message: {MessageType}",
                    _className, message.CorrelationId, typeof(TMessage).Name
                );

                var idempotency = await serviceProvider
                .GetRequiredService<IHybridCacheService>()
                .GetOrCreateAsync(
                    _className + "-" + message.CorrelationId,
                    async (cancellationToken) => true,
                    cancellationToken
                );
                if (idempotency)
                {
                    _logger.LogWarning(
                        "[{ClassName}] | [HandleMessageAsync] | CorrelationId: {CorrelationId} | Duplicate message detected. Skipping processing for message type {MessageType}",
                        _className, message.CorrelationId, typeof(TMessage).Name
                    );
                    return;
                }

                await HandleMessageAsync(serviceProvider, message, cancellationToken);

                _logger.LogInformation(
                    "[{ClassName}] | [HandleMessageAsync] | CorrelationId: {CorrelationId} | Processed message in {ElapsedMilliseconds} ms",
                    _className, message.CorrelationId, stopWatch.ElapsedMilliseconds
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "[{ClassName}] | [HandleMessageAsync] | CorrelationId: {CorrelationId} | Error processing message: {ErrorMessage}",
                    _className, eventArguments.BasicProperties.CorrelationId, ex.Message
                );

                await serviceProvider
                    .GetRequiredService<IProduceService>()
                    .HandleAsync(message!, cancellationToken, _queueName + "_deadLetter");

                throw;
            }
        };

        await channel.BasicConsumeAsync(
            queue: _queueName,
            autoAck: true,
            consumer: consumer,
            cancellationToken: cancellationToken
        );

        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    protected abstract Task HandleMessageAsync(IServiceProvider serviceProvider, TMessage message, CancellationToken cancellationToken);
}