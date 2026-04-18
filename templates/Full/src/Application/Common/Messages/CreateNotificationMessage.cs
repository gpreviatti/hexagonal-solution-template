using Domain.Common.Enums;

namespace Application.Common.Messages;

public sealed record CreateNotificationMessage(
    Guid CorrelationId,
    NotificationType NotificationType,
    NotificationStatus NotificationStatus,
    string? CreatedBy = null,
    object? Message = null
) : BaseMessage(CorrelationId, DateTime.UtcNow);
