using Application.Common.Enums;

namespace Application.Common.Messages;

public sealed record CreateNotificationMessage(
    Guid CorrelationId,
    NotificationType NotificationType,
    string NotificationStatus,
    string? CreatedBy = null,
    object? Message = null
) : BaseMessage(CorrelationId, DateTime.UtcNow);
