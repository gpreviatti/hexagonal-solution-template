using System.Text.Json;
using Domain.Common;
using Domain.Common.Enums;

namespace Domain.Notifications;

public sealed class Notification : DomainEntity
{
    public Notification() {}

    public Notification(
        NotificationType notificationType,
        NotificationStatus notificationStatus,
        object? message = null,
        string? createdBy = null,
        string? timezoneId = null
    ) : base(createdBy ?? "System", timezoneId)
    {
        NotificationType = notificationType;
        NotificationStatus = notificationStatus;

        if (message is not null)
            Message = JsonSerializer.Serialize(message);
    }
    public NotificationType NotificationType { get; init; }
    public NotificationStatus NotificationStatus { get; init; }
    public string? Message { get; init; }
}
