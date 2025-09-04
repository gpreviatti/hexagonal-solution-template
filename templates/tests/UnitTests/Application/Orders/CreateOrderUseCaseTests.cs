using Application.Orders;
using Domain.Orders;
using Microsoft.Extensions.Logging;
using UnitTests.Application.Common;

namespace UnitTests.Application.Orders;

public sealed class CreateOrderUseCaseFixture : BaseApplicationFixture<Order, CreateOrderRequest, CreateOrderUseCase>
{
    public CreateOrderUseCaseFixture()
    {
        MockServiceProviderServices();
        useCase = new(mockServiceProvider.Object);
    }

    public new void ClearInvocations()
    {
        base.ClearInvocations();
    }

    public CreateOrderRequest SetValidRequest()
    {
        var items = autoFixture
            .CreateMany<CreateOrderItemRequest>(1);

        return new CreateOrderRequest(Guid.NewGuid(), "AwesomeComputer", [.. items]);
    }

    public static CreateOrderRequest SetInvalidRequestWithNoItems() =>
        new(Guid.NewGuid(), "AwesomeComputer", []);

    public void VerifyCreateOrderLogNoItemsError(int times = 1) =>
        mockLogger.VerifyLog(l => l.LogWarning("*Order must have at least one item.*"), Times.Exactly(times));

    public void VerifyFailedToCreateOrderLog(int times = 1) =>
        mockLogger.VerifyLog(l => l.LogWarning("*Failed to create order.*"), Times.Exactly(times));
}

public sealed class CreateOrderUseCaseTest : IClassFixture<CreateOrderUseCaseFixture>
{
    private readonly CreateOrderUseCaseFixture _fixture;

    public CreateOrderUseCaseTest(CreateOrderUseCaseFixture fixture)
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
        _fixture.SetSuccessfulAddAsync();

        // Act
        var result = await _fixture.useCase.HandleAsync(request, _fixture.cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Empty(result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyFinishUseCaseLog();
        _fixture.VerifyCreateOrderLogNoItemsError(0);
        _fixture.VerifyFailedToCreateOrderLog(0);
        _fixture.VerifyAddAsync(1);
    }

    [Fact]
    public async Task GivenAInvalidRequestThenFails()
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
        _fixture.VerifyFinishUseCaseLog(0);
        _fixture.VerifyCreateOrderLogNoItemsError(0);
        _fixture.VerifyFailedToCreateOrderLog(0);
        _fixture.VerifyAddAsync(0);
    }

    [Fact]
    public async Task GivenAInvalidRequestThenFailsWhenThereIsNoItems()
    {
        // Arrange
        var request = CreateOrderUseCaseFixture.SetInvalidRequestWithNoItems();
        _fixture.SetSuccessfulValidator(request);

        // Act
        var result = await _fixture.useCase.HandleAsync(
            request,
            _fixture.cancellationToken
        );

        // Assert
        Assert.False(result.Success);
        Assert.NotEmpty(result.Message);
        Assert.Equal("Order must have at least one item.", result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyFinishUseCaseLog(0);
        _fixture.VerifyCreateOrderLogNoItemsError(1);
        _fixture.VerifyFailedToCreateOrderLog(0);
        _fixture.VerifyAddAsync(0);
    }

    [Fact]
    public async Task GivenAValidRequestThenFailsWhenRepositoryReturnsZero()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        _fixture.SetFailedAddAsync();

        // Act
        var result = await _fixture.useCase.HandleAsync(
            request,
            _fixture.cancellationToken
        );

        // Assert
        Assert.False(result.Success);
        Assert.NotEmpty(result.Message);
        Assert.Equal("Failed to create order.", result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyFinishUseCaseLog(0);
        _fixture.VerifyAddAsync(1);
        _fixture.VerifyCreateOrderLogNoItemsError(0);
        _fixture.VerifyFailedToCreateOrderLog(1);
    }
}
