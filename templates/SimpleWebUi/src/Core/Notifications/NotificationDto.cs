using Core.Common.Enums;

namespace Core.Notifications;

public sealed record NotificationDto
{
    public int Id { get; set; }
    public NotificationType NotificationType { get; set; }
    public NotificationStatus NotificationStatus { get; set; }
    public string? Message { get; set; }
};
