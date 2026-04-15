---
name: use-case-tests-generator
description: 'Generate xUnit use case tests for the hexagonal architecture template project following project test infrastructure patterns'
---

# Use Case Tests Generator — Hexagonal Architecture Template

Generate xUnit unit tests for Application layer use cases following the project's test infrastructure, fixture patterns, and naming conventions.

## When to Use

Activate this skill when:
- User requests unit tests for a new or existing use case
- User asks to test `BaseInOutUseCase`, `BaseInUseCase`, or `BaseOutUseCase` implementations
- User mentions "use case tests", "application tests", or "HandleAsync tests"
- User asks to add test coverage for Create, Get, GetAll, Update, or Delete use cases
- User asks to test FluentValidation request validators

---

## Skill-local References

| File | Purpose |
|------|---------|
| `./references/fixture-cheatsheet.md` | Quick map of fixture helpers and verification methods |
| `./references/test-class-template.md` | Reusable class template for new use case tests |

> Keep this skill resilient: use these local references instead of external file links.

---

## Test File Structure

Every use case test file follows this structure (all classes in one file):

```
1. [Optional] Request Validator Fixture class
2. [Optional] Request Validation Tests class
3. Use Case Fixture class  (extends BaseApplicationFixture<TRequest, TUseCase>)
4. Use Case Tests class    (implements IClassFixture<TUseCaseFixture>)
```

---

## Naming Conventions

- **Test methods**: `GivenContext_WhenCondition_ThenExpectedResult` — no underscores inside segments
  - `GivenAValidRequestThenPass`
  - `GivenAnInvalidRequestThenFails`
  - `GivenAValidRequestWhenOrderNotFoundThenFails`
  - `GivenAValidRequestWhenRepositoryReturnsZeroThenFails`
- **All `[Fact]` attributes must include `DisplayName = nameof(MethodName)`**
- **Fixture classes**: `{UseCaseName}Fixture` (e.g., `CreateOrderUseCaseFixture`)
- **Test classes**: `{UseCaseName}Test` (e.g., `CreateOrderUseCaseTest`)
- **Validation fixture**: `{RequestName}ValidationFixture`
- **Validation tests**: `{RequestName}ValidationTests`

---

## Test Infrastructure API

### BaseApplicationFixture<TRequest, TUseCase>

```csharp
// Available properties
Mock<IServiceProvider> MockServiceProvider
Mock<ILogger>          MockLogger
Mock<ILoggerFactory>   MockLoggerFactory
Mock<IProduceService>  MockProduceService
Mock<IBaseRepository>  MockRepository
Mock<IValidator<TRequest>> MockValidator
Mock<IHybridCacheService>  MockCache
TUseCase UseCase  // set in fixture constructor

// Available methods
void ClearInvocations()
void SetSuccessfulValidator(TRequest request)
void SetFailedValidator(TRequest request)
BasePaginatedRequest SetValidBasePaginatedRequest()  // for GetAll use cases
void SetValidGetOrCreateAsync<TResult>(TResult result)
void SetInvalidGetOrCreateAsync<TResult>()
void VerifyCache<TResult>(int times)
void VerifyProduce<TMessage>(int times = 1)
CancellationToken CancellationToken  // from BaseFixture
IFixture AutoFixture                  // from BaseFixture
```

### RepositoryMockExtensions

```csharp
// Add
mockRepository.SetSuccessfulAddAsync<TEntity>()    // returns 1
mockRepository.SetFailedAddAsync<TEntity>()         // returns 0
mockRepository.VerifyAddAsync<TEntity>(int times)

// Update
mockRepository.SetSuccessfulUpdate<TEntity>()       // returns 1
mockRepository.SetFailedUpdate<TEntity>()           // returns 0
mockRepository.VerifyUpdate<TEntity>(int times)

// Query (GetQueryable — used by Get and Delete use cases)
mockRepository.SetupQueryable<TEntity>(ICollection<TEntity> entities)
mockRepository.SetupQueryable<TEntity>(Guid correlationId, bool? newContext, ICollection<TEntity> entities)
mockRepository.VerifyQueryable<TEntity>(int times = 1)

// GetAll paginated
mockRepository.SetValidGetAllPaginatedAsyncNoIncludes<TEntity, TDto>(IEnumerable<TDto> data, int totalRecords)
mockRepository.SetInvalidGetAllPaginatedAsync<TEntity, TDto>()
mockRepository.VerifyGetAllPaginatedNoIncludes<TEntity, TDto>(int times = 1)
```

