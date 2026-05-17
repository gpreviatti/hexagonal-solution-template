using System.ComponentModel.DataAnnotations;
using Application.Common.Helpers;
using Application.Common.Requests;
using Application.Common.UseCases;
using Domain.Common.Enums;
using Domain.Notifications;

namespace Application.Notifications;

public sealed record CreateNotificationRequest(
    [Required] Guid CorrelationId,
    [Required, EnumDataType(typeof(NotificationType))] NotificationType NotificationType,
    [Required, EnumDataType(typeof(NotificationStatus))] NotificationStatus NotificationStatus,
    string? CreatedBy = null,
    object? Message = null
) : BaseRequest(CorrelationId);

public sealed class CreateNotificationUseCase(IServiceProvider serviceProvider) : BaseInUseCase<CreateNotificationRequest>(serviceProvider)
{
    public override async Task HandleInternalAsync(
        CreateNotificationRequest request,
        CancellationToken cancellationToken
    )
    {
        var notification = new Notification(
            request.NotificationType,
            request.NotificationStatus,
            request.Message,
            request.CreatedBy,
            request.TimezoneId
        );

        var addResult = await Repository.AddAsync(notification, request.CorrelationId, cancellationToken);

        if (addResult == 0)
            Logs.FailedOperation(Logger, request.CorrelationId, "Failed to create notification. No rows affected.");
    }
}
