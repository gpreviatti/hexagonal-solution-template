namespace Application.Notifications;

public sealed record NotificationDto(
    int Id,
    string NotificationType,
    string NotificationStatus,
    string Message,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string? CreatedBy,
    string? UpdatedBy
)
{
    public static implicit operator NotificationDto(Domain.Notifications.Notification notification) => new(
        notification.Id, notification.NotificationType,
        notification.NotificationStatus, notification.Message,
        notification.CreatedAt, notification.UpdatedAt,
        notification.CreatedBy, notification.UpdatedBy
    );
};
