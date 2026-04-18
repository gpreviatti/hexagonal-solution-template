using System.Text.Json;
using Domain.Common;
using Domain.Common.Enums;
using Domain.Common.Extensions;

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
        using var activity = ActivitySource.StartActivity($"{GetType().Name}.Constructor");
        activity.SetDefaultTags();

        NotificationType = notificationType;
        NotificationStatus = notificationStatus;

        if (message is not null)
            Message = JsonSerializer.Serialize(message);

        activity?.SetTag(nameof(NotificationType), NotificationType);
        activity?.SetTag(nameof(NotificationStatus), NotificationStatus);
    }
    public NotificationType NotificationType { get; init; }
    public NotificationStatus NotificationStatus { get; init; }
    public string? Message { get; init; }
}
