using System.Text.Json;
using Domain.Common;
using Domain.Common.Enums;

namespace Domain.Notifications;

public sealed class Notification : DomainEntity
{
    public Notification() {}

    public Notification(
        NotificationType notificationType,
        string notificationStatus,
        object? message = null,
        string? createdBy = null,
        string? timezoneId = null
    ) : base(createdBy ?? "System", timezoneId)
    {
        using var activity = ActivitySource.StartActivity($"{GetType().Name}.Constructor");

        NotificationType = notificationType;
        NotificationStatus = notificationStatus;
        Message = message != null ? JsonSerializer.Serialize(message) : string.Empty;

        activity?.SetTag(nameof(NotificationType), NotificationType);
        activity?.SetTag(nameof(NotificationStatus), NotificationStatus);
    }
    public NotificationType NotificationType { get; init; }
    public string NotificationStatus { get; init; }
    public string Message { get; init; }
}
