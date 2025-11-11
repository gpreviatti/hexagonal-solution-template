using System.Diagnostics;
using System.Text.Json;
using Application.Common.Messages;
using Application.Common.Services;
using Infrastructure.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Infrastructure.Messaging.Consumers;

internal abstract class BaseConsumer<TMessage, TConsumer> : BaseBackgroundService<BaseConsumer<TMessage, TConsumer>> where TMessage : BaseMessage
{
    private readonly string _className = typeof(TConsumer).Name;
    private readonly string _queueName;
    private readonly IDictionary<string, object?> _arguments;
    private readonly ConnectionFactory _factory;
    private readonly IProduceService _produceService;
    private readonly IHybridCacheService _hybridCacheService;

    public BaseConsumer(
        ILogger<BaseConsumer<TMessage, TConsumer>> logger,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        string queueName,
        IDictionary<string, object?> arguments = null!
    ) : base(logger, serviceScopeFactory, configuration)
    {
        _produceService = serviceProvider.GetRequiredService<IProduceService>();
        _hybridCacheService = serviceProvider.GetRequiredService<IHybridCacheService>();

        var connectionString = configuration.GetConnectionString("RabbitMQ");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Invalid RabbitMQ connection string.");
        }

        _queueName = queueName;
        _arguments = arguments;
        _factory = new() { Uri = new(connectionString) };
    }

    protected override async Task ExecuteInternalAsync(CancellationToken cancellationToken) => await HandleRabbitMqAsync(
        cancellationToken,
        async (message, cancellationToken) =>
        {
            try
            {
                var stopWatch = Stopwatch.StartNew();

                logger.LogInformation(
                    "[{ClassName}] | [HandleMessageAsync] | CorrelationId: {CorrelationId} | Received message: {MessageType}",
                    _className, message.CorrelationId, typeof(TMessage).Name
                );

                var isExecutedKey = _className + "-" + message.CorrelationId;
                var isExecuted = await _hybridCacheService.GetOrCreateAsync(
                    isExecutedKey,
                    async (cancellationToken) => false,
                    cancellationToken
                );

                if (isExecuted)
                {
                    logger.LogWarning(
                        "[{ClassName}] | [HandleMessageAsync] | CorrelationId: {CorrelationId} | Duplicate message detected. Skipping processing.",
                        _className, message.CorrelationId
                    );
                    return;
                }

                await HandleUseCaseAsync(serviceProvider, message, cancellationToken);

                await _hybridCacheService.CreateAsync(
                    isExecutedKey,
                    async (cancellationToken) => true,
                    cancellationToken
                );

                logger.LogInformation(
                    "[{ClassName}] | [HandleMessageAsync] | CorrelationId: {CorrelationId} | Processed message in {ElapsedMilliseconds} ms",
                    _className, message.CorrelationId, stopWatch.ElapsedMilliseconds
                );
            }
            catch (Exception ex)
            {
                logger.LogError(
                    "[{ClassName}] | [HandleMessageAsync] | CorrelationId: {CorrelationId} | Error processing message: {ErrorMessage}",
                    _className, message?.CorrelationId, ex.Message
                );

                await _produceService
                    .HandleAsync(message!, cancellationToken, _queueName + "_deadLetter");

                throw;
            }
        }
    );

    private async Task HandleRabbitMqAsync(
        CancellationToken cancellationToken,
        Func<TMessage, CancellationToken, Task> handleAsync
    )
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

        AsyncEventingBasicConsumer consumer = new(channel);

        consumer.ReceivedAsync += async (model, eventArguments) =>
        {
            var basicProperties = eventArguments.BasicProperties;
            var body = eventArguments.Body.ToArray();

            TMessage message = null!;
            try
            {
                message = JsonSerializer.Deserialize<TMessage>(body)!;

                if (message == null || message.GetType() != typeof(TMessage))
                {
                    logger.LogWarning(
                        "[{ClassName}] | [HandleMessageAsync] | Received null message of type {MessageType}",
                        _className, typeof(TMessage).Name
                    );
                    return;
                }
            }
            catch (JsonException ex)
            {
                logger.LogError(
                    "[{ClassName}] | [HandleRabbitMqAsync] | [{CorrelationId}] | AppId: {AppId} | ClusterId: {ClusterId} | Error deserializing message: {ErrorMessage} ",
                    _className, basicProperties.CorrelationId, basicProperties.AppId, basicProperties.ClusterId, ex.Message
                );
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    "[{ClassName}] | [HandleRabbitMqAsync] | [{CorrelationId}] | AppId: {AppId} | ClusterId: {ClusterId} | Unexpected error: {ErrorMessage}",
                    _className, basicProperties.CorrelationId, basicProperties.AppId, basicProperties.ClusterId, ex.Message
                );
                throw;
            }

            await handleAsync.Invoke(message, cancellationToken);
        };

        await channel.BasicConsumeAsync(
            queue: _queueName,
            autoAck: true,
            consumer: consumer,
            cancellationToken: cancellationToken
        );
    }

    protected abstract Task HandleUseCaseAsync(IServiceProvider serviceProvider, TMessage message, CancellationToken cancellationToken);
}