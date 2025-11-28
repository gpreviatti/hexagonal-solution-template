namespace Application.Notifications;

public sealed record NotificationDto
{
    public int Id { get; set; }
    public string NotificationType { get; set; }
    public string NotificationStatus { get; set; }
    public string Message { get; set; }
};
