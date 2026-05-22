using Core.Common.Messages;
using Core.Common.UseCases;
using Core.Notifications;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Messaging.Consumers;

internal sealed class CreateNotificationConsumer(IServiceScopeFactory serviceScopeFactory) : BaseConsumer<CreateNotificationMessage, CreateNotificationConsumer>(serviceScopeFactory)
{
    protected override async Task HandleUseCaseAsync(
        IServiceProvider serviceProvider,
        CreateNotificationMessage message,
        CancellationToken cancellationToken
    ) => await serviceProvider.GetRequiredService<IBaseInUseCase<CreateNotificationRequest>>().HandleAsync(new(
            message.CorrelationId,
            message.NotificationType,
            message.NotificationStatus,
            message.CreatedBy,
            message.Message
        ), cancellationToken
    );
}
