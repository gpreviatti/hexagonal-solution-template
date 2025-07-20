using Application.Common.UseCases;
using Application.Orders;
using Domain.Common;
using Domain.Orders;
using UnitTests.Application.Common;

namespace UnitTests.Application.Orders.Create;

public sealed class CreateOrderUseCaseFixture : BaseApplicationFixture<Order, CreateOrderRequest>
{
    public IBaseInOutUseCase<CreateOrderRequest, OrderDto> useCase;

    public CreateOrderUseCaseFixture()
    {
        MockServiceProviderServices();
        useCase = new CreateOrderUseCase(mockServiceProvider.Object);
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

    public void SetSuccessfulRepository()
    {
        var order = autoFixture
            .Create<Order>();

        var result = Result.Ok(order);

        MockRepository(result);
    }

    public void VerifyCreateOrderLogNoItemsError(Guid correlationId, int times = 1) =>
        mockLogger.Verify(l => l.Warning(
            "[{ClassName}] | [{MethodName}] | [{CorrelationId}] | Order must have at least one item.",
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
        _fixture.SetSuccessfulRepository();

        // Act
        var result = await _fixture.useCase.Handle(request, _fixture.cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Empty(result.Message);

        _fixture.VerifyStartUseCaseLog("BaseInOutUseCase", request.CorrelationId);
        _fixture.VerifyFinishUseCaseLog(nameof(CreateOrderUseCase), request.CorrelationId);
        _fixture.VerifyCreateOrderLogNoItemsError(request.CorrelationId, 0);
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
        _fixture.VerifyRepository(0);
    }

    [Fact]
    public async Task GivenAInvalidRequestThenFailsWhenThereIsNoItems()
    {
        // Arrange
        var request = CreateOrderUseCaseFixture.SetInvalidRequestWithNoItems();
        _fixture.SetFailedValidator(request);

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
    }
}
