namespace Core.Common.Messages;

public abstract record BaseMessage(Guid CorrelationId, DateTime CreatedAt);
