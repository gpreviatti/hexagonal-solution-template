using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;
using System.Threading.Channels;
using Core.Common.Messages;
using Core.Common.Services;
using Core.Common;
using Core.Common.Enums;
using Core.Common.Extensions;
using Infrastructure.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Logs = Core.Common.Helpers.Logs;

namespace Infrastructure.Messaging.Consumers;

internal abstract class BaseConsumer<TMessage, TConsumer> : BaseBackgroundService<BaseConsumer<TMessage, TConsumer>> where TMessage : BaseMessage
{
    private readonly string _consumerName = typeof(TConsumer).Name;
    private readonly string _queueName;
    private readonly IDictionary<string, object?> _arguments;
    private readonly ConnectionFactory _factory;
    private IChannel _channel = null!;
    protected IProduceService producerService = null!;
    private readonly ActivitySource _activities = DefaultConfigurations.ActivitySource;
    private readonly Channel<TMessage> _workItemChannel;
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
            .CreateCounter<int>($"{DefaultConfigurations.CoreName}.{_consumerName}.Error", "total", "Number of times the consumer encountered an error");

        ConsumerDuplicatedMessageMetric = DefaultConfigurations.Meter
            .CreateCounter<int>($"{DefaultConfigurations.CoreName}.{_consumerName}.DuplicatedMessage", "total", "Number of times the consumer received a duplicated message");

        var connection = _factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = connection.CreateChannelAsync().GetAwaiter().GetResult();
        var randomGuid = Guid.NewGuid();
        Logs.Debug(logger, randomGuid, "Connected to RabbitMQ. Declaring queues.");

        _channel.QueueDeclareAsync(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: _arguments);
        _channel.QueueDeclareAsync(queue: _queueName + "_deadLetter", durable: true, exclusive: false, autoDelete: false, arguments: _arguments);

        _workItemChannel = Channel.CreateUnbounded<TMessage>(new UnboundedChannelOptions { SingleReader = true });
    }

    protected override async Task ExecuteInternalAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var randomGuid = Guid.NewGuid();
        Logs.Debug(logger, randomGuid, "Starting to consume messages.");

        AsyncEventingBasicConsumer rabbitConsumer = new(_channel);

        rabbitConsumer.ReceivedAsync += async (_, eventArguments) =>
        {
            var body = eventArguments.Body.ToArray();
            try
            {
                Logs.Debug(logger, randomGuid, "Message received. Deserializing.");

                var message = JsonSerializer.Deserialize<TMessage>(body)!;

                if (message == null || message.GetType() != typeof(TMessage))
                {
                    Logs.Warning(logger, randomGuid, typeof(TMessage).Name + " is null or of incorrect type.");
                    return;
                }

                Logs.Debug(logger, message.CorrelationId, "Message deserialized. Enqueueing.");

                await _workItemChannel.Writer.WriteAsync(message, cancellationToken);
            }
            catch (JsonException ex)
            {
                Logs.Error(logger, randomGuid, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Logs.Error(logger, randomGuid, ex.Message);
                throw;
            }
        };

        await _channel.BasicConsumeAsync(
            queue: _queueName,
            autoAck: true,
            consumer: rabbitConsumer,
            cancellationToken: cancellationToken
        );

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var message = await _workItemChannel.Reader.ReadAsync(cancellationToken);
                await ProcessMessageAsync(serviceProvider, message, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // stoppingToken was signalled — exit cleanly
            }
            catch (Exception ex)
            {
                Logs.Error(logger, ex.Message);
            }
        }
    }

    private async Task ProcessMessageAsync(IServiceProvider serviceProvider, TMessage message, CancellationToken cancellationToken)
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
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        Logs.Debug(logger, Guid.NewGuid(), $"{_consumerName} is stopping.");
        _workItemChannel.Writer.Complete();
        await base.StopAsync(cancellationToken);
    }

    protected abstract Task HandleUseCaseAsync(IServiceProvider serviceProvider, TMessage message, CancellationToken cancellationToken);
}
