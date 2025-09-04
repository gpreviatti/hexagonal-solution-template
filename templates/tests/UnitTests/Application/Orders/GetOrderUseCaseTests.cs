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

    [Fact]
    public async Task GivenAValidRequestThenPass()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        var expectedOrder = _fixture.autoFixture.Create<Order>();

        _fixture.SetValidGetOrCreateAsync(expectedOrder);

        // Act
        var result = await _fixture.useCase.Handle(request, _fixture.cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Empty(result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(expectedOrder.Id, result.Data.Id);
        Assert.Equal(expectedOrder.Description, result.Data.Description);
        Assert.Equal(expectedOrder.Total, result.Data.Total);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyFinishUseCaseLog();
        _fixture.VerifyOrderNotFoundLog(0);
        _fixture.VerifyCache<Order>(1);
    }

    [Fact]
    public async Task GivenAInvalidRequestThenFails()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetFailedValidator(request);

        // Act
        var result = await _fixture.useCase.Handle(
            request,
            _fixture.cancellationToken
        );

        // Assert
        Assert.False(result.Success);
        Assert.NotEmpty(result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyFinishUseCaseLog(0);
        _fixture.VerifyOrderNotFoundLog(0);
        _fixture.VerifyCache<Order>(0);
    }

    [Fact]
    public async Task GivenAValidRequestWhenOrderNotFoundThenFails()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);

        _fixture.SetInvalidGetOrCreateAsync<Order>();

        // Act
        var result = await _fixture.useCase.Handle(
            request,
            _fixture.cancellationToken
        );

        // Assert
        Assert.False(result.Success);
        Assert.NotEmpty(result.Message);
        Assert.Equal("Order not found.", result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyFinishUseCaseLog(0);
        _fixture.VerifyOrderNotFoundLog(1);
        _fixture.VerifyCache<Order>(1);
    }
}
