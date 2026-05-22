using System.Diagnostics;
using Core.Common.Helpers;
using Core.Common.Messages;
using Core.Common.Services;
using Core.Common;
using Core.Common.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Channels;

namespace Infrastructure.Messaging.Producers;

public sealed class ProducerService(IServiceProvider serviceProvider) : IProduceService
{
    private readonly ILogger<ProducerService> _logger = serviceProvider.GetRequiredService<ILogger<ProducerService>>();
    private readonly ActivitySource _activities = DefaultConfigurations.ActivitySource;

    public async Task HandleAsync<TMessage>(TMessage message, CancellationToken cancellationToken) where TMessage : BaseMessage
    {
        await Task.Yield();

        using var activity = _activities.StartActivity($"{nameof(ProducerService)}.{nameof(HandleAsync)}.{typeof(TMessage).Name}");
        
        activity.SetDefaultTags();

        Logs.DebugStartingOperation(_logger, message.CorrelationId, typeof(TMessage).Name + " publishing started.");

        await serviceProvider.GetRequiredService<Channel<TMessage>>().Writer.WriteAsync(message, cancellationToken);

        Logs.DebugFinishedOperation(_logger, message.CorrelationId, typeof(TMessage).Name + " published.");
    }

    public async Task HandleAsync<TMessage>(IEnumerable<TMessage> messages, CancellationToken cancellationToken) where TMessage : BaseMessage
    {
        await Task.Yield();

        using var activity = _activities.StartActivity($"{nameof(ProducerService)}.{nameof(HandleAsync)}.{typeof(TMessage).Name}.Batch");
        activity.SetDefaultTags();

        Logs.Debug(_logger, messages.FirstOrDefault()?.CorrelationId ?? Guid.Empty, typeof(TMessage).Name + " batch publishing started.");


        foreach (var message in messages)
        {
            Logs.DebugStartingOperation(_logger, message.CorrelationId, typeof(TMessage).Name + " batch publishing started.");

            await serviceProvider.GetRequiredService<Channel<TMessage>>().Writer.WriteAsync(message, cancellationToken);

            Logs.DebugFinishedOperation(_logger, message.CorrelationId, typeof(TMessage).Name + " batch published.");
        }

        Logs.Debug(_logger, messages.FirstOrDefault()?.CorrelationId ?? Guid.Empty, typeof(TMessage).Name + " batch publishing finished.");
    }
}
