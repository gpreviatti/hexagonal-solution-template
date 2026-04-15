using Application.Common.Messages;
using Application.Orders;
using Domain.Orders;
using FluentValidation;
using FluentValidation.TestHelper;
using UnitTests.Application.Common;

namespace UnitTests.Application.Orders;

public sealed class UpdateOrderRequestValidationFixture
{
    public IValidator<UpdateOrderRequest> Validator { get; } = new UpdateOrderRequestValidator();

    public static UpdateOrderRequest GetValidRequest() => new(Guid.NewGuid(), 1, "updated order", [
        new("item1", "description1", 10.0m),
        new("item2", "description2", 20.0m)
    ]);
}

public sealed class UpdateOrderRequestValidationTests(UpdateOrderRequestValidationFixture fixture) : IClassFixture<UpdateOrderRequestValidationFixture>
{
    private readonly UpdateOrderRequestValidationFixture _fixture = fixture;

    [Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
    public async Task GivenAValidRequestThenPass()
    {
        // Arrange
        var request = UpdateOrderRequestValidationFixture.GetValidRequest();

        // Act
        var result = await _fixture.Validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = nameof(GivenAnInvalidRequestThenFails))]
    public async Task GivenAnInvalidRequestThenFails()
    {
        // Arrange
        var request = UpdateOrderRequestValidationFixture.GetValidRequest() with
        {
            CorrelationId = Guid.Empty,
            OrderId = 0,
            Description = string.Empty,
            Items = []
        };

        // Act
        var result = await _fixture.Validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor("CorrelationId");
        result.ShouldHaveValidationErrorFor("OrderId");
        result.ShouldHaveValidationErrorFor("Description");
        result.ShouldHaveValidationErrorFor("Items");
    }

    [Fact(DisplayName = nameof(GivenAnInvalidItemThenFails))]
    public async Task GivenAnInvalidItemThenFails()
    {
        // Arrange
        var request = UpdateOrderRequestValidationFixture.GetValidRequest() with
        {
            Items =
            [
                new("", "invalid item", 0m)
            ]
        };

        // Act
        var result = await _fixture.Validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor("Items[0].Name");
        result.ShouldHaveValidationErrorFor("Items[0].Value");
    }
}

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
        _fixture.VerifyProduce<CreateNotificationMessage>();
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenOrderIsDeletedThenFails))]
    public async Task GivenAValidRequestWhenOrderIsDeletedThenFails()
    {
        // Arrange
        var order = UpdateOrderUseCaseFixture.CreateOrder();
        order.Delete("System");

        var request = _fixture.SetValidRequest(order.Id);
        _fixture.SetSuccessfulValidator(request);
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
        Assert.Equal("Failed to update order.", result.Message);

        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation();
        _fixture.MockLogger.VerifyOperationFailed(1);
        _fixture.MockRepository.VerifyQueryable<Order>();
        _fixture.MockRepository.VerifyUpdate<Order>(1);
        _fixture.VerifyProduce<CreateNotificationMessage>();
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenSuccessThenResponseContainsUpdatedFields))]
    public async Task GivenAValidRequestWhenSuccessThenResponseContainsUpdatedFields()
    {
        // Arrange
        var order = UpdateOrderUseCaseFixture.CreateOrder();
        var item1 = new UpdateOrderItemRequest("NewItem1", "NewDesc1", 150m);
        var item2 = new UpdateOrderItemRequest("NewItem2", "NewDesc2", 250m);
        var request = new UpdateOrderRequest(Guid.NewGuid(), order.Id, "Updated Description", [item1, item2]);

        _fixture.SetSuccessfulValidator(request);
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

    [Fact(DisplayName = nameof(GivenAValidRequestWhenItemsAreEmptyThenFails))]
    public async Task GivenAValidRequestWhenItemsAreEmptyThenFails()
    {
        // Arrange
        var order = UpdateOrderUseCaseFixture.CreateOrder();
        var request = new UpdateOrderRequest(Guid.NewGuid(), order.Id, "Updated Order", []);

        _fixture.SetSuccessfulValidator(request);
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