### LogMockExtensions

```csharp
mockLogger.VerifyStartOperation(int times = 1)     // "Starting operation"
mockLogger.VerifyFinishOperation(int times = 1)    // "Finished operation"
mockLogger.VerifyNotFound(int times = 1)           // "not found."
mockLogger.VerifyOperationFailed(int times = 1)    // "Failed operation"
mockLogger.VerifyInformation(string message, int times = 1)
mockLogger.VerifyWarning(string message, int times = 1)
mockLogger.VerifyError(string message, int times = 1)
mockLogger.VerifyDebug(string message, int times = 1)
```

---

## Pattern 1 — BaseInOutUseCase (Create)

Use this pattern for **create** use cases that take a request and return a typed response.

```csharp
using Application.Common.Messages;
using Application.Orders;
using Domain.Orders;
using FluentValidation;
using FluentValidation.TestHelper;
using UnitTests.Application.Common;

namespace UnitTests.Application.Orders;

// ── 1. Validator Fixture ────────────────────────────────────────────────────

public sealed class CreateOrderRequestValidationFixture
{
    public IValidator<CreateOrderRequest> Validator { get; } = new CreateOrderRequestValidator();

    public static CreateOrderRequest GetValidRequest() => new(Guid.NewGuid(), "new order", [
        new("item1", "description1", 10.0m),
        new("item2", "description2", 20.0m)
    ]);
}

// ── 2. Validator Tests ──────────────────────────────────────────────────────

public sealed class CreateOrderRequestValidationTests(CreateOrderRequestValidationFixture fixture)
    : IClassFixture<CreateOrderRequestValidationFixture>
{
    private readonly CreateOrderRequestValidationFixture _fixture = fixture;

    [Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
    public async Task GivenAValidRequestThenPass()
    {
        // Arrange
        var request = CreateOrderRequestValidationFixture.GetValidRequest();

        // Act
        var result = await _fixture.Validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = nameof(GivenAnInvalidRequestThenFails))]
    public async Task GivenAnInvalidRequestThenFails()
    {
        // Arrange
        var request = CreateOrderRequestValidationFixture.GetValidRequest() with
        {
            CorrelationId = Guid.Empty,
            Description = string.Empty,
            Items = []
        };

        // Act
        var result = await _fixture.Validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor("CorrelationId");
        result.ShouldHaveValidationErrorFor("Description");
        result.ShouldHaveValidationErrorFor("Items");
    }

    [Fact(DisplayName = nameof(GivenAnInvalidItemThenFails))]
    public async Task GivenAnInvalidItemThenFails()
    {
        // Arrange
        var request = CreateOrderRequestValidationFixture.GetValidRequest() with
        {
            Items = [new("", "invalid item", 0m)]
        };

        // Act
        var result = await _fixture.Validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor("Items[0].Name");
        result.ShouldHaveValidationErrorFor("Items[0].Value");
    }
}

// ── 3. Use Case Fixture ─────────────────────────────────────────────────────

public sealed class CreateOrderUseCaseFixture : BaseApplicationFixture<CreateOrderRequest, CreateOrderUseCase>
{
    public CreateOrderUseCaseFixture() => UseCase = new(MockServiceProvider.Object);

    public CreateOrderRequest SetValidRequest()
    {
        var items = AutoFixture.CreateMany<CreateOrderItemRequest>(1);
        return new(Guid.NewGuid(), "AwesomeComputer", [.. items]);
    }

    public static CreateOrderRequest SetInvalidRequestWithNoItems() =>
        new(Guid.NewGuid(), "AwesomeComputer", []);
}

// ── 4. Use Case Tests ───────────────────────────────────────────────────────

public sealed class CreateOrderUseCaseTest : IClassFixture<CreateOrderUseCaseFixture>
{
    private readonly CreateOrderUseCaseFixture _fixture;

    public CreateOrderUseCaseTest(CreateOrderUseCaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearInvocations();  // always reset mocks between tests
    }

    [Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
    public async Task GivenAValidRequestThenPass()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetSuccessfulAddAsync<Order>();

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Null(result.Message);

        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation();
        _fixture.MockRepository.VerifyAddAsync<Order>(1);
        _fixture.VerifyProduce<CreateNotificationMessage>();
    }

    [Fact(DisplayName = nameof(GivenAnInvalidRequestThenFails))]
    public async Task GivenAnInvalidRequestThenFails()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetFailedValidator(request);

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
        Assert.NotEmpty(result.Message);

        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation(0);
        _fixture.MockRepository.VerifyAddAsync<Order>(0);
        _fixture.VerifyProduce<CreateNotificationMessage>(0);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenRepositoryReturnsZeroThenFails))]
    public async Task GivenAValidRequestWhenRepositoryReturnsZeroThenFails()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetFailedAddAsync<Order>();

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
        Assert.NotEmpty(result.Message);

        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation();
        _fixture.MockLogger.VerifyWarning("Failed to create order.", 1);
        _fixture.MockRepository.VerifyAddAsync<Order>(1);
        _fixture.VerifyProduce<CreateNotificationMessage>(0);
    }

    // Optional: test domain-level guard (e.g., no items)
    [Fact(DisplayName = nameof(GivenAInvalidRequestThenFailsWhenThereIsNoItems))]
    public async Task GivenAInvalidRequestThenFailsWhenThereIsNoItems()
    {
        // Arrange
        var request = CreateOrderUseCaseFixture.SetInvalidRequestWithNoItems();
        _fixture.SetSuccessfulValidator(request);

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Order must have at least one item.", result.Message);

        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation();
        _fixture.MockLogger.VerifyWarning("Order must have at least one item.", 1);
        _fixture.MockRepository.VerifyAddAsync<Order>(0);
    }
}
```

