# Test Pattern — BaseInOutUseCase (Update)

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
        var order = UpdateOrderUseCaseFixture.CreateOrder();
        var request = _fixture.SetValidRequest(order.Id);
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetupQueryable(request.CorrelationId, null, [order]);
        _fixture.MockRepository.SetSuccessfulUpdate<Order>();

        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

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
        var request = _fixture.SetValidRequest();
        _fixture.SetFailedValidator(request);

        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

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
        var request = _fixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetupQueryable<Order>(request.CorrelationId, null, []);

        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

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
        var order = UpdateOrderUseCaseFixture.CreateOrder();
        var request = _fixture.SetValidRequest(order.Id);
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetupQueryable(request.CorrelationId, null, [order]);
        _fixture.MockRepository.SetFailedUpdate<Order>();

        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        Assert.False(result.Success);
        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation();
        _fixture.MockLogger.VerifyOperationFailed(1);
        _fixture.MockRepository.VerifyQueryable<Order>();
        _fixture.MockRepository.VerifyUpdate<Order>(1);
    }
}
```
