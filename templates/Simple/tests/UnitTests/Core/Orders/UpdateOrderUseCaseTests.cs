using Core.Common.Messages;
using Core.Orders;
using UnitTests.Core.Common;

namespace UnitTests.Core.Orders;

public sealed class UpdateOrderUseCaseFixture : BaseCoreFixture<UpdateOrderUseCase>
{
    public UpdateOrderUseCaseFixture() => UseCase = new(MockServiceProvider.Object);

    public UpdateOrderRequest SetValidRequest(int orderId = 1)
    {
        var items = AutoFixture.CreateMany<UpdateOrderItemRequest>(1);
        return new(Guid.NewGuid(), orderId, "Updated Order", [.. items]);
    }

    public static UpdateOrderRequest SetInvalidRequest() =>
        new(Guid.Empty, 0, "Updated Order", [new("Item1", "Desc1", 1m)]);

    public static Order CreateOrder() => Order.Create(
        "Original Order",
        [new("Item 1", "Desc 1", 100m)],
        "System"
    ).Value;
}

public sealed class UpdateOrderUseCaseTest : IClassFixture<UpdateOrderUseCaseFixture>
{
    private readonly UpdateOrderUseCaseFixture _fixture;

    public UpdateOrderUseCaseTest(UpdateOrderUseCaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearInvocations();
    }

    [Fact(DisplayName = nameof(GivenAValidRequestThenPassAsync))]
    public async Task GivenAValidRequestThenPassAsync()
    {
        // Arrange
        var order = UpdateOrderUseCaseFixture.CreateOrder();
        var request = _fixture.SetValidRequest(order.Id);
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

    [Fact(DisplayName = nameof(GivenAnInvalidRequestThenFailsAsync))]
    public async Task GivenAnInvalidRequestThenFailsAsync()
    {
        // Arrange
        var request = UpdateOrderUseCaseFixture.SetInvalidRequest();

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

    [Fact(DisplayName = nameof(GivenAValidRequestWhenOrderNotFoundThenFailsAsync))]
    public async Task GivenAValidRequestWhenOrderNotFoundThenFailsAsync()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
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

    [Fact(DisplayName = nameof(GivenAValidRequestWhenOrderIsDeletedThenFailsAsync))]
    public async Task GivenAValidRequestWhenOrderIsDeletedThenFailsAsync()
    {
        // Arrange
        var order = UpdateOrderUseCaseFixture.CreateOrder();
        order.Delete("System");

        var request = _fixture.SetValidRequest(order.Id);
        _fixture.MockRepository.SetupQueryable(request.CorrelationId, null, [order]);

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Cannot update a deleted order.", result.Message);

        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation();
        _fixture.MockLogger.VerifyOperationFailed(1);
        _fixture.MockRepository.VerifyUpdate<Order>(0);
        _fixture.VerifyProduce<CreateNotificationMessage>();
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenRepositoryReturnsZeroThenFailsAsync))]
    public async Task GivenAValidRequestWhenRepositoryReturnsZeroThenFailsAsync()
    {
        // Arrange
        var order = UpdateOrderUseCaseFixture.CreateOrder();
        var request = _fixture.SetValidRequest(order.Id);
        _fixture.MockRepository.SetupQueryable(request.CorrelationId, null, [order]);
        _fixture.MockRepository.SetFailedUpdate<Order>();

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Failed to update order.", result.Message);

        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation();
        _fixture.MockLogger.VerifyOperationFailed(1);
        _fixture.MockRepository.VerifyQueryable<Order>();
        _fixture.MockRepository.VerifyUpdate<Order>(1);
        _fixture.VerifyProduce<CreateNotificationMessage>();
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenSuccessThenResponseContainsUpdatedFieldsAsync))]
    public async Task GivenAValidRequestWhenSuccessThenResponseContainsUpdatedFieldsAsync()
    {
        // Arrange
        var order = UpdateOrderUseCaseFixture.CreateOrder();
        var item1 = new UpdateOrderItemRequest("NewItem1", "NewDesc1", 150m);
        var item2 = new UpdateOrderItemRequest("NewItem2", "NewDesc2", 250m);
        var request = new UpdateOrderRequest(Guid.NewGuid(), order.Id, "Updated Description", [item1, item2]);

        _fixture.MockRepository.SetupQueryable(request.CorrelationId, null, [order]);
        _fixture.MockRepository.SetSuccessfulUpdate<Order>();

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Updated Description", result.Data.Description);
        Assert.Equal(400m, result.Data.Total);
        Assert.Equal(2, result.Data.Items!.Count);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenItemsAreEmptyThenFailsAsync))]
    public async Task GivenAValidRequestWhenItemsAreEmptyThenFailsAsync()
    {
        // Arrange
        var order = UpdateOrderUseCaseFixture.CreateOrder();
        var request = new UpdateOrderRequest(Guid.NewGuid(), order.Id, "Updated Order", []);

        _fixture.MockRepository.SetupQueryable(request.CorrelationId, null, [order]);

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Order must have at least one item.", result.Message);

        _fixture.MockLogger.VerifyOperationFailed(1);
        _fixture.MockRepository.VerifyUpdate<Order>(0);
        _fixture.VerifyProduce<CreateNotificationMessage>();
    }
}
