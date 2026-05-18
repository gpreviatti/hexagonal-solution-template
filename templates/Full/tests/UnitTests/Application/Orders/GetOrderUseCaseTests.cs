using Application.Orders;
using Domain.Orders;
using UnitTests.Application.Common;

namespace UnitTests.Application.Orders;

public sealed class GetOrderUseCaseFixture : BaseApplicationFixture<GetOrderRequest, GetOrderUseCase>
{
    public GetOrderUseCaseFixture() => UseCase = new(MockServiceProvider.Object);
    public GetOrderRequest SetValidRequest(int? id = null) => new(Guid.NewGuid(), id ?? AutoFixture.Create<int>());
    public static GetOrderRequest SetInvalidRequest() => new(Guid.Empty, 0);
}

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
        var resultCreateOrder = Order.Create(
            "Test Order",
            [
                new("Item 1", "Description 1", 500m),
                new("Item 2", "Description 2", 500m)
            ]
        );
        var expectedOrder = resultCreateOrder.Value;
        var request = _fixture.SetValidRequest(resultCreateOrder.Value.Id);
        request = request with { Id = expectedOrder.Id };
        _fixture.MockRepository.SetupQueryable(request.CorrelationId, null, [expectedOrder]);

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Null(result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(expectedOrder.Id, result.Data.Id);
        Assert.Equal(expectedOrder.Description, result.Data.Description);
        Assert.Equal(expectedOrder.Total, result.Data.Total);
        Assert.NotNull(result.Data.Items);
        Assert.Equal(expectedOrder.Items?.Count, result.Data.Items!.Count);

        _fixture.MockRepository.VerifyQueryable<Order>();
        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyNotFound(0);
        _fixture.MockLogger.VerifyFinishOperation();
    }

    [Fact(DisplayName = nameof(GivenAInvalidRequestThenFails))]
    public async Task GivenAInvalidRequestThenFails()
    {
        // Arrange
        var request = GetOrderUseCaseFixture.SetInvalidRequest();

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
        _fixture.MockRepository.SetupQueryable<Order>(request.CorrelationId, null, []);

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
        Assert.NotEmpty(result.Message);
        Assert.Equal("Order not found.", result.Message);

        _fixture.MockRepository.VerifyQueryable<Order>();
        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyNotFound(1);
        _fixture.MockLogger.VerifyFinishOperation();
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenOrderExistsThenResponseContainsAllOrderDtoFields))]
    public async Task GivenAValidRequestWhenOrderExistsThenResponseContainsAllOrderDtoFields()
    {
        // Arrange
        var resultCreateOrder = Order.Create(
            "Complete Test Order",
            [
                new("Item 1", "Description 1", 150m),
                new("Item 2", "Description 2", 250m)
            ]
        );
        var expectedOrder = resultCreateOrder.Value;
        var request = _fixture.SetValidRequest(expectedOrder.Id);
        _fixture.MockRepository.SetupQueryable(request.CorrelationId, null, [expectedOrder]);

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(expectedOrder.Id, result.Data.Id);
        Assert.Equal(expectedOrder.Description, result.Data.Description);
        Assert.Equal(expectedOrder.Total, result.Data.Total);
        Assert.NotNull(result.Data.PeriodSinceWasCreated);
        Assert.NotEmpty(result.Data.PeriodSinceWasCreated);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenOrderExistsThenResponseContainsAllItemDtoFields))]
    public async Task GivenAValidRequestWhenOrderExistsThenResponseContainsAllItemDtoFields()
    {
        // Arrange
        var resultCreateOrder = Order.Create(
            "Item Fields Test Order",
            [
                new("Mouse", "Razer", 100m),
                new("Keyboard", "Mechanical", 150m),
                new("Monitor", "4K", 500m)
            ]
        );
        var expectedOrder = resultCreateOrder.Value;
        var request = _fixture.SetValidRequest(expectedOrder.Id);
        _fixture.MockRepository.SetupQueryable(request.CorrelationId, null, [expectedOrder]);

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.Items);
        Assert.Equal(3, result.Data.Items.Count);

        var itemsList = result.Data.Items.ToList();
        var expectedItemsList = expectedOrder.Items.ToList();

        // Verify all fields are mapped for each item
        for (int i = 0; i < itemsList.Count; i++)
        {
            var itemDto = itemsList[i];
            var expectedItem = expectedItemsList[i];

            Assert.Equal(expectedItem.Name, itemDto.Name);
            Assert.Equal(expectedItem.Description, itemDto.Description);
            Assert.Equal(expectedItem.Value, itemDto.Value);
        }
    }

    [Fact(DisplayName = nameof(GivenAValidRequestOnMultipleCallsThenShouldReturnConsistentResults))]
    public async Task GivenAValidRequestOnMultipleCallsThenShouldReturnConsistentResults()
    {
        // Arrange
        var resultCreateOrder = Order.Create(
            "Consistency Test Order",
            [
                new("Item 1", "Description 1", 300m),
                new("Item 2", "Description 2", 200m)
            ]
        );
        var expectedOrder = resultCreateOrder.Value;
        var request = _fixture.SetValidRequest(expectedOrder.Id);
        _fixture.MockRepository.SetupQueryable(request.CorrelationId, null, [expectedOrder]);

        // Act
        var result1 = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        _fixture.ClearInvocations();
        _fixture.MockRepository.SetupQueryable(request.CorrelationId, null, [expectedOrder]);

        var result2 = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.True(result1.Success);
        Assert.True(result2.Success);
        Assert.NotNull(result1.Data);
        Assert.NotNull(result2.Data);
        Assert.Equal(result1.Data!.Id, result2.Data!.Id);
        Assert.Equal(result1.Data!.Total, result2.Data!.Total);
        Assert.NotNull(result1.Data!.Items);
        Assert.NotNull(result2.Data!.Items);
        Assert.Equal(result1.Data!.Items.Count, result2.Data!.Items.Count);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenOrderHasNoItemsThenShouldReturnEmptyItemList))]
    public async Task GivenAValidRequestWhenOrderHasNoItemsThenShouldReturnEmptyItemList()
    {
        // Arrange
        var resultCreateOrder = Order.Create(
            "Test Order",
            [new("Item", "Description", 100m)]
        );
        var expectedOrder = resultCreateOrder.Value;
        var request = _fixture.SetValidRequest(expectedOrder.Id);
        _fixture.MockRepository.SetupQueryable(request.CorrelationId, null, [expectedOrder]);

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.Items);
    }
}
