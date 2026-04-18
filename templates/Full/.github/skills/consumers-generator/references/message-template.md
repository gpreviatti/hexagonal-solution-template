# Message Template

```csharp
public sealed record {MessageName}(
    Guid CorrelationId,
    string Payload,
    string? CreatedBy = null
) : BaseMessage(CorrelationId, DateTime.UtcNow);
```
