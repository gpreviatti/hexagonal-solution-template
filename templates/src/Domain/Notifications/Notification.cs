using System.Text.Json;
using Domain.Common;

namespace Domain.Notifications;

public sealed class Notification : DomainEntity
{
    public Notification() {}

    public Notification(
        string notificationType,
        string notificationStatus,
        string? createdBy = null,
        object? message = null
    ) : base(DateTime.UtcNow, createdBy)
    {
        NotificationType = notificationType;
        NotificationStatus = notificationStatus;
        Message = message != null ? JsonSerializer.Serialize(message) : string.Empty;
    }
    public string NotificationType { get; init; }
    public string NotificationStatus { get; init; }
    public string Message { get; init; }
}