---

## Pattern 2 — BaseInOutUseCase (Get by Id)

Use this pattern for **get single entity** use cases that query the repository and can return not-found.

```csharp
using Application.Orders;
using Domain.Orders;
using UnitTests.Application.Common;

namespace UnitTests.Application.Orders;

// ── 3. Use Case Fixture ─────────────────────────────────────────────────────

public sealed class GetOrderUseCaseFixture : BaseApplicationFixture<GetOrderRequest, GetOrderUseCase>
{
    public GetOrderUseCaseFixture() => UseCase = new(MockServiceProvider.Object);

    public GetOrderRequest SetValidRequest(int? id = null) =>
        new(Guid.NewGuid(), id ?? AutoFixture.Create<int>());
}

// ── 4. Use Case Tests ───────────────────────────────────────────────────────

public sealed class GetOrderUseCaseTest : IClassFixture<GetOrderUseCaseFixture>
{
    private readonly GetOrderUseCaseFixture _fixture;

    public GetOrderUseCaseTest(GetOrderUseCaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearInvocations();
    }

    [Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
    public async Task GivenAValidRequestThenPass()
    {
        // Arrange
        var order = Order.Create("Test Order", [new("Item 1", "Desc 1", 500m)]).Value;
        var request = _fixture.SetValidRequest(order.Id);
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetupQueryable(request.CorrelationId, null, [order]);

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Null(result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(order.Id, result.Data.Id);

        _fixture.MockRepository.VerifyQueryable<Order>();
        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyNotFound(0);
        _fixture.MockLogger.VerifyFinishOperation();
    }

    [Fact(DisplayName = nameof(GivenAnInvalidRequestThenFails))]
    public async Task GivenAnInvalidRequestThenFails()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetFailedValidator(request);

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
        Assert.NotEmpty(result.Message);

        _fixture.MockRepository.VerifyQueryable<Order>(0);
        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyNotFound(0);
        _fixture.MockLogger.VerifyFinishOperation(0);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenOrderNotFoundThenFails))]
    public async Task GivenAValidRequestWhenOrderNotFoundThenFails()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetupQueryable<Order>(request.CorrelationId, null, []);

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Order not found.", result.Message);

        _fixture.MockRepository.VerifyQueryable<Order>();
        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyNotFound(1);
        _fixture.MockLogger.VerifyFinishOperation();
    }
}
```

