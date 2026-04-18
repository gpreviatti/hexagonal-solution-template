---
name: consumers-generator
description: 'Generate RabbitMQ message consumers and message record types in the hexagonal architecture template project following BaseConsumer patterns, deduplication, and DI auto-registration'
---

# Consumers Generator — Hexagonal Architecture Template

Generate RabbitMQ consumers and the supporting message record types for the Infrastructure layer, following the project's `BaseConsumer<TMessage, TConsumer>` patterns.

## When to Use

Activate this skill when:
- User asks to create a new message consumer or queue listener
- User asks to add a new RabbitMQ queue or expand the messaging layer
- User mentions "consumer", "message handler", "queue", "event consumer", or "background service for messaging"
- User wants to add a new `NotificationType` enum value for a new queue
- User asks to create a new message record type (`*Message`)
- User asks how to produce messages from a use case

---

## Skill-local References

| File | Purpose |
|------|---------|
| `./references/consumer-template.md` | Base consumer implementation template |
| `./references/message-template.md` | Base message record template |

> Use these in-folder references for long-term stability.

---

## Architecture Overview

```
Producer (UseCase)          Message Record           Consumer (Background Service)
     │                           │                           │
     │── ProduceService ──────>  TMessage : BaseMessage ──> BaseConsumer<TMessage, TConsumer>
     │   HandleAsync(msg,        (sealed record with         │── deduplication (IHybridCacheService)
     │   queue: "QueueName")      CorrelationId)             │── error → deadLetter queue
                                                             │── HandleUseCaseAsync(abstract)
                                                             └── calls IBaseInUseCase
```

**Key behaviors built into `BaseConsumer`:**
- **Deduplication**: Uses `IHybridCacheService` keyed on `consumerName + correlationId` — duplicate messages are safely skipped and counted
- **Dead-letter routing**: On unhandled exception, message is re-published to `{QueueName}_deadLetter` automatically
- **Telemetry**: Each message creates an `Activity` span with `correlationId` and `queueName` tags; error and duplicate counters are created as `Counter<int>` metrics
- **Auto queue declaration**: Both `{QueueName}` and `{QueueName}_deadLetter` queues are declared on startup
- **DI registration**: All classes in `Infrastructure.Messaging.Consumers` that implement `IHostedService` are auto-registered — no manual `AddHostedService<T>()` required

---

## Step-by-Step: Creating a New Consumer

### Step 1 — Add the queue name to `NotificationType` enum

`src/Domain/Common/Enums/NotificationType.cs`

```csharp
namespace Domain.Common.Enums;

public enum NotificationType
{
    OrderCreated,
    OrderUpdated,
    OrderDeleted,
    PaymentProcessed  // ← new value; string value becomes the queue name
}
```

> The enum value's `.ToString()` is used directly as the RabbitMQ queue name.

---

### Step 2 — Create the message record

`src/Application/Common/Messages/ProcessPaymentMessage.cs`

```csharp
using Domain.Common.Enums;

namespace Application.Common.Messages;

public sealed record ProcessPaymentMessage(
    Guid CorrelationId,
    Guid OrderId,
    decimal Amount,
    string Currency,
    string? CreatedBy = null
) : BaseMessage(CorrelationId, DateTime.UtcNow);
```

**Rules:**
- Extends `BaseMessage(CorrelationId, DateTime.UtcNow)` — `CorrelationId` and `CreatedAt` are inherited
- Use `sealed record` — immutable, value-equality, serializable
- All properties required for the use case go here — keep it flat (avoid nested objects)
- Optional fields default to `null`
- Namespace: `Application.Common.Messages`
- File location: `src/Application/Common/Messages/`

---

### Step 3 — Create the consumer class

`src/Infrastructure/Messaging/Consumers/ProcessPaymentConsumer.cs`

```csharp
using Application.Common.Messages;
using Application.Common.UseCases;
using Application.Payments;
using Domain.Common.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Messaging.Consumers;

internal sealed class ProcessPaymentConsumer(
    ILogger<ProcessPaymentConsumer> logger,
    IServiceScopeFactory serviceScopeFactory,
    IConfiguration configuration
) : BaseConsumer<ProcessPaymentMessage, ProcessPaymentConsumer>(
    logger,
    serviceScopeFactory,
    configuration,
    NotificationType.PaymentProcessed  // ← must match the queue/enum value
)
{
    protected override async Task HandleUseCaseAsync(
        IServiceProvider serviceProvider,
        ProcessPaymentMessage message,
        CancellationToken cancellationToken
    ) => await serviceProvider
        .GetRequiredService<IBaseInUseCase<ProcessPaymentRequest>>()
        .HandleAsync(new(
            message.CorrelationId,
            message.OrderId,
            message.Amount,
            message.Currency,
            message.CreatedBy
        ), cancellationToken);
}
```

**Rules:**
- Class modifier: `internal sealed`
- Use primary constructor — three fixed parameters: `ILogger<TSelf>`, `IServiceScopeFactory`, `IConfiguration`
- First generic parameter `TMessage` = your message record
- Second generic parameter `TConsumer` = the consumer class itself (used for naming metrics and activities)
- Pass the matching `NotificationType` enum value as the `queueName` argument
- `HandleUseCaseAsync` resolves the use case from `serviceProvider` using `GetRequiredService<IBaseInUseCase<TRequest>>()`
- Map message properties directly to the use case request constructor
- Namespace: `Infrastructure.Messaging.Consumers`
- File location: `src/Infrastructure/Messaging/Consumers/`

