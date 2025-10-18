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
);