---

## Pattern 3 — BaseInOutUseCase (GetAll Paginated)

Use this pattern for **paginated list** use cases. The request type is `BasePaginatedRequest`.

```csharp
using Application.Common.Requests;
using Application.Orders;
using Domain.Orders;
using UnitTests.Application.Common;

namespace UnitTests.Application.Orders;

// ── 3. Use Case Fixture ─────────────────────────────────────────────────────

public sealed class GetAllOrdersUseCaseFixture : BaseApplicationFixture<BasePaginatedRequest, GetAllOrdersUseCase>
{
    public GetAllOrdersUseCaseFixture() => UseCase = new(MockServiceProvider.Object);
    // Uses SetValidBasePaginatedRequest() inherited from BaseApplicationFixture
}

// ── 4. Use Case Tests ───────────────────────────────────────────────────────

public sealed class GetAllOrdersUseCaseTest : IClassFixture<GetAllOrdersUseCaseFixture>
{
    private readonly GetAllOrdersUseCaseFixture _fixture;

    public GetAllOrdersUseCaseTest(GetAllOrdersUseCaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearInvocations();
    }

    [Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
    public async Task GivenAValidRequestThenPass()
    {
        // Arrange
        var totalRecords = 5;
        var request = _fixture.SetValidBasePaginatedRequest();
        _fixture.SetSuccessfulValidator(request);
        var expectedOrders = _fixture.AutoFixture.CreateMany<OrderDto>(totalRecords);
        _fixture.MockRepository.SetValidGetAllPaginatedAsyncNoIncludes<Order, OrderDto>(expectedOrders, totalRecords);

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Null(result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(expectedOrders.Count(), result.Data.Count());
        Assert.Equal(1, result.TotalPages);
        Assert.Equal(totalRecords, result.TotalRecords);

        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockRepository.VerifyGetAllPaginatedNoIncludes<Order, OrderDto>(1);
        _fixture.MockLogger.VerifyNotFound(0);
        _fixture.MockLogger.VerifyFinishOperation();
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenNoOrdersFoundThenFails))]
    public async Task GivenAValidRequestWhenNoOrdersFoundThenFails()
    {
        // Arrange
        var request = _fixture.SetValidBasePaginatedRequest();
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetInvalidGetAllPaginatedAsync<Order, OrderDto>();

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("No orders found.", result.Message);

        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockRepository.VerifyGetAllPaginatedNoIncludes<Order, OrderDto>(1);
        _fixture.MockLogger.VerifyNotFound(1);
        _fixture.MockLogger.VerifyFinishOperation();
    }

    [Fact(DisplayName = nameof(GivenAnInvalidRequestThenFails))]
    public async Task GivenAnInvalidRequestThenFails()
    {
        // Arrange
        var request = _fixture.SetValidBasePaginatedRequest();
        _fixture.SetFailedValidator(request);

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
        Assert.NotEmpty(result.Message);

        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockRepository.VerifyGetAllPaginatedNoIncludes<Order, OrderDto>(0);
        _fixture.MockLogger.VerifyNotFound(0);
        _fixture.MockLogger.VerifyFinishOperation(0);
    }
}
```

---

## Pattern 4 — BaseInOutUseCase (Update)

Use this pattern for **update** use cases that query, mutate, then save the entity.

