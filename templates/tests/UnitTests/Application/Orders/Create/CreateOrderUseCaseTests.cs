using Application.Common.Messages;
using Application.Orders;
using Domain.Orders;
using Microsoft.Extensions.Logging;
using UnitTests.Application.Common;

namespace UnitTests.Application.Orders.Create;

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

    public void VerifyCreateOrderLogNoItemsError(Guid correlationId, int times = 1) =>
        mockLogger.Verify(l => l.LogWarning(
            DefaultApplicationMessages.DefaultApplicationMessage + "Order must have at least one item.",
            nameof(CreateOrderUseCase), "HandleInternalAsync", correlationId
        ), Times.Exactly(times));

    public void VerifyFailedToCreateOrderLog(Guid correlationId, int times = 1) =>
        mockLogger.Verify(l => l.LogWarning(
            DefaultApplicationMessages.DefaultApplicationMessage + "Failed to create order.",
            nameof(CreateOrderUseCase), "HandleInternalAsync", correlationId
        ), Times.Exactly(times));
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
        var result = await _fixture.useCase.Handle(request, _fixture.cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Empty(result.Message);

        _fixture.VerifyStartUseCaseLog("BaseInOutUseCase", request.CorrelationId);
        _fixture.VerifyFinishUseCaseLog(nameof(CreateOrderUseCase), request.CorrelationId);
        _fixture.VerifyCreateOrderLogNoItemsError(request.CorrelationId, 0);
        _fixture.VerifyFailedToCreateOrderLog(request.CorrelationId, 0);
        _fixture.VerifyRepository(1);
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

        _fixture.VerifyStartUseCaseLog("BaseInOutUseCase", request.CorrelationId);
        _fixture.VerifyFinishUseCaseLog(nameof(CreateOrderUseCase), request.CorrelationId, 0);
        _fixture.VerifyCreateOrderLogNoItemsError(request.CorrelationId, 0);
        _fixture.VerifyFailedToCreateOrderLog(request.CorrelationId, 0);
        _fixture.VerifyRepository(0);
    }

    [Fact]
    public async Task GivenAInvalidRequestThenFailsWhenThereIsNoItems()
    {
        // Arrange
        var request = CreateOrderUseCaseFixture.SetInvalidRequestWithNoItems();
        _fixture.SetSuccessfulValidator(request);

        // Act
        var result = await _fixture.useCase.Handle(
            request,
            _fixture.cancellationToken
        );

        // Assert
        Assert.False(result.Success);
        Assert.NotEmpty(result.Message);
        Assert.Equal("Order must have at least one item.", result.Message);

        _fixture.VerifyStartUseCaseLog("BaseInOutUseCase", request.CorrelationId);
        _fixture.VerifyFinishUseCaseLog(nameof(CreateOrderUseCase), request.CorrelationId, 0);
        _fixture.VerifyCreateOrderLogNoItemsError(request.CorrelationId, 1);
        _fixture.VerifyFailedToCreateOrderLog(request.CorrelationId, 0);
        _fixture.VerifyRepository(0);
    }

    [Fact]
    public async Task GivenAValidRequestThenFailsWhenRepositoryReturnsZero()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        _fixture.SetFailedAddAsync();

        // Act
        var result = await _fixture.useCase.Handle(
            request,
            _fixture.cancellationToken
        );

        // Assert
        Assert.False(result.Success);
        Assert.NotEmpty(result.Message);
        Assert.Equal("Failed to create order.", result.Message);

        _fixture.VerifyStartUseCaseLog("BaseInOutUseCase", request.CorrelationId);
        _fixture.VerifyFinishUseCaseLog(nameof(CreateOrderUseCase), request.CorrelationId, 0);
        _fixture.VerifyRepository(1);
        _fixture.VerifyCreateOrderLogNoItemsError(request.CorrelationId, 0);
        _fixture.VerifyFailedToCreateOrderLog(request.CorrelationId, 1);
    }
}
