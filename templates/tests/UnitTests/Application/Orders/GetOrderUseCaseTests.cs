using Application.Common.Requests;
using Application.Orders;
using Domain.Orders;
using Microsoft.Extensions.Logging;
using UnitTests.Application.Common;

namespace UnitTests.Application.Orders;

public sealed class GetOrderUseCaseFixture : BaseApplicationFixture<Order, GetOrderRequest, GetOrderUseCase>
{
    public GetOrderUseCaseFixture()
    {
        MockServiceProviderServices();
        useCase = new(mockServiceProvider.Object);
    }

    public new void ClearInvocations()
    {
        base.ClearInvocations();
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

    [Fact(DisplayName = nameof(Given_A_Valid_Request_Without_Cache_Then_Pass))]
    public async Task Given_A_Valid_Request_Without_Cache_Then_Pass()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        var expectedOrder = _fixture.autoFixture.Create<Order>();
        _fixture.SetupGetByIdAsNoTrackingAsync(expectedOrder);

        // Act
        var result = await _fixture.useCase.HandleAsync(request, _fixture.cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Empty(result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(expectedOrder.Id, result.Data.Id);
        Assert.Equal(expectedOrder.Description, result.Data.Description);
        Assert.Equal(expectedOrder.Total, result.Data.Total);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyOrderNotFoundLog(0);
        _fixture.VerifyFinishUseCaseLog();
        _fixture.VerifyFinishUseCaseWithCacheLog(0);
    }

    [Fact(DisplayName = nameof(Given_A_Valid_Request_With_Cache_Then_Pass))]
    public async Task Given_A_Valid_Request_With_Cache_Then_Pass()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        var expectedResult = _fixture.autoFixture
            .Create<BaseResponse<OrderDto>>()
            with { Success = true, Message = string.Empty };
        _fixture.SetValidGetOrCreateAsync(expectedResult);
        var cacheKey = $"order-{request.Id}";

        // Act
        var result = await _fixture.useCase.HandleAsync(request, _fixture.cancellationToken, cacheKey);
        var data = result.Data;

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Empty(result.Message);
        Assert.NotNull(data);
        Assert.Equal(expectedResult.Data.Id, data.Id);
        Assert.Equal(expectedResult.Data.Description, data.Description);
        Assert.Equal(expectedResult.Data.Total, data.Total);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyCache<BaseResponse<OrderDto>>(1);
        _fixture.VerifyOrderNotFoundLog(0);
        _fixture.VerifyFinishUseCaseWithCacheLog(1);
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
        Assert.NotEmpty(result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyOrderNotFoundLog(0);
        _fixture.VerifyCache<Order>(0);
        _fixture.VerifyFinishUseCaseLog(0);
        _fixture.VerifyFinishUseCaseWithCacheLog(0);
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
        Assert.NotEmpty(result.Message);
        Assert.Equal("Order not found.", result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyOrderNotFoundLog(1);
        _fixture.VerifyFinishUseCaseLog();
        _fixture.VerifyFinishUseCaseWithCacheLog(0);
    }
}