```csharp
using Application.Common.Messages;
using Application.Orders;
using Domain.Orders;
using UnitTests.Application.Common;

namespace UnitTests.Application.Orders;

// ── 3. Use Case Fixture ─────────────────────────────────────────────────────

public sealed class UpdateOrderUseCaseFixture : BaseApplicationFixture<UpdateOrderRequest, UpdateOrderUseCase>
{
    public UpdateOrderUseCaseFixture() => UseCase = new(MockServiceProvider.Object);

    public UpdateOrderRequest SetValidRequest(int orderId = 1)
    {
        var items = AutoFixture.CreateMany<UpdateOrderItemRequest>(1);
        return new(Guid.NewGuid(), orderId, "Updated Order", [.. items]);
    }

    public static Order CreateOrder() => Order.Create(
        "Original Order",
        [new("Item 1", "Desc 1", 100m)],
        "System"
    ).Value;
}

// ── 4. Use Case Tests ───────────────────────────────────────────────────────

public sealed class UpdateOrderUseCaseTest : IClassFixture<UpdateOrderUseCaseFixture>
{
    private readonly UpdateOrderUseCaseFixture _fixture;

    public UpdateOrderUseCaseTest(UpdateOrderUseCaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearInvocations();
    }

    [Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
    public async Task GivenAValidRequestThenPass()
    {
        // Arrange
        var order = UpdateOrderUseCaseFixture.CreateOrder();
        var request = _fixture.SetValidRequest(order.Id);
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetupQueryable(request.CorrelationId, null, [order]);
        _fixture.MockRepository.SetSuccessfulUpdate<Order>();

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Null(result.Message);
        Assert.NotNull(result.Data);

        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation();
        _fixture.MockLogger.VerifyNotFound(0);
        _fixture.MockLogger.VerifyOperationFailed(0);
        _fixture.MockRepository.VerifyQueryable<Order>();
        _fixture.MockRepository.VerifyUpdate<Order>(1);
        _fixture.VerifyProduce<CreateNotificationMessage>();
    }

    [Fact(DisplayName = nameof(GivenAnInvalidRequestThenFails))]
    public async Task GivenAnInvalidRequestThenFails()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetFailedValidator(request);

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
        Assert.NotEmpty(result.Message);

        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation(0);
        _fixture.MockRepository.VerifyQueryable<Order>(0);
        _fixture.MockRepository.VerifyUpdate<Order>(0);
        _fixture.VerifyProduce<CreateNotificationMessage>(0);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenOrderNotFoundThenFails))]
    public async Task GivenAValidRequestWhenOrderNotFoundThenFails()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetupQueryable<Order>(request.CorrelationId, null, []);

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Order not found.", result.Message);

        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation();
        _fixture.MockLogger.VerifyNotFound(1);
        _fixture.MockRepository.VerifyQueryable<Order>();
        _fixture.MockRepository.VerifyUpdate<Order>(0);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenRepositoryReturnsZeroThenFails))]
    public async Task GivenAValidRequestWhenRepositoryReturnsZeroThenFails()
    {
        // Arrange
        var order = UpdateOrderUseCaseFixture.CreateOrder();
        var request = _fixture.SetValidRequest(order.Id);
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetupQueryable(request.CorrelationId, null, [order]);
        _fixture.MockRepository.SetFailedUpdate<Order>();

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.False(result.Success);

        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation();
        _fixture.MockLogger.VerifyOperationFailed(1);
        _fixture.MockRepository.VerifyQueryable<Order>();
        _fixture.MockRepository.VerifyUpdate<Order>(1);
    }
}
```

---

## Pattern 5 — BaseInOutUseCase (Delete / Soft Delete)

Delete use cases fetch the entity, apply domain logic (e.g., `DeleteOrder()`), then call `UpdateAsync`. Test the state-check guard.

