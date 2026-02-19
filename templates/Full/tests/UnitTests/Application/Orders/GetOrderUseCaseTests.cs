using Application.Orders;
using Domain.Orders;
using UnitTests.Application.Common;

namespace UnitTests.Application.Orders;

public sealed class GetOrderUseCaseFixture : BaseApplicationFixture<GetOrderRequest, GetOrderUseCase>
{
    public GetOrderUseCaseFixture() => UseCase = new(MockServiceProvider.Object);
    public GetOrderRequest SetValidRequest() => new(Guid.NewGuid(), AutoFixture.Create<int>());
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
        var request = _fixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        var expectedOrder = _fixture.AutoFixture.Create<OrderDto>();
        _fixture.MockRepository.SetupGetByIdAsNoTrackingAsync<Order, OrderDto>(expectedOrder);

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

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyNotFoundLog(0);
        _fixture.VerifyFinishUseCaseLog();
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWithoutItemsThenPass))]
    public async Task GivenAValidRequestWithoutItemsThenPass()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        OrderDto expectedOrder = new()
        {
            Id = 1,
            Description = "Test Order",
            Total = 1000m,
            Items = []
        };
        _fixture.MockRepository.SetupGetByIdAsNoTrackingAsync<Order, OrderDto>(expectedOrder);

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
        Assert.Equal(0, result.Data.Items?.Count);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyNotFoundLog(0);
        _fixture.VerifyFinishUseCaseLog();
    }

    [Fact(DisplayName = nameof(GivenAInvalidRequestThenFails))]
    public async Task GivenAInvalidRequestThenFails()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetFailedValidator(request);

        // Act
        var result = await _fixture.UseCase.HandleAsync(
            request,
            _fixture.CancellationToken
        );

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
        Assert.NotEmpty(result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyNotFoundLog(0);
        _fixture.VerifyFinishUseCaseLog(0);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenOrderNotFoundThenFails))]
    public async Task GivenAValidRequestWhenOrderNotFoundThenFails()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);

        // Act
        var result = await _fixture.UseCase.HandleAsync(
            request,
            _fixture.CancellationToken
        );

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
        Assert.NotEmpty(result.Message);
        Assert.Equal("Order not found.", result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyNotFoundLog(1);
        _fixture.VerifyFinishUseCaseLog();
    }
}
