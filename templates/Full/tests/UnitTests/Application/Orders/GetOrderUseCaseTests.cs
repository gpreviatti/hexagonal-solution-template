using Application.Orders;
using Domain.Orders;
using Microsoft.Extensions.Logging;
using UnitTests.Application.Common;

namespace UnitTests.Application.Orders;

public sealed class GetOrderUseCaseFixture : BaseApplicationFixture<GetOrderRequest, GetOrderUseCase>
{
    public GetOrderUseCaseFixture()
    {
        useCase = new(mockServiceProvider.Object);
    }
    public GetOrderRequest SetValidRequest() => new(Guid.NewGuid(), autoFixture.Create<int>());

    public void VerifyOrderNotFoundLog(int times = 1) =>
        mockLogger.VerifyLog(l => l.LogWarning("*Order not found.*"), Times.Exactly(times));
}

public sealed class GetOrderUseCaseTest : IClassFixture<GetOrderUseCaseFixture>
{
    private readonly GetOrderUseCaseFixture _fixture;

    public GetOrderUseCaseTest(GetOrderUseCaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearInvocations();
    }

    [Fact(DisplayName = nameof(Given_A_Valid_Request_Then_Pass))]
    public async Task Given_A_Valid_Request_Then_Pass()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        var expectedOrder = _fixture.autoFixture.Create<OrderDto>();
        _fixture.mockRepository.SetupGetByIdAsNoTrackingAsync<Order, OrderDto>(expectedOrder);

        // Act
        var result = await _fixture.useCase.HandleAsync(request, _fixture.cancellationToken);

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
        _fixture.VerifyOrderNotFoundLog(0);
        _fixture.VerifyFinishUseCaseLog();
    }

    [Fact(DisplayName = nameof(Given_A_Valid_Request_Without_Items_Then_Pass))]
    public async Task Given_A_Valid_Request_Without_Items_Then_Pass()
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
        _fixture.mockRepository.SetupGetByIdAsNoTrackingAsync<Order, OrderDto>(expectedOrder);

        // Act
        var result = await _fixture.useCase.HandleAsync(request, _fixture.cancellationToken);

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
        _fixture.VerifyOrderNotFoundLog(0);
        _fixture.VerifyFinishUseCaseLog();
    }

    [Fact(DisplayName = nameof(Given_A_Invalid_Request_Then_Fails))]
    public async Task Given_A_Invalid_Request_Then_Fails()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetFailedValidator(request);

        // Act
        var result = await _fixture.useCase.HandleAsync(
            request,
            _fixture.cancellationToken
        );

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
        Assert.NotEmpty(result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyOrderNotFoundLog(0);
        _fixture.VerifyFinishUseCaseLog(0);
    }

    [Fact(DisplayName = nameof(Given_A_Valid_Request_When_Order_Not_Found_Then_Fails))]
    public async Task Given_A_Valid_Request_When_Order_Not_Found_Then_Fails()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);

        // Act
        var result = await _fixture.useCase.HandleAsync(
            request,
            _fixture.cancellationToken
        );

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
        Assert.NotEmpty(result.Message);
        Assert.Equal("Order not found.", result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyOrderNotFoundLog(1);
        _fixture.VerifyFinishUseCaseLog();
    }
}
