# Test Pattern — BaseInOutUseCase (Delete / Soft Delete)

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
        var order = DeleteOrderUseCaseFixture.CreateOrder();
        var request = DeleteOrderUseCaseFixture.SetValidRequest(order.Id);
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetupQueryable(request.CorrelationId, null, [order]);
        _fixture.MockRepository.SetSuccessfulUpdate<Order>();

        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

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
        var request = DeleteOrderUseCaseFixture.SetValidRequest();
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
        var request = DeleteOrderUseCaseFixture.SetValidRequest();
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
        _fixture.VerifyProduce<CreateNotificationMessage>();
    }

    // Guard: already deleted — must test the domain state check
    [Fact(DisplayName = nameof(GivenAValidRequestWhenOrderAlreadyDeletedThenFails))]
    public async Task GivenAValidRequestWhenOrderAlreadyDeletedThenFails()
    {
        var order = DeleteOrderUseCaseFixture.CreateOrder();
        order.DeleteOrder("System");  // put entity into deleted state

        var request = DeleteOrderUseCaseFixture.SetValidRequest(order.Id);
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetupQueryable(request.CorrelationId, null, [order]);

        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

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
        var order = DeleteOrderUseCaseFixture.CreateOrder();
        var request = DeleteOrderUseCaseFixture.SetValidRequest(order.Id);
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetupQueryable(request.CorrelationId, null, [order]);
        _fixture.MockRepository.SetFailedUpdate<Order>();

        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

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
