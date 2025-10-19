using System.Text.Json;
using Domain.Common;

namespace Domain.Notifications;

public sealed class Notification(
    string notificationType,
    string notificationStatus,
    DateTime? currentDate,
    string? createdBy = null,
    object? message = null
) : DomainEntity(currentDate, createdBy)
{
    public string NotificationType { get; private set; } = notificationType;
    public string NotificationStatus { get; private set; } = notificationStatus;
    public string Message { get; private set; } = message != null ? JsonSerializer.Serialize(message) : string.Empty;
}
