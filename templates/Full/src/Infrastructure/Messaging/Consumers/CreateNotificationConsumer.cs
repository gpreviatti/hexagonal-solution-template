using Application.Common.Constants;
using Application.Common.Messages;
using Application.Common.UseCases;
using Application.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Messaging.Consumers;

internal sealed class CreateNotificationConsumer(
    ILogger<CreateNotificationConsumer> logger,
    IServiceScopeFactory serviceScopeFactory,
    IConfiguration configuration
) : BaseConsumer<CreateNotificationMessage, CreateNotificationConsumer>(logger, serviceScopeFactory, configuration, NotificationType.OrderCreated)
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
