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
using Logs = Application.Common.Helpers.Logs;

namespace Infrastructure.Messaging.Consumers;

sealed file record IsExecuted(bool Value);

internal abstract class BaseConsumer<TMessage, TConsumer> : BaseBackgroundService<BaseConsumer<TMessage, TConsumer>> where TMessage : BaseMessage
{
    private readonly string _className = typeof(TConsumer).Name;
    private readonly string _queueName;
    private readonly IDictionary<string, object?> _arguments;
    private readonly ConnectionFactory _factory;
    private readonly Stopwatch _stopwatch = new();
    protected IProduceService producerService = null!;

    public BaseConsumer(
        ILogger<BaseConsumer<TMessage, TConsumer>> logger,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        string queueName,
        IDictionary<string, object?> arguments = null!
    ) : base(logger, serviceScopeFactory, configuration)
    {
        var connectionString = configuration.GetConnectionString("RabbitMQ");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Invalid RabbitMQ connection string.");
        }

        _queueName = queueName;
        _arguments = arguments;
        _factory = new() { Uri = new(connectionString) };
    }

    protected override async Task ExecuteInternalAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken) => await HandleRabbitMqAsync(
        async (message, cancellationToken) =>
        {
            producerService = serviceProvider.GetRequiredService<IProduceService>();
            try
            {
                _stopwatch.Restart();
                var hybridCacheService = serviceProvider.GetRequiredService<IHybridCacheService>();


                Logs.ReceivedMessage(logger, _className, message.CorrelationId, typeof(TMessage).Name);

                var isExecutedKey = _className + "-" + message.CorrelationId;
                var isExecuted = await hybridCacheService.GetOrCreateAsync(
                    isExecutedKey,
                    async (cancellationToken) => false,
                    cancellationToken
                );

                if (isExecuted)
                {
                    Logs.DuplicateMessageDetected(logger, _className, message.CorrelationId);
                    return;
                }

                Logs.StartProcessingMessage(logger, _className, message.CorrelationId);

                await HandleUseCaseAsync(serviceProvider, message, cancellationToken);

                await hybridCacheService.CreateAsync(isExecutedKey, true, cancellationToken);

                Logs.ProcessedMessage(logger, _className, message.CorrelationId, _stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                Logs.ErrorProcessingMessage(logger, _className, message?.CorrelationId, ex.Message, ex.StackTrace);

                _ = producerService.HandleAsync(message!, CancellationToken.None, _queueName + "_deadLetter");

                throw;
            }
        },
        cancellationToken
    );

    private async Task HandleRabbitMqAsync(
        Func<TMessage, CancellationToken, Task> handleAsync,
        CancellationToken cancellationToken
    )
    {
        var connection = await _factory.CreateConnectionAsync(cancellationToken);
        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: _queueName,
            exclusive: false,
            autoDelete: false,
            arguments: _arguments,
            cancellationToken: cancellationToken
        );

        await channel.QueueDeclareAsync(
            queue: _queueName + "_deadLetter",
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
                    Logs.ReceivedNullMessage(logger, _className, typeof(TMessage).Name);
                    return;
                }
            }
            catch (JsonException ex)
            {
                Logs.ErrorDeserializingMessage(logger, _className, basicProperties.CorrelationId, basicProperties.AppId, basicProperties.ClusterId, ex.Message, ex.StackTrace);

                throw;
            }
            catch (Exception ex)
            {
                Logs.UnexpectedError(logger, _className, basicProperties.CorrelationId, basicProperties.AppId, basicProperties.ClusterId, ex.Message, ex.StackTrace);

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