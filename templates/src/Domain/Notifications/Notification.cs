using System.Text.Json;
using Domain.Common;

namespace Domain.Notifications;

public sealed class Notification : DomainEntity
{
    public Notification() { }
    public Notification(
        string notificationType,
        DateTime? currentDate,
        string createdBy = null,
        object message = null
    ) : base(currentDate, createdBy)
    {
        NotificationType = notificationType;
        Message = message != null ? JsonSerializer.Serialize(message) : string.Empty;
    }

    public string NotificationType { get; set; }
    public NotificationTypeStatus NotificationTypeStatus { get; private set; }
    public string Message { get; set; }

    public void SetNotificationTypeStatus(NotificationTypeStatus status) => NotificationTypeStatus = status;
}