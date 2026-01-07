namespace Application.Common.Messages;

public sealed record CreateNotificationMessage(
    Guid CorrelationId,
    string NotificationType,
    string NotificationStatus,
    string? CreatedBy = null,
    object? Message = null
) : BaseMessage(CorrelationId, DateTime.UtcNow);
