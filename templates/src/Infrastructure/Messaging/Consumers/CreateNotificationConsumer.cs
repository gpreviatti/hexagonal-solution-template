using Application.Common.Constants;
using Application.Common.Messages;
using Application.Common.Requests;
using Application.Common.UseCases;
using Application.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Messaging.Consumers;

public sealed class CreateNotificationConsumer(
    ILogger<CreateNotificationConsumer> logger,
    IConfiguration configuration,
    IBaseInOutUseCase<CreateNotificationRequest, BaseResponse<NotificationDto>, CreateNotificationUseCase> useCase
) : BaseConsumer<CreateNotificationMessage, CreateNotificationConsumer>(logger, configuration, NotificationType.OrderCreated)
{
    protected override async Task HandleMessageAsync(
        CreateNotificationMessage message,
        CancellationToken cancellationToken
    ) => await useCase.HandleAsync(new(
            message.CorrelationId,
            message.NotificationType,
            message.NotificationStatus,
            message.CreatedBy,
            message.Message
        ), cancellationToken
    );
}
