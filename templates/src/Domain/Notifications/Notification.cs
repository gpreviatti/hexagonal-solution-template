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
    ) : base(createdBy)
    {
        NotificationType = notificationType;
        NotificationStatus = notificationStatus;
        Message = message != null ? JsonSerializer.Serialize(message) : string.Empty;
    }
    public string NotificationType { get; private set; }
    public string NotificationStatus { get; private set; }
    public string Message { get; private set; }
}
