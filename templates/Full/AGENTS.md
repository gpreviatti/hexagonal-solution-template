# 🤖 Hexagonal Architecture AI Agent Guide

This document guides AI coding agents to work effectively in this hexagonal architecture template. Agents should read this first and follow the patterns documented here.

## Quick Start for Agents

- **Architecture:** 4-layer hexagonal (Domain → Application → Infrastructure → WebApp)
- **Dependency direction:** Always inward; Domain has ZERO external dependencies
- **Build command:** `dotnet build`
- **Test command:** `dotnet test`
- **Run command:** `dotnet run --project src/WebApp` (listens on `https://localhost:7000`)
- **Skills:** Use `/create-entity`, `/create-use-case`, `/create-endpoint`, `/create-test`, `/create-consumer` when generating code

---

## 🏗️ Architecture Overview

This is a **Hexagonal Architecture** (Ports & Adapters) following Domain-Driven Design (DDD). Business logic is isolated from external concerns.

```
Domain Layer (src/Domain/)
    ↓ (depends on nothing)
Application Layer (src/Application/) [ports/interfaces]
    ↓
Infrastructure Layer (src/Infrastructure/) [adapter implementations]
    ↓
WebApp Layer (src/WebApp/) [HTTP/gRPC entry points]
```

**Key principle:** Dependencies flow inward only. Never violate this.

