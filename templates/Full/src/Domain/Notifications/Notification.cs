using System.Text.Json;
using Domain.Common;

namespace Domain.Notifications;

public sealed class Notification : DomainEntity
{
    public Notification() {}

    public Notification(
        string notificationType,
        string notificationStatus,
        object? message = null,
        string? createdBy = null,
        string? timezoneId = null
    ) : base(createdBy ?? "System", timezoneId)
    {
        NotificationType = notificationType;
        NotificationStatus = notificationStatus;
        Message = message != null ? JsonSerializer.Serialize(message) : string.Empty;
    }
    public string NotificationType { get; init; }
    public string NotificationStatus { get; init; }
    public string Message { get; init; }
}
