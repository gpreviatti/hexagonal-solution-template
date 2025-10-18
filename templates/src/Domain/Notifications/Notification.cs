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
    public string NotificationType { get; set; } = notificationType;
    public string NotificationStatus { get; set; } = notificationStatus;
    public string Message { get; set; } = message != null ? JsonSerializer.Serialize(message) : string.Empty;
}
