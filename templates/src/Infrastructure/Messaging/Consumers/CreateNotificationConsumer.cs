using Application.Common.Messages;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Messaging.Consumers;

public sealed class CreateNotificationConsumer(ILogger<CreateNotificationConsumer> logger) : BaseConsumer<CreateNotificationMessage, CreateNotificationConsumer>(logger)
{
    protected override Task HandleMessageAsync(CreateNotificationMessage message, CancellationToken cancellationToken)
    {
        // Implementation for creating notification consumers
        return Task.CompletedTask;
    }
}