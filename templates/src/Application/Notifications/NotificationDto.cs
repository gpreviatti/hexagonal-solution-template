using Domain.Notifications;

namespace Application.Notifications;

public sealed record NotificationDto(
    int Id,
    string NotificationType,
    NotificationTypeStatus NotificationTypeStatus,
    string Message,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string CreatedBy,
    string UpdatedBy
);