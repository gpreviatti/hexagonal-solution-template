# Consumer Template

```csharp
internal sealed class {ConsumerName}(
    ILogger<{ConsumerName}> logger,
    IServiceScopeFactory serviceScopeFactory,
    IConfiguration configuration
) : BaseConsumer<{MessageName}, {ConsumerName}>(
    logger,
    serviceScopeFactory,
    configuration,
    NotificationType.{QueueEnumValue}
)
{
    protected override async Task HandleUseCaseAsync(
        IServiceProvider serviceProvider,
        {MessageName} message,
        CancellationToken cancellationToken
    ) => await serviceProvider
        .GetRequiredService<IBaseInUseCase<{RequestType}>>()
        .HandleAsync(new(
            message.CorrelationId
        ), cancellationToken);
}
```