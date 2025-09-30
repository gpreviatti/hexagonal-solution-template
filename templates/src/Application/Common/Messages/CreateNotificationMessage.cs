namespace Application.Common.Messages;

public record CreateNotificationMessage(
    Guid CorrelationId,
    string NotificationType,
    string CreatedBy = null,
    object Message = null
) : BaseMessage(CorrelationId, DateTime.UtcNow);