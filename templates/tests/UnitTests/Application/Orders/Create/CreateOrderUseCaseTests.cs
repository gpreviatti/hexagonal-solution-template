using Application.Orders;

namespace UnitTests.Application.Orders.Create;

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

        _fixture.mockLogger.Verify(l => l.Warning(
            "[{ClassName}] | [{MethodName}] | [{CorrelationId}] | Unable to create order",
            nameof(CreateOrderUseCase), "HandleInternalAsync", request.CorrelationId
        ), Times.Never);
        _fixture.VerifyRepository(1);
    }

    [Fact]
    public async Task GivenAInvalidRequestThenFails()
    {
        // Arrange
        var request = _fixture.autoFixture.Create<CreateOrderRequest>();
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
        _fixture.mockLogger.Verify(l => l.Warning(
            "[{ClassName}] | [{MethodName}] | [{CorrelationId}] | Unable to create order",
            nameof(CreateOrderUseCase), "HandleInternalAsync", request.CorrelationId
        ), Times.Never);
        _fixture.VerifyRepository(0);
    }
}
