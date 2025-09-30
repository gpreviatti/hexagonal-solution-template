using Application.Common.Messages;

namespace Infrastructure.Messaging.Consumers;

public sealed class CreateNotificationConsumer : BaseConsumer<CreateNotificationMessage>
{
    protected override Task HandleMessageAsync(CreateNotificationMessage message, CancellationToken cancellationToken)
    {
        // Implementation for creating notification consumers
        return Task.CompletedTask;
    }
}