using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;
using Application.Common.Messages;
using Application.Common.Services;
using Domain.Common;
using Domain.Common.Enums;
using Domain.Common.Extensions;
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
    private readonly string _consumerName = typeof(TConsumer).Name;
    private readonly string _queueName;
    private readonly IDictionary<string, object?> _arguments;
    private readonly ConnectionFactory _factory;
    protected IProduceService producerService = null!;
    private readonly ActivitySource _activities = DefaultConfigurations.ActivitySource;
    protected Counter<int> ConsumerErrorMetric { get; }
    protected Counter<int> ConsumerDuplicatedMessageMetric { get; }

    public BaseConsumer(
        ILogger<BaseConsumer<TMessage, TConsumer>> logger,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        NotificationType queueName,
        IDictionary<string, object?> arguments = null!
    ) : base(logger, serviceScopeFactory, configuration)
    {
        var connectionString = configuration.GetConnectionString("RabbitMQ");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Invalid RabbitMQ connection string.");
        }

        _queueName = queueName.ToString();
        _arguments = arguments;
        _factory = new() { Uri = new(connectionString) };

        ConsumerErrorMetric = DefaultConfigurations.Meter
            .CreateCounter<int>($"{DefaultConfigurations.ApplicationName}.{_consumerName}.Error", "total", "Number of times the consumer encountered an error");

        ConsumerDuplicatedMessageMetric = DefaultConfigurations.Meter
            .CreateCounter<int>($"{DefaultConfigurations.ApplicationName}.{_consumerName}.DuplicatedMessage", "total", "Number of times the consumer received a duplicated message");
    }

    protected override async Task ExecuteInternalAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken) => await HandleRabbitMqAsync(
        async (message, cancellationToken) =>
        {
            var messageType = typeof(TMessage).Name;
            using var activity = _activities.StartActivity($"{_consumerName}", ActivityKind.Consumer);
            activity.SetDefaultTags();
            activity?.SetTag("correlationId", message.CorrelationId);
            activity?.SetTag("queueName", _queueName);

            producerService = serviceProvider.GetRequiredService<IProduceService>();

            try
            {
                var hybridCacheService = serviceProvider.GetRequiredService<IHybridCacheService>();

                Logs.Debug(logger, message.CorrelationId, messageType + " received. Checking if it has already been processed.");

                var isExecutedKey = _consumerName + "-" + message.CorrelationId;
                var isExecuted = await hybridCacheService.GetOrCreateAsync(
                    message.CorrelationId,
                    isExecutedKey,
                    async (cancellationToken) => false,
                    cancellationToken
                );

                if (isExecuted)
                {
                    Logs.Warning(logger, message.CorrelationId, messageType + " has already been processed. Skipping.");
                    ConsumerDuplicatedMessageMetric.Add(1);
                    return;
                }

                Logs.DebugStartingOperation(logger, message.CorrelationId, messageType + " processing started.");

                await HandleUseCaseAsync(serviceProvider, message, cancellationToken);

                await hybridCacheService.CreateAsync(message.CorrelationId, isExecutedKey, true, cancellationToken);

                Logs.DebugFinishedOperation(logger, message.CorrelationId, messageType + " processing finished.");
            }
            catch (Exception ex)
            {
                Logs.Error(logger, message.CorrelationId, ex.Message);

                ConsumerErrorMetric.Add(1);

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

        Logs.Debug(logger, Guid.NewGuid(), "Connected to RabbitMQ. Declaring queues.");

        await channel.QueueDeclareAsync(
            queue: _queueName,
            exclusive: true,
            autoDelete: true,
            arguments: _arguments,
            cancellationToken: cancellationToken
        );

        await channel.QueueDeclareAsync(
            queue: _queueName + "_deadLetter",
            exclusive: true,
            autoDelete: true,
            arguments: _arguments,
            cancellationToken: cancellationToken
        );

        AsyncEventingBasicConsumer consumer = new(channel);

        Logs.Debug(logger, Guid.NewGuid(), "Queues declared. Starting to consume messages.");

        consumer.ReceivedAsync += async (model, eventArguments) =>
        {
            var basicProperties = eventArguments.BasicProperties;
            var body = eventArguments.Body.ToArray();

            TMessage message = null!;
            try
            {
                Logs.Debug(logger, Guid.NewGuid(), "Message received. Deserializing.");

                message = JsonSerializer.Deserialize<TMessage>(body)!;

                Logs.Debug(logger, message.CorrelationId, "Message deserialized. Validating.");

                if (message == null || message.GetType() != typeof(TMessage))
                {
                    Logs.Warning(logger, Guid.NewGuid(), typeof(TMessage).Name + " is null or of incorrect type.");
                    return;
                }
            }
            catch (JsonException ex)
            {
                Logs.Error(logger, Guid.NewGuid(), ex.Message);

                throw;
            }
            catch (Exception ex)
            {
                Logs.Error(logger, Guid.NewGuid(), ex.Message);

                throw;
            }

            Logs.Debug(logger, message.CorrelationId, "Message validated. Handling use case.");

            await handleAsync.Invoke(message, cancellationToken);

            Logs.Debug(logger, message.CorrelationId, "Use case handled.");
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