---

### Step 4 — No registration needed

`MessagingDependencyInjection.cs` auto-registers all consumers via reflection:

```csharp
// Already in place — no changes required
var consumerTypes = infrastructureAssembly.GetTypes()
    .Where(t => t.IsClass
        && !t.IsAbstract
        && t.Namespace?.Contains("Messaging.Consumers") == true
        && typeof(IHostedService).IsAssignableFrom(t))
    .ToList();
```

Your consumer is picked up automatically as long as:
- It is in the `Infrastructure.Messaging.Consumers` namespace
- It is a non-abstract class
- It inherits `BaseConsumer<,>` (which inherits `BackgroundService`, which implements `IHostedService`)

---

## Producing Messages from a Use Case

`BaseUseCase` exposes a `CreateNotification()` helper that publishes a `CreateNotificationMessage` for the standard notification flow. For **custom messages** (like `ProcessPaymentMessage`), call `ProduceService.HandleAsync` directly:

```csharp
// Producing a custom message from a use case
await ProduceService.HandleAsync(
    new ProcessPaymentMessage(
        correlationId,
        order.Id,
        order.Total,
        "BRL",
        request.CreatedBy
    ),
    cancellationToken,
    queue: NotificationType.PaymentProcessed.ToString()
);
```

The standard `CreateNotification()` helper (already on `BaseUseCase`) wraps `ProduceService.HandleAsync` for the `CreateNotificationMessage` queue — use it for order lifecycle events:

```csharp
// Standard notification helper — use for OrderCreated / OrderUpdated / OrderDeleted
CreateNotification(correlationId, NotificationStatus.Success, request.CreatedBy, _notificationType, response);
```

**`IProduceService` signature:**
```csharp
Task HandleAsync<TMessage>(
    TMessage message,
    CancellationToken cancellationToken,
    string queue = "",
    string exchange = ""
) where TMessage : BaseMessage;

// Batch overload
Task HandleAsync<TMessage>(
    IEnumerable<TMessage> messages,
    CancellationToken cancellationToken,
    string queue = "",
    string exchange = ""
) where TMessage : BaseMessage;
```

---

## Complete Example: Full Consumer Flow

Scenario: A new `ShipOrder` consumer that listens to the `OrderShipped` queue and calls `ShipOrderUseCase`.

**1. Enum** — `src/Domain/Common/Enums/NotificationType.cs`
```csharp
public enum NotificationType
{
    OrderCreated,
    OrderUpdated,
    OrderDeleted,
    OrderShipped   // ← add
}
```

**2. Message** — `src/Application/Common/Messages/ShipOrderMessage.cs`
```csharp
namespace Application.Common.Messages;

public sealed record ShipOrderMessage(
    Guid CorrelationId,
    Guid OrderId,
    string TrackingCode,
    string? ShippedBy = null
) : BaseMessage(CorrelationId, DateTime.UtcNow);
```

**3. Consumer** — `src/Infrastructure/Messaging/Consumers/ShipOrderConsumer.cs`
```csharp
using Application.Common.Messages;
using Application.Common.UseCases;
using Application.Orders;
using Domain.Common.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Messaging.Consumers;

internal sealed class ShipOrderConsumer(
    ILogger<ShipOrderConsumer> logger,
    IServiceScopeFactory serviceScopeFactory,
    IConfiguration configuration
) : BaseConsumer<ShipOrderMessage, ShipOrderConsumer>(
    logger,
    serviceScopeFactory,
    configuration,
    NotificationType.OrderShipped
)
{
    protected override async Task HandleUseCaseAsync(
        IServiceProvider serviceProvider,
        ShipOrderMessage message,
        CancellationToken cancellationToken
    ) => await serviceProvider
        .GetRequiredService<IBaseInUseCase<ShipOrderRequest>>()
        .HandleAsync(new(
            message.CorrelationId,
            message.OrderId,
            message.TrackingCode,
            message.ShippedBy
        ), cancellationToken);
}
```

**4. Produce from use case** — inside `CreateOrderUseCase.HandleInternalAsync` (or any use case):
```csharp
await ProduceService.HandleAsync(
    new ShipOrderMessage(correlationId, order.Id, trackingCode, request.CreatedBy),
    cancellationToken,
    queue: NotificationType.OrderShipped.ToString()
);
```

---

## Checklist

Before submitting a new consumer, verify:

- [ ] `NotificationType` enum has a new value matching the queue name
- [ ] Message record is `sealed record`, extends `BaseMessage(CorrelationId, DateTime.UtcNow)`, placed in `Application.Common.Messages`
- [ ] Consumer is `internal sealed`, primary constructor takes `ILogger<TSelf>`, `IServiceScopeFactory`, `IConfiguration`
- [ ] `BaseConsumer<TMessage, TConsumer>` type params are correct (message first, consumer class second)
- [ ] `queueName` argument matches the new `NotificationType` enum value
- [ ] `HandleUseCaseAsync` resolves the use case via `GetRequiredService<IBaseInUseCase<TRequest>>()`
- [ ] No manual `AddHostedService<T>()` registration added
- [ ] Producer call uses `queue: NotificationType.{Value}.ToString()`
