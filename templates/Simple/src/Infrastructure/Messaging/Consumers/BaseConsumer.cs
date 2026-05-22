using System.Diagnostics;
using System.Diagnostics.Metrics;
using Core.Common.Messages;
using Core.Common.Services;
using Core.Common;
using Infrastructure.Common;
using Microsoft.Extensions.DependencyInjection;
using Logs = Core.Common.Helpers.Logs;
using Core.Common.Extensions;

namespace Infrastructure.Messaging.Consumers;
internal abstract class BaseConsumer<TMessage, TConsumer> : BaseBackgroundChannelService<BaseConsumer<TMessage, TConsumer>, TMessage> where TMessage : BaseMessage
{
    private readonly string _consumerName = typeof(TConsumer).Name;
    private readonly ActivitySource _activities = DefaultConfigurations.ActivitySource;
    protected Counter<int> ConsumerErrorMetric { get; }
    protected Counter<int> ConsumerDuplicatedMessageMetric { get; }

    public BaseConsumer(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
    {
        ConsumerErrorMetric = DefaultConfigurations.Meter
            .CreateCounter<int>($"{DefaultConfigurations.ApplicationName}.{_consumerName}.Error", "total", "Number of times the consumer encountered an error");

        ConsumerDuplicatedMessageMetric = DefaultConfigurations.Meter
            .CreateCounter<int>($"{DefaultConfigurations.ApplicationName}.{_consumerName}.DuplicatedMessage", "total", "Number of times the consumer received a duplicated message");
    }

    protected override async Task ExecuteInternalAsync(IServiceProvider serviceProvider, TMessage message, CancellationToken cancellationToken)
    {
        var messageType = typeof(TMessage).Name;
        using var activity = _activities.StartActivity($"{_consumerName}", ActivityKind.Consumer);
        activity.SetDefaultTags();
        activity?.SetTag("correlationId", message.CorrelationId);

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

            throw;
        }
    }

    protected abstract Task HandleUseCaseAsync(IServiceProvider serviceProvider, TMessage message, CancellationToken cancellationToken);
}