```csharp
using Application.Common.Messages;
using Application.Orders;
using Domain.Orders;
using UnitTests.Application.Common;

namespace UnitTests.Application.Orders;

// ── 3. Use Case Fixture ─────────────────────────────────────────────────────

public sealed class DeleteOrderUseCaseFixture : BaseApplicationFixture<DeleteOrderRequest, DeleteOrderUseCase>
{
    public DeleteOrderUseCaseFixture() => UseCase = new(MockServiceProvider.Object);

    public static DeleteOrderRequest SetValidRequest(int orderId = 1) =>
        new(Guid.NewGuid(), orderId, "admin");

    public static Order CreateOrder() => Order.Create(
        "Order to Delete",
        [new("Item 1", "Desc 1", 100m)],
        "System"
    ).Value;
}

// ── 4. Use Case Tests ───────────────────────────────────────────────────────

public sealed class DeleteOrderUseCaseTest : IClassFixture<DeleteOrderUseCaseFixture>
{
    private readonly DeleteOrderUseCaseFixture _fixture;

    public DeleteOrderUseCaseTest(DeleteOrderUseCaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearInvocations();
    }

    [Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
    public async Task GivenAValidRequestThenPass()
    {
        // Arrange
        var order = DeleteOrderUseCaseFixture.CreateOrder();
        var request = DeleteOrderUseCaseFixture.SetValidRequest(order.Id);
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetupQueryable(request.CorrelationId, null, [order]);
        _fixture.MockRepository.SetSuccessfulUpdate<Order>();

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Null(result.Message);

        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation();
        _fixture.MockLogger.VerifyNotFound(0);
        _fixture.MockLogger.VerifyOperationFailed(0);
        _fixture.MockRepository.VerifyQueryable<Order>();
        _fixture.MockRepository.VerifyUpdate<Order>(1);
        _fixture.VerifyProduce<CreateNotificationMessage>();
    }

    [Fact(DisplayName = nameof(GivenAnInvalidRequestThenFails))]
    public async Task GivenAnInvalidRequestThenFails()
    {
        // Arrange
        var request = DeleteOrderUseCaseFixture.SetValidRequest();
        _fixture.SetFailedValidator(request);

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
        Assert.NotEmpty(result.Message);

        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation(0);
        _fixture.MockRepository.VerifyQueryable<Order>(0);
        _fixture.MockRepository.VerifyUpdate<Order>(0);
        _fixture.VerifyProduce<CreateNotificationMessage>(0);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenOrderNotFoundThenFails))]
    public async Task GivenAValidRequestWhenOrderNotFoundThenFails()
    {
        // Arrange
        var request = DeleteOrderUseCaseFixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetupQueryable<Order>(request.CorrelationId, null, []);

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Order not found.", result.Message);

        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation();
        _fixture.MockLogger.VerifyNotFound(1);
        _fixture.MockRepository.VerifyQueryable<Order>();
        _fixture.MockRepository.VerifyUpdate<Order>(0);
        _fixture.VerifyProduce<CreateNotificationMessage>();
    }

    // Guard: already deleted — must test the domain state check
    [Fact(DisplayName = nameof(GivenAValidRequestWhenOrderAlreadyDeletedThenFails))]
    public async Task GivenAValidRequestWhenOrderAlreadyDeletedThenFails()
    {
        // Arrange
        var order = DeleteOrderUseCaseFixture.CreateOrder();
        order.DeleteOrder("System");  // put entity into deleted state

        var request = DeleteOrderUseCaseFixture.SetValidRequest(order.Id);
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetupQueryable(request.CorrelationId, null, [order]);

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Order is already deleted.", result.Message);

        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation();
        _fixture.MockLogger.VerifyOperationFailed(1);
        _fixture.MockRepository.VerifyUpdate<Order>(0);
        _fixture.VerifyProduce<CreateNotificationMessage>();
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenRepositoryReturnsZeroThenFails))]
    public async Task GivenAValidRequestWhenRepositoryReturnsZeroThenFails()
    {
        // Arrange
        var order = DeleteOrderUseCaseFixture.CreateOrder();
        var request = DeleteOrderUseCaseFixture.SetValidRequest(order.Id);
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetupQueryable(request.CorrelationId, null, [order]);
        _fixture.MockRepository.SetFailedUpdate<Order>();

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Failed to delete order.", result.Message);

        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation();
        _fixture.MockLogger.VerifyOperationFailed(1);
        _fixture.MockRepository.VerifyQueryable<Order>();
        _fixture.MockRepository.VerifyUpdate<Order>(1);
        _fixture.VerifyProduce<CreateNotificationMessage>();
    }
}
```

---

## Pattern 6 — BaseInUseCase (Fire and Forget, No Response)

`BaseInUseCase<TRequest>` use cases return `void` (Task). There is no `result.Success` to assert on — verify behavior through repository and logger mocks.

