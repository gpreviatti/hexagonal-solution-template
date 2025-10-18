using System.Text.Json;
using Domain.Common;

namespace Domain.Notifications;

public sealed class Notification : DomainEntity
{
    public Notification() { }
    public Notification(
        string notificationType,
        string notificationStatus,
        DateTime? currentDate,
        string? createdBy = null,
        object? message = null
    ) : base(currentDate, createdBy)
    {
        NotificationType = notificationType;
        NotificationStatus = notificationStatus;
        Message = message != null ? JsonSerializer.Serialize(message) : string.Empty;
    }

    public string NotificationType { get; set; }
    public string NotificationStatus { get; set; }
    public string Message { get; set; }
}
