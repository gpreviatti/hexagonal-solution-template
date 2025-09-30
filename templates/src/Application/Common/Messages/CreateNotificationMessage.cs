namespace Application.Common.Messages;

public record CreateNotificationMessage(string NotificationType, DateTime CreatedAt, string CreatedBy = null, object Message = null);