```csharp
using Application.Products;
using Domain.Products;
using UnitTests.Application.Common;

namespace UnitTests.Application.Products;

// ── 3. Use Case Fixture ─────────────────────────────────────────────────────

public sealed class SyncProductUseCaseFixture : BaseApplicationFixture<SyncProductRequest, SyncProductUseCase>
{
    public SyncProductUseCaseFixture() => UseCase = new(MockServiceProvider.Object);

    public static SyncProductRequest SetValidRequest() => new(Guid.NewGuid(), 42);
}

// ── 4. Use Case Tests ───────────────────────────────────────────────────────

public sealed class SyncProductUseCaseTest : IClassFixture<SyncProductUseCaseFixture>
{
    private readonly SyncProductUseCaseFixture _fixture;

    public SyncProductUseCaseTest(SyncProductUseCaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearInvocations();
    }

    [Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
    public async Task GivenAValidRequestThenPass()
    {
        // Arrange
        var request = SyncProductUseCaseFixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetSuccessfulAddAsync<Product>();

        // Act
        // BaseInUseCase returns Task (void) — no result to assert
        await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert — verify side effects via mocks
        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation();
        _fixture.MockRepository.VerifyAddAsync<Product>(1);
    }

    [Fact(DisplayName = nameof(GivenAnInvalidRequestThenDoesNothing))]
    public async Task GivenAnInvalidRequestThenDoesNothing()
    {
        // Arrange
        var request = SyncProductUseCaseFixture.SetValidRequest();
        _fixture.SetFailedValidator(request);

        // Act
        await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert — FinishOperation is NOT called on validation failure
        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation(0);
        _fixture.MockRepository.VerifyAddAsync<Product>(0);
    }
}
```

---

## Required Test Scenarios by Use Case Type

| Use Case Type | Required Scenarios |
|---|---|
| **Create** | Valid request passes, Invalid request fails (validator), Repository returns 0 (fails), Domain guard violation (if any) |
| **Get by Id** | Valid + entity found, Invalid request (validator fails), Entity not found |
| **GetAll** | Valid + records found, Valid + no records found, Invalid request (validator fails) |
| **Update** | Valid request passes (data returned), Invalid request (validator fails), Entity not found, Repository returns 0 (fails), Optional: domain state guard |
| **Delete** | Valid request passes, Invalid request (validator fails), Entity not found, Entity already deleted (if soft-delete), Repository returns 0 (fails) |
| **BaseInUseCase** | Valid request (verify side effects), Invalid request (verify nothing happens) |

---

## Implementation Rules

### Always

- Call `_fixture.ClearInvocations()` in the test class constructor
- Add `[Fact(DisplayName = nameof(MethodName))]` on every test
- Assert both `result.Success` **and** `result.Message` for failure cases
- Call `VerifyStartOperation()` on every test (it always logs this)
- Call `VerifyFinishOperation(0)` when validation fails (it skips the internal execution)
- Call `VerifyFinishOperation()` even when business logic fails after validation (it still runs)
- Use `SetupQueryable(request.CorrelationId, null, [entities])` — always pass `correlationId` and `null` for newContext

### Never

- Do not use `Times.Once()` — use `Times.Exactly(n)` through the extension methods (`1` or `0`)
- Do not call `_fixture.MockRepository.VerifyQueryable<T>()` unless the use case actually queries
- Do not assert `result.Data` on failure cases
- Do not set up repository mocks when the test expects validation to fail

### Fixture Static vs Instance Methods

- Use `static` factory methods when the request does not require `AutoFixture` (e.g., `SetValidRequest(int id)` with fixed values)
- Use instance methods when `AutoFixture.CreateMany<T>()` is needed (e.g., generating item collections)

---

## File Placement

```
tests/UnitTests/Application/{Domain}/{EntityName}UseCaseTests.cs
```

Example: `tests/UnitTests/Application/Orders/CreateOrderUseCaseTests.cs`

All classes for one use case (validation fixture, validation tests, use case fixture, use case tests) go in **one file**.