### See also
- Full architecture explanation: [Readme.md > Project Overview](Readme.md#-project-overview)
- Hexagonal Architecture concept: https://alistair.cockburn.us/hexagonal-architecture/

---

## 📁 Layer Patterns and Conventions

### 🟠 Domain Layer (`src/Domain/`)

The **innermost, pure business logic layer**. No dependencies on other layers or infrastructure packages.

#### Entity Patterns

**Two entity strategies:**

**Pattern A — Aggregate Root** (use `Result<T>` + static `Create`)
- For aggregate roots or entities with complex invariants and multiple operations
- Example: `Order`, `Product`, any bounded context root
- Static `Create()` factory validates and returns `Result<Entity>`
- Business methods return `Result<Unit>` for outcomes

**Pattern B — Child Entity / Value Object** (constructor validation + `DomainException`)
- For child entities or simpler domain objects within an aggregate
- Example: `Item`, `Address`, `ProductVariant`
- Constructor directly throws `DomainException` on invalid invariants
- No factory method needed

**General rules (both patterns):**
- **Base class:** All entities inherit from `DomainEntity` (has `Id`, `CreatedAt`, `UpdatedAt`, `DeletedAt`)
- **Sealed classes:** Domain entities and value objects should be sealed
- **Private setters:** Properties are read-only externally; business logic methods modify state internally
- **Business logic:** All invariants enforced inside entities, never in services
- **Soft deletes:** Use only; `IsDeleted` flag is set, never permanently deleted
- **Result<T> type:** Aggregate roots return `Result<T>` for expected errors
- **Domain exceptions:** Only for invariant violations that should never happen
- **OpenTelemetry:** Call `Handle(activity => { ... })` to create Activity spans with tagged attributes

**Example — Aggregate Root (Pattern A):**
```csharp
public sealed class Order : DomainEntity
{
    public string OrderNumber { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }

    private Order() { }  // Required for EF Core

    // Static factory with Result<T> return
    public static Result<Order> Create(string orderNumber, decimal totalAmount)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            return Result.Failure<Order>("Order number is required");
        if (totalAmount <= 0)
            return Result.Failure<Order>("Total amount must be positive");

        return Result.Success(new Order
        {
            OrderNumber = orderNumber,
            Status = OrderStatus.Pending,
            TotalAmount = totalAmount
        });
    }

    // Business operation returning Result<Unit>
    public Result<Unit> MarkAsShipped()
    {
        if (Status != OrderStatus.Pending)
            return Result.Failure<Unit>("Only pending orders can be shipped");
        Status = OrderStatus.Shipped;
        return Result.Success(Unit.Value);
    }
}
```

**Example — Child Entity (Pattern B):**
```csharp
public sealed class OrderItem : DomainEntity
{
    public OrderItem() { }  // Required for EF Core

    public OrderItem(string name, decimal price, int quantity)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Item name is required");
        if (price <= 0)
            throw new DomainException("Item price must be positive");

        Name = name;
        Price = price;
        Quantity = quantity;
    }

    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public int Quantity { get; private set; }
}
```

**See also:** [Readme.md > src/Domain/](Readme.md#-src-domain)

### 🔵 Application Layer (`src/Application/`)

Orchestrates domain objects and coordinates infrastructure through **ports (interfaces)**.

#### Port (Interface) Patterns
- **IBaseRepository<T>:** Repository port for data access (interface defined here, implementation in Infrastructure)
- **IHybridCacheService:** Caching port (Redis + in-memory)
- **IProduceService:** Message publishing port (RabbitMQ via MassTransit)
- Define ports in `Common/` subdirectories relevant to the concern

#### Use Case Patterns
- **Base class selection:**
  - `BaseInOutUseCase<TRequest, TResponse>` — For CRUD operations returning data (Create, Get, Update)
  - `BaseInUseCase<TRequest>` — For side-effect operations without response payload (Delete, fire-and-forget)
  - `BaseOutUseCase<TResponse>` — For operations with no input request
- **Single responsibility:** Each use case file handles exactly one operation (Create, Get, Update, Delete, GetAll)
- **Request Records:** Inherit from `BaseRequest` which provides `CorrelationId`, `User`, `TimezoneId`
- **Validation:** Implement `AbstractValidator<TRequest>` (auto-discovered by DI); validation runs automatically
- **Naming:** `Create{Entity}UseCase`, `Get{Entity}UseCase`, `Update{Entity}UseCase`, etc.
- **Error handling:** Use `Result<TResponse>` for expected failures
- **Repository pattern:** Inject `IBaseRepository<T>` for persistence
- **Sealed classes:** Use cases should be sealed
- **Logging:** Use structured logging with `ILogger<UseCase>`
- **Notification publishing:** Use `CreateNotification()` helper or `IProduceService.ProduceAsync()` for async messaging

**Example use case structure:**
```csharp
namespace Application.Orders;

public sealed record CreateOrderRequest(
    Guid CorrelationId,
    string Description,
    CreateOrderItemRequest[] Items,
    string CreatedBy = "",
    string TimezoneId = ""
) : BaseRequest(CorrelationId, CreatedBy, TimezoneId);

public sealed record CreateOrderItemRequest(string Name, string Description, decimal Value);

public sealed class CreateOrderItemRequestValidator : AbstractValidator<CreateOrderItemRequest>
{
    public CreateOrderItemRequestValidator()
    {
        RuleFor(r => r.Name).NotEmpty();
        RuleFor(r => r.Value).NotEmpty();
    }
}

public sealed class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(r => r.CorrelationId).NotEmpty();
        RuleFor(r => r.Description).NotEmpty();
        RuleFor(r => r.Items).NotEmpty();
        RuleForEach(r => r.Items).SetValidator(new CreateOrderItemRequestValidator());
    }
}

public sealed class CreateOrderUseCase : BaseInOutUseCase<CreateOrderRequest, OrderDto>
{
    private readonly IBaseRepository<Order> _repository;
    private readonly IProduceService _producer;
    private readonly ILogger<CreateOrderUseCase> _logger;

    public CreateOrderUseCase(
        IBaseRepository<Order> repository,
        IProduceService producer,
        IValidator<CreateOrderRequest> validator,
        ILogger<CreateOrderUseCase> logger)
        : base(validator)
    {
        _repository = repository;
        _producer = producer;
        _logger = logger;
    }

    public override async Task<Result<OrderDto>> Execute(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var orderResult = Order.Create(request.OrderNumber, request.TotalAmount);
        if (orderResult.IsFailure)
            return Result.Failure<OrderDto>(orderResult.Error);

        var order = orderResult.Value;
        await _repository.AddAsync(order, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order created: {OrderId}", order.Id);

        // Publish integration event
        await _producer.ProduceAsync(new OrderCreatedEvent { OrderId = order.Id }, cancellationToken);

        return Result.Success(new OrderDto(order.Id, order.OrderNumber, order.TotalAmount));
    }
}
```

#### DTOs
- Immutable record types: `public record OrderDto(Guid Id, string OrderNumber, decimal TotalAmount);`
- Used for Application-level responses, not entity serialization

**See also:** [Readme.md > src/Application/](Readme.md#-src-application)

### 🟢 Infrastructure Layer (`src/Infrastructure/`)

Implements ports from Application. Contains all adapters for data, caching, messaging, and telemetry.

#### Repository Pattern
- Inherit from `BaseRepository<T>` which implements `IBaseRepository<T>`
- Use EF Core `DbSet` for query/add/update/delete operations
- Automatically filters soft-deleted records via `IsDeleted` global filter

#### Entity Framework Mappings
- **File per entity:** `Data/Mapping/{Entity}Configuration.cs`
- Use Fluent API for all configuration
- Configure: primary key, columns, relationships, enums, precision, and default values
- Use `builder.HasQueryFilter(p => !p.IsDeleted)` for soft delete filtering

**Example mapping:**
```csharp
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OrderNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.Status).HasConversion<string>();
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
```

#### RabbitMQ Consumers
- Inherit from `BaseConsumer<TMessage, TConsumer>` where `TMessage` is the integration message and `TConsumer` is the consumer class itself
- Implement `HandleUseCaseAsync(IServiceProvider, TMessage, CancellationToken)` abstract method
- Consumer receives dependencies via constructor DI (logger, service scope factory, configuration)
- Automatic deduplication via `CorrelationId` cached in `IHybridCacheService`
- Dead-letter queue routing on unhandled exceptions
- Auto-registered when placed in `Infrastructure.Messaging.Consumers/` and implementing `IHostedService`

**Key built-in behaviors:**
- **Deduplication:** Uses `CorrelationId` as cache key — duplicate messages safely skipped
- **Telemetry:** Each message creates OpenTelemetry Activity span with tags; error/duplicate counters
- **Auto queue:** Both `{QueueName}` and `{QueueName}_deadLetter` queues declared on startup
- **Graceful shutdown:** Implements `IHostedService` lifecycle

**Example consumer flow:**
```
1. Message arrives via RabbitMQ
2. BaseConsumer deserialization
3. Deduplication check (correlationId cache hit → skip)
4. Call HandleUseCaseAsync → invoke use case via DI
5. On success → update cache, commit
6. On error → re-publish to deadLetter queue, log error
```

**See also:** [Readme.md > src/Infrastructure/](Readme.md#-src-infrastructure)

### 🟣 WebApp Layer (`src/WebApp/`)

Entry point for HTTP/gRPC requests. Minimal API endpoints.

#### Endpoint Patterns
- **File per resource:** `Endpoints/{Resource}Endpoints.cs`
- **Use MapGroup:** `group.MapGet("/{id}", GetById).WithName("GetById");`
- **Dependency injection:** Inject use cases directly into route handlers via `[FromServices]`
- **Correlation ID:** Read from `[FromHeader] Guid correlationId` and pass through via `Activity.Current?.AddTag("correlation-id", correlationId)`
- **Cache header:** Optional `[FromHeader] bool cacheEnabled = true` for hybrid caching
- **HTTP status codes:**
  - `GET /{id}` → `200 OK` or `404 NotFound`
  - `POST /` → `201 Created` (with Location header) or `400 BadRequest`
  - `PUT /{id}` → `200 OK` or `400 BadRequest`
  - `DELETE /{id}` → `200 OK` or `400 BadRequest`
  - `POST /paginated` → `200 OK` or `400 BadRequest`
- **Sealed classes:** Endpoint handler classes should be sealed
- **Request/Response types:** Use `BaseResponse<T>` for single items, `BasePaginatedResponse<T>` for lists

**Example endpoint structure:**
```csharp
public sealed class OrderEndpoints
{
    private const string BaseRoute = "/api/orders";

    public static void MapOrderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup(BaseRoute)
            .WithName("Orders")
            .WithTags("Orders");

        group.MapGet("/", GetAll).WithName("GetAllOrders").WithOpenApi();
        group.MapGet("/{id}", GetById).WithName("GetOrderById").WithOpenApi();
        group.MapPost("/", Create).WithName("CreateOrder").WithOpenApi();
        group.MapPut("/{id}", Update).WithName("UpdateOrder").WithOpenApi();
        group.MapDelete("/{id}", Delete).WithName("DeleteOrder").WithOpenApi();
    }

    private static async Task<IResult> Create(
        [FromBody] CreateOrderRequest request,
        [FromHeader] Guid correlationId,
        CreateOrderUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.Execute(request, cancellationToken);
        return result.IsFailure
            ? Results.BadRequest(result.Error)
            : Results.CreatedAtRoute("GetOrderById", new { id = result.Value.Id }, result.Value);
    }

    private static async Task<IResult> GetById(
        Guid id,
        [FromHeader] Guid correlationId,
        [FromHeader] bool cacheEnabled = true,
        GetOrderUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.Execute(new GetOrderRequest(id), cancellationToken);
        return result.IsFailure
            ? Results.NotFound()
            : Results.Ok(result.Value);
    }

    // ... other handlers
}
```

#### HTTP Testing File
- **Location:** `src/WebApp/WebApp.http`
- **Purpose:** Local manual endpoint testing without tools
- **Pattern:** Each endpoint has a sample request with realistic test data

#### Endpoint Registration

**Location:** `src/WebApp/Endpoints/EndpointExtensions.cs`

**Pattern:** Add one line per endpoint group
```csharp
app.MapProductEndpoints();
app.MapOrderEndpoints();
```

**Critical:** Use `internal static class` (never `public`), extension method pattern, and retrieve `IHybridCacheService` at group level

**See also:** [Readme.md > src/WebApp/](Readme.md#-src-webapp)

---

## 📂 Project File Organization

### Key Infrastructure Files

**Dependency Injection:**
- `src/Application/ApplicationDependencyInjection.cs` — Auto-registers use cases, validators
- `src/Infrastructure/InfrastructureDependencyInjection.cs` — Registers repository, cache, messaging ports
- `src/WebApp/Program.cs` — Composes all layers, defines pipeline

**Data Access:**
- `src/Infrastructure/Data/MyDbContext.cs` — EF Core context
- `src/Infrastructure/Data/Mapping/{Entity}Configuration.cs` — Entity configurations (one per entity)
- `src/Infrastructure/Data/Repositories/{Entity}Repository.cs` — Custom repository logic (if needed)
- `src/Infrastructure/Data/Migrations/` — EF Core migrations

**Messaging:**
- `src/Infrastructure/Messaging/Producers/` — Message publishing implementations
- `src/Infrastructure/Messaging/Consumers/` — RabbitMQ consumer implementations
- `src/Application/Common/Messages/` — Message record contracts

**Caching:**
- `src/Infrastructure/Cache/HybridCacheService.cs` — Redis + in-memory hybrid cache

**Observability:**
- `src/Infrastructure/OpenTelemetry/` — Tracing, metrics, logging configuration
- `src/WebApp/Middlewares/` — Request correlation, error handling middleware

**API Endpoints:**
- `src/WebApp/Endpoints/` — Minimal API endpoint definitions
- `src/WebApp/Endpoints/EndpointExtensions.cs` — Registers all MapXxxEndpoints() calls
- `src/WebApp/WebApp.http` — Local manual endpoint testing samples

**Testing:**
- `tests/CommonTests/Fixtures/` — Shared base fixtures (`BaseApplicationFixture`, `BaseHttpFixture`, `BaseMessagingFixture`)
- `tests/UnitTests/Domain/` — Domain entity and value object tests
- `tests/UnitTests/Application/` — Use case and validator tests
- `tests/IntegrationTests/WebApp/Http/` — HTTP endpoint integration tests
- `tests/IntegrationTests/WebApp/Messaging/` — Consumer/producer integration tests
- `tests/IntegrationTests/WebApp/Grpc/` — gRPC integration tests
- `tests/LoadTests/` — k6 performance scripts

---

Follow [Readme.md > Testing Strategy](Readme.md#-testing-strategy) for full details.

### Test Structure
```
tests/UnitTests/              # Isolated domain/application tests
tests/IntegrationTests/       # Full HTTP slice tests + real DB
tests/LoadTests/              # k6 performance scripts
tests/CommonTests/            # Shared fixtures (BaseApplicationFixture<T>)
```

### Unit Test Patterns

**Naming convention:** `GivenContext_WhenCondition_ThenExpectedResult`

**Base fixture:** Inherit from `BaseApplicationFixture<TRequest, TUseCase>` which provides:
- Auto-mocked `IBaseRepository<T>` for all entities
- Auto-mocked `IProduceService` for message publishing
- Auto-mocked `IHybridCacheService` for caching
- Auto-validated request via injected `IValidator<TRequest>`
- Logger injection

**Example unit test:**
```csharp
using System.Net;
using Application.Common.Requests;
using Application.Orders;
using IntegrationTests.Common;
using IntegrationTests.WebApp.Http.Common;
using WebApp;

namespace IntegrationTests.WebApp.Http.Orders;

public class CreateOrderTestFixture : BaseHttpFixture
{
    public CreateOrderRequest SetValidRequest()
    {
        var items = AutoFixture.Build<CreateOrderItemRequest>()
            .With(i => i.Value, AutoFixture.Create<decimal>() + 1)
            .CreateMany(2)
            .ToArray();

        return AutoFixture.Build<CreateOrderRequest>()
            .With(r => r.Items, items)
            .With(r => r.TimezoneId, "UTC")
            .Create();
    }

    public CreateOrderRequest SetInvalidRequest() => AutoFixture
            .Build<CreateOrderRequest>()
            .With(r => r.Description, string.Empty)
            .With(r => r.TimezoneId, "UTC")
            .Create();
}

[Collection("WebApplicationFactoryCollectionDefinition")]
public sealed class CreateOrderTest : IClassFixture<CreateOrderTestFixture>
{
    private readonly CreateOrderTestFixture _fixture;

    public CreateOrderTest(CustomWebApplicationFactory<Program> customWebApplicationFactory, CreateOrderTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.SetApiHelper(customWebApplicationFactory);
        _fixture.ResourceUrl = "orders";
    }

    [Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
    public async Task GivenAValidRequestThenPass()
    {
        // Arrange
        var request = _fixture.SetValidRequest();

        // Act
        var result = await _fixture.ApiHelper.PostAsync(_fixture.ResourceUrl, request);
        var response = await ApiHelper.DeSerializeResponse<BaseResponse<OrderDto>>(result);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        Assert.True(response!.Success);
        Assert.NotNull(response.Data);
    }

    [Fact(DisplayName = nameof(GivenAInvalidRequestThenFails))]
    public async Task GivenAInvalidRequestThenFails()
    {
        // Arrange
        var request = _fixture.SetInvalidRequest();

        // Act
        var result = await _fixture.ApiHelper.PostAsync(_fixture.ResourceUrl, request);
        var response = await ApiHelper.DeSerializeResponse<BaseResponse<OrderDto>>(result);
        // Assert
        Assert.NotNull(response);
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.False(response.Success);
        Assert.Null(response.Data);
    }
}

```

### Integration Test Patterns

Integration tests run against a real `WebApplicationFactory<Program>` with real PostgreSQL database. Tests verify the full request → response flow across all layers.

#### HTTP Integration Tests

**Structure:**
- **File location:** `tests/IntegrationTests/WebApp/Http/{Feature}/{Operation}Test.cs`
- **Base fixture:** Inherit from `IClassFixture<BaseHttpFixture>` with `[Collection("WebApplicationFactoryCollectionDefinition")]`
- **Request/Response:** Use `ApiHelper` for HTTP calls; deserialize responses with `BaseResponse<T>`
- **Database:** Real PostgreSQL; test data persisted and cleaned up between tests
- **Naming:** `GivenContext_WhenCondition_ThenExpectedResult`

**Example HTTP integration test:**
```csharp
[Collection("WebApplicationFactoryCollectionDefinition")]
public class GetOrderTest : IClassFixture<BaseHttpFixture>
{
    private readonly BaseHttpFixture _fixture;

    public GetOrderTest(CustomWebApplicationFactory<Program> customWebApplicationFactory, BaseHttpFixture fixture)
    {
        _fixture = fixture;
        _fixture.SetApiHelper(customWebApplicationFactory);
        _fixture.ResourceUrl = "orders/{0}";
    }

    [Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
    public async Task GivenAValidRequestThenPass()
    {
        // Arrange
        var id = 1;
        var url = string.Format(CultureInfo.InvariantCulture, _fixture.ResourceUrl, id);
        _fixture.ApiHelper.AddHeaders(new Dictionary<string, string>
        {
            { "CorrelationId", Guid.NewGuid().ToString() }
        });

        // Act
        var result = await _fixture.ApiHelper.GetAsync(url);
        var response = await ApiHelper.DeSerializeResponse<BaseResponse<OrderDto>>(result);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.True(response!.Success);
        Assert.NotNull(response.Data);
    }

    [Fact(DisplayName = nameof(GivenAnInvalidRequestThenFails))]
    public async Task GivenAnInvalidRequestThenFails()
    {
        // Arrange
        var id = 9999999;  // Non-existent ID
        var url = string.Format(CultureInfo.InvariantCulture, _fixture.ResourceUrl, id);
        _fixture.ApiHelper.AddHeaders(new Dictionary<string, string>
        {
            { "CorrelationId", Guid.NewGuid().ToString() }
        });

        // Act
        var result = await _fixture.ApiHelper.GetAsync(url);
        var response = await ApiHelper.DeSerializeResponse<BaseResponse<OrderDto>>(result);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.False(response!.Success);
        Assert.Null(response.Data);
    }
}
```

#### Messaging (RabbitMQ) Patterns

**Consumer message contract:**
```csharp
namespace Application.Common.Messages;

// Base contract
public abstract record BaseMessage(Guid CorrelationId, DateTime CreatedAt);

// Specific message type
public sealed record CreateNotificationMessage(
    Guid CorrelationId,
    NotificationType NotificationType,
    NotificationStatus NotificationStatus,
    string? CreatedBy = null,
    object? Message = null
) : BaseMessage(CorrelationId, DateTime.UtcNow);
```

**Consumer implementation:**
```csharp
namespace Infrastructure.Messaging.Consumers;

// Inherit from BaseConsumer<TMessage, TConsumer>
internal sealed class CreateNotificationConsumer(
    ILogger<CreateNotificationConsumer> logger,
    IServiceScopeFactory serviceScopeFactory,
    IConfiguration configuration
) : BaseConsumer<CreateNotificationMessage, CreateNotificationConsumer>(
    logger,
    serviceScopeFactory,
    configuration,
    NotificationType.OrderCreated
)
{
    // Implement Handle to call application use case
    protected override async Task HandleUseCaseAsync(
        IServiceProvider serviceProvider,
        CreateNotificationMessage message,
        CancellationToken cancellationToken
    ) => await serviceProvider
        .GetRequiredService<IBaseInUseCase<CreateNotificationRequest>>()
        .HandleAsync(new(
            message.CorrelationId,
            message.NotificationType,
            message.NotificationStatus,
            message.CreatedBy,
            message.Message
        ), cancellationToken);
}
```

**Messaging integration tests:**
- **File location:** `tests/IntegrationTests/WebApp/Messaging/{Feature}/{Operation}Test.cs`
- **Base fixture:** Inherit from `BaseMessagingFixture` with `[Collection("WebApplicationFactoryCollectionDefinition")]`
- **Message production:** Use `HandleProducerAsync()` to publish messages
- **Deduplication testing:** Verify that identical messages (same `CorrelationId`) are not processed twice
- **Database verification:** Query database directly to verify consumer side effects

**Example messaging integration test:**
```csharp
[Collection("WebApplicationFactoryCollectionDefinition")]
public sealed class CreateNotificationTest : IClassFixture<CreateNotificationTestFixture>
{
    private readonly CreateNotificationTestFixture _fixture;

    public CreateNotificationTest(CustomWebApplicationFactory<Program> factory, CreateNotificationTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.SetServices(factory);
    }

    [Fact(DisplayName = nameof(GivenAValidMessageThenPass))]
    public async Task GivenAValidMessageThenPass()
    {
        // Arrange
        var message = _fixture.SetValidMessage();

        // Act
        await _fixture.HandleProducerAsync(message, NotificationType.OrderCreated.ToString());

        // Assert - Query database to verify consumer processed the message
        var notification = await _fixture.Repository.GetQueryable<Notification>(Guid.NewGuid())
            .Where(n => n.NotificationType == message.NotificationType
                && n.CreatedBy == message.CreatedBy)
            .FirstOrDefaultAsync(_fixture.CancellationToken);

        Assert.NotNull(notification);
        Assert.Equal(message.NotificationType, notification.NotificationType);
    }

    [Fact(DisplayName = nameof(GivenADuplicateMessageThenShouldNotCreateDuplicate))]
    public async Task GivenADuplicateMessageThenShouldNotCreateDuplicate()
    {
        // Arrange
        var message = _fixture.SetValidMessage();

        // Act
        await _fixture.HandleProducerAsync(message, NotificationType.OrderCreated.ToString());
        await _fixture.HandleProducerAsync(message, NotificationType.OrderCreated.ToString());

        // Assert - Verify only one record created despite duplicate message
        var notifications = await _fixture.Repository.GetQueryable<Notification>(Guid.NewGuid())
            .Where(n => n.NotificationType == message.NotificationType
                && n.CreatedBy == message.CreatedBy)
            .ToListAsync(_fixture.CancellationToken);

        Assert.Single(notifications);  // Deduplication via CorrelationId
    }
}
```

**Custom test fixture for messaging:**
```csharp
using AutoFixture;
using Application.Common.Messages;
using IntegrationTests.WebApp.Messaging.Common;
using WebApp;

public class CreateNotificationTestFixture : BaseMessagingFixture
{
    public new void SetServices(CustomWebApplicationFactory<Program> factory)
    {
        var scope = factory.Services.CreateAsyncScope();
        SetServices(scope);
    }

    // Builder method to construct valid test messages with AutoFixture
    public CreateNotificationMessage SetValidMessage() =>
        AutoFixture.Build<CreateNotificationMessage>()
            .With(m => m.NotificationType, NotificationType.OrderCreated)
            .With(m => m.CreatedBy, $"CreatedBy-{Guid.NewGuid()}")
            .Create();
}
```

### Mutation Testing
Run via `dotnet stryker` with config files in `tests/UnitTests/`:
- `stryker-config-application.json` — validates Application layer tests catch mutations
- `stryker-config-domain.json` — validates Domain layer tests catch mutations

**Thresholds:** high ≥ 90%, low ≥ 80%, break < 50%

### Load Test Patterns

Load tests use [k6](https://k6.io) to simulate concurrent traffic and measure performance. Files located in `tests/LoadTests/`.

#### k6 Test Structure
- **File naming:** `script{Endpoint}.js` (e.g., `scriptHttp.js`, `scriptGrpc.js`)
- **Environment variables:**
  - `VUS` — Virtual users (default: 10)
  - `DURATION` — Test duration (default: 60s)
  - `WEBAPP_URL` — Application URL (default: https://localhost:7175)
  - `GRACEFUL_STOP` — Graceful shutdown period (default: 10s)
- **Metrics:** Custom counters, trends, and rates to track performance
- **Thresholds:** Performance SLOs (p95 response time, success rate, total requests)

**Example k6 HTTP load test:**
```javascript
import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter, Trend, Rate } from 'k6/metrics';

export const options = {
  scenarios: {
    get_order: {
      exec: 'getOrder',
      executor: 'constant-vus',
      vus: __ENV.VUS ? parseInt(__ENV.VUS) : 10,
      duration: __ENV.DURATION ? __ENV.DURATION : '60s',
      gracefulStop: __ENV.GRACEFUL_STOP ? __ENV.GRACEFUL_STOP : '10s'
    }
  },
  thresholds: {
    http_req_duration: ['p(50) < 100', 'p(95) < 500', 'p(99.9) < 1000'],
    http_req_failed: ['rate<0.1'],
    get_order_response_time: ['p(95) < 500'],
    get_order_success_rate: ['rate>0.95'],
    get_order_requests_total: ['count>500']
  },
};

const webappUrl = __ENV.WEBAPP_URL || 'https://localhost:7175';

const headers = {
  headers: {
    'correlationId': crypto.randomUUID(),
    'Accept': 'application/json',
    'CacheEnabled': 'false'
  }
};

const getOrderRequestsCounter = new Counter('get_order_requests_total');
const getOrderResponseTime = new Trend('get_order_response_time');
const getOrderSuccessRate = new Rate('get_order_success_rate');

export function getOrder() {
  getOrderRequestsCounter.add(1);

  const res = http.get(`${webappUrl}/api/orders/123e4567-e89b-12d3-a456-426614174000`, headers);

  getOrderResponseTime.add(res.timings.duration);

  const checkResults = check(res, {
    'status is 200': (r) => r.status === 200,
    'response has id': (r) => r.json('id') !== null,
  });

  getOrderSuccessRate.add(checkResults);

  sleep(1);
}
```

#### Running Load Tests

**Start application and backing services:**
```bash
dotnet run --project src/WebApp

# In another terminal, start backing services
docker compose -f docker-compose-local.yml up -d
```

**Run load test with defaults (10 VUs, 60 seconds):**
```bash
k6 run tests/LoadTests/scriptHttp.js --summary-mode=full
```

**Run with custom parameters:**
```bash
k6 run tests/LoadTests/scriptHttp.js \
  --env VUS=50 \
  --env DURATION=300s \
  --env WEBAPP_URL=https://localhost:7000 \
  --summary-mode=full
```

**View detailed metrics:**
```bash
k6 run tests/LoadTests/scriptHttp.js --out csv=results.csv
```

---

## 🔧 Dependency Injection and Registration

### Auto-Discovery Mechanism

The `Program.cs` uses **reflection-based auto-discovery** to register without manual configuration:

**Use cases:** Any class ending in `UseCase` that inherits from `BaseInOutUseCase<,>`, `BaseInUseCase<>`, or `BaseOutUseCase<>` is auto-registered with the appropriate interface.

**Validators:** Any class implementing `AbstractValidator<T>` is auto-registered.

**Consumers:** Any class in `Infrastructure.Messaging.Consumers` implementing `IHostedService` (base consumers inherit this) is auto-registered.

**What you get automatically:**
- Creating `CreateOrderUseCase` → registered as `IBaseInOutUseCase<CreateOrderRequest, OrderDto>`
- Creating `GetOrderUseCase` → registered as `IBaseInOutUseCase<GetOrderRequest, OrderDto>`
- Creating `DeleteOrderUseCase` → registered as `IBaseInUseCase<DeleteOrderRequest>`
- Creating `CreateOrderRequestValidator` → registered as `IValidator<CreateOrderRequest>`
- Creating `CreateNotificationConsumer` → registered as `IHostedService`, starts automatically

**No manual registration needed** for these patterns — just create the class with the right base type.

### Manual Registration

Only needed for **custom ports and adapters**. Call extension methods in `Program.cs`:
```csharp
builder.Services
    .AddDomain()
    .AddApplication()
    .AddInfrastructure(builder)
    .AddPresentation();
```

### Adding New Port (Adapter) Dependencies
1. Define port interface in Application layer (e.g., `IMyService`) under `Common/Services/`
2. Implement in Infrastructure layer (e.g., `MyService` under `Infrastructure/`)
3. Register in `InfrastructureDependencyInjection.cs`: `services.AddScoped<IMyService, MyService>();`

---

---

## 🚀 Development Workflow: Adding a New Feature

Always follow the dependency direction: **Domain → Application → Infrastructure → WebApp → Tests**.

### Step 1: Domain Entity
- **File:** `src/Domain/{Feature}/{Entity}.cs`
- **Pattern:** Factory method with invariant validation, sealed class
- **No external dependencies** — only Domain.Common references allowed
- **Aggregate root:** Use Pattern A with `Result<T>` for complex entities
- **Value object:** Use Pattern B with constructor validation for simple objects
- **Example:** `Product.cs`, `Order.cs`, `Address.cs`

### Step 2: Application Use Cases
- **Files:**
  - `src/Application/{Feature}/{OperationName}UseCase.cs` (use case + request + validator)
  - `src/Application/{Feature}/{Entity}Dto.cs` (response DTO)
- **Pattern:** Inherit from `BaseInOutUseCase<TRequest, TResponse>`, inject ports
- **Validators:** Create `{Operation}RequestValidator.cs` inheriting from `AbstractValidator<TRequest>`
- **Port usage:** Inject `IBaseRepository<T>`, `IProduceService`, `IHybridCacheService` as needed
- **Example:** `CreateProductUseCase.cs`, `GetProductUseCase.cs`, `UpdateProductUseCase.cs`

### Step 3: Infrastructure Persistence
- **EF Core mapping:** `src/Infrastructure/Data/Mapping/{Entity}Configuration.cs`
  - Implement `IEntityTypeConfiguration<T>`
  - Configure key, columns, constraints, enums, precision, relationships
  - Always add `builder.HasQueryFilter(p => !p.IsDeleted)` for soft deletes
- **Custom repository** (if needed): `src/Infrastructure/Data/Repositories/{Entity}Repository.cs`
- **Migration:**
  ```bash
  dotnet ef migrations add Add{Feature} --project src/Infrastructure --startup-project src/Infrastructure --output-dir Data/Migrations
  dotnet ef database update --project src/Infrastructure --startup-project src/Infrastructure
  ```

### Step 4: WebApp Endpoints
- **File:** `src/WebApp/Endpoints/{Feature}Endpoints.cs`
- **Pattern:**
  - Use `MapGroup` with resource name
  - Inject use cases via `[FromServices]`
  - Include correlation ID in all endpoints
  - Use proper HTTP status codes (200, 201, 404, 400)
  - Return `BaseResponse<T>` or `BasePaginatedResponse<T>`
- **HTTP samples:** Add to `src/WebApp/WebApp.http` with realistic test data
- **Registration:** Add one line to `src/WebApp/Endpoints/EndpointExtensions.cs`
- **Example:** `ProductEndpoints.cs` with GET /{id}, POST /, PUT /{id}, DELETE /{id}

### Step 5: Tests (all layers)
- **Domain tests:** `tests/UnitTests/Domain/{Feature}/{Entity}Tests.cs`
  - Test factory methods and business operations
  - Test invariant violations
  - Test soft delete behavior
- **Application tests:** `tests/UnitTests/Application/{Feature}/{Operation}UseCaseTests.cs`
  - Test with mocked repository via `BaseApplicationFixture`
  - Test validation, success, and failure paths
  - Test producer/cache calls
- **Integration tests:** `tests/IntegrationTests/WebApp/Http/{Feature}/{Operation}Test.cs`
  - Test against real database via `WebApplicationFactory`
  - Test valid and invalid request scenarios
  - Test HTTP status codes and response payloads

**Architecture tests** in `tests/UnitTests/Architecture/` automatically validate dependency rules.

---

## 🛠️ Build and Test Commands

### Build
```bash
dotnet build
```

### Run
```bash
dotnet run --project src/WebApp
# Listens on https://localhost:7000
# Swagger: https://localhost:7000/swagger
# Health: https://localhost:7000/health
```

### Test All Layers
```bash
dotnet test
```

### Test Specific Layer
```bash
dotnet test tests/UnitTests
dotnet test tests/IntegrationTests
```

### Mutation Testing
```bash
cd tests/UnitTests
dotnet stryker --config-file stryker-config-application.json
dotnet stryker --config-file stryker-config-domain.json
```

### Load Testing
```bash
# Ensure backing services are running
docker compose -f docker-compose-local.yml up -d

# Run load test
k6 run tests/LoadTests/scriptHttp.js --summary-mode=full
```

### Docker Setup
```bash
# Start all backing services (PostgreSQL, Redis, RabbitMQ, monitoring)
docker compose -f docker-compose-local.yml up -d

# Stop
docker compose -f docker-compose-local.yml down
```

---

## 🤖 AI Agent Skills

Skills automate code generation and enforce conventions. They contain detailed templates, reference patterns, and validation checklists. Invoke them when appropriate:

| Skill | Activation Triggers | Purpose |
|---|---|---|
| **Entity Generator** | User requests new entity, domain object, aggregate, or value object; asks to add business rules/validation to entity | Generate domain entities with DDD patterns, factory methods (Pattern A/B), validation, unit tests, and migration guidance |
| **Use Case Generator** | User requests use case; asks to implement CRUD/Delete operations; mentions `BaseInOutUseCase` or `BaseInUseCase` | Generate Application use cases with request records, FluentValidation validators, repository calls, messaging patterns, and xUnit tests |
| **Endpoint Generator** | User requests API endpoint, HTTP route, or CRUD endpoints; asks to expose use case via HTTP | Generate Minimal API endpoints with correlation ID support, caching headers, proper HTTP status codes, request/response types, and `.http` samples |
| **Consumer Generator** | User requests message consumer, queue listener, or RabbitMQ handler; asks to consume events or handle messages | Generate RabbitMQ consumers using `BaseConsumer<TMessage>` with message contracts, deduplication logic, auto-registration, and integration tests |
| **EF Mapper Generator** | User adds/changes entity properties; introduces enums, decimals, or relationships; asks for DB configuration | Generate EF Core mapping classes with Fluent API, enum conversions, decimal precision, relationships, and migration commands |
| **Integration Test Generator** | User asks for integration tests for HTTP/gRPC/messaging; wants WebApplicationFactory-based tests; requests end-to-end validation | Generate HTTP/gRPC/messaging integration tests with proper fixtures, realistic scenarios, assertions, deduplication testing, and database verification |
| **Load Test Generator** | User asks for load tests, performance benchmarks, k6 scripts, or SLA validation | Generate k6 load test scripts with scenarios, custom metrics, thresholds, environment-driven profiles, and run commands |
| **Code Review** | User asks for code review; wants to validate architecture, tests, complexity, or security before merge | Full code review against DDD/SOLID standards, cyclomatic complexity ≤ 30, modern C#, test coverage, and security gates |

### Skill Usage Examples

```
"Create a new domain entity Product with name, price, and SKU"
→ `/create-entity`
→ Generates: Entity with factory (Pattern A), validators, unit tests, migration guidance

"Generate a use case to create a product with validation and messaging"
→ `/create-use-case`
→ Generates: CreateProductUseCase, CreateProductRequest, validator, tests with mocked repos/producers

"Add HTTP endpoints for Products CRUD with caching"
→ `/create-endpoint`
→ Generates: ProductEndpoints.cs with GET/{id}, POST /, PUT/{id}, DELETE/{id}, WebApp.http samples

"Create a RabbitMQ consumer for OrderCreatedEvent"
→ `/create-consumer`
→ Generates: Consumer, message contract record, integration tests, deduplication test

"Generate EF Core mappings for Product with enum conversion"
→ `/create-ef-mapper`
→ Generates: ProductConfiguration.cs with Fluent API, precision rules, migration commands

"Write integration tests for the Products endpoint"
→ `/create-integration-test`
→ Generates: HTTP test with valid/invalid scenarios, fixtures, database assertions

"Create k6 load tests for the GET products endpoint"
→ `/create-load-test`
→ Generates: k6 script with scenarios, metrics, thresholds, run commands

"Review this code change against our architecture"
→ `/code-review`
→ Performs full review: DDD/SOLID, tests, complexity, security, modern C# patterns
```

---

## ✅ Code Quality Standards

The template enforces strict quality gates for all contributions:

### Architecture Standards (DDD/SOLID)
- **Domain layer purity:** No infrastructure concerns, only business logic and invariants
- **Dependency direction:** Always inward (Domain → Application → Infrastructure → WebApp)
- **Single Responsibility:** Each class has one reason to change
- **Ports pattern:** Infrastructure implements Application-defined interfaces only
- **Sealed by default:** Classes/records are sealed unless inheritance is intentional

### Testing Standards
- **Unit tests required:** Domain entities, use cases, validators must have unit tests
- **Integration tests required:** HTTP/gRPC endpoints must have integration tests
- **Test naming:** `GivenContext_WhenCondition_ThenExpectedResult` pattern
- **Fixture patterns:** Use `BaseApplicationFixture<TRequest, TUseCase>` for mocked tests, `BaseHttpFixture` for HTTP tests, `BaseMessagingFixture` for messaging tests
- **Coverage expectations:** Critical business logic should have high coverage; mutation test survival rate ≥ 80%

### Code Complexity Standards
- **Hard limit:** Cyclomatic complexity ≤ 30 per method
- **Refactor triggers:** Nested branching, multiple conditions, long methods
- **Strategies:** Extract methods, use pattern matching, split logic into domain methods

### Modern C# Standards
- **File-scoped namespaces:** Always use `namespace X;` not `namespace X { }`
- **Sealed records/classes:** Default sealed, open only when needed
- **Collection expressions:** Use `[1, 2, 3]` instead of `new[] { 1, 2, 3 }`
- **Async/await:** Required for I/O paths; use `CancellationToken` parameter
- **Pattern matching:** Use switch expressions and is patterns for clarity
- **Records for DTOs:** Immutable record types for Application responses

### Security Standards
- **No hardcoded credentials:** All config via environment variables or configuration
- **Input validation:** Validate all requests at Application layer boundary
- **Safe logging:** Never log sensitive data; no PII in logs
- **Exception handling:** Catch specific exceptions; never suppress without reason

---

1. **Sealed classes by default:** Domain entities, use cases, endpoints should all be sealed unless inheritance is intentional
2. **No circular dependencies:** Respect the dependency flow (inward only)
3. **Result<T> pattern:** Use for expected errors, not exceptions
4. **Soft deletes only:** `IsDeleted` flag, no permanent deletes; EF query filter auto-applies
5. **Correlation ID threading:** Always include in endpoint handlers and log it
6. **Test naming:** `GivenContext_WhenCondition_ThenExpectedResult` pattern
7. **HTTP samples:** When adding endpoints, update `src/WebApp/WebApp.http` with test requests
8. **Validation at boundary:** FluentValidation runs automatically in use cases
9. **Repository injection:** Always inject `IBaseRepository<T>` port, not concrete `DbContext`
10. **Message deduplication:** Consumers auto-handle via `CorrelationId` in `BaseConsumer<TMessage, TConsumer>`

---

## 📖 Additional Resources

- [Readme.md](Readme.md) — Full project documentation, architecture explanation, commands
- [TEST_STRUCTURE_GUIDE.md](TEST_STRUCTURE_GUIDE.md) — Detailed testing patterns and conventions
- [Hexagonal Architecture](https://alistair.cockburn.us/hexagonal-architecture/) — Conceptual overview
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html) — DDD principles

---

**Last updated:** May 2026
**Template version:** Full Hexagonal Architecture
**Required .NET SDK:** 9.0+
