using Application.Common.Messages;
using Application.Orders;
using Application.Orders.Create;

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
        var result = await _fixture.useCase.Handle(
            request,
            _fixture.cancellationToken
        );

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Null(result.Message);

        _fixture.VerifyLoggerInformation<BaseResponse<OrderDto>>(1);
        _fixture.VerifyRepository(1);
        _fixture.VerifyLoggerError<CreateOrderRequest>(0);
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
        Assert.NotNull(result.Data);
        Assert.False(result.Success);
        Assert.NotEmpty(result.Message);

        _fixture.VerifyLoggerInformation<BaseResponse<OrderDto>>(0);
        _fixture.VerifyRepository(0);
        _fixture.VerifyLoggerError<CreateOrderRequest>(0);
    }

    [Fact]
    public async Task GivenAValidRequestWhenCreateOrderFailsThenFails()
    {
        // Arrange
        var request = _fixture.autoFixture.Create<CreateOrderRequest>();
        _fixture.SetSuccessfulValidator(request);

        // Act
        var result = await _fixture.useCase.Handle(
            request,
            _fixture.cancellationToken
        );

        // Assert
        Assert.NotNull(result.Data);
        Assert.False(result.Success);
        Assert.NotEmpty(result.Message);

        _fixture.VerifyLoggerInformation<BaseResponse<OrderDto>>(0);
        _fixture.VerifyRepository(0);
        _fixture.VerifyLoggerError<CreateOrderRequest>(1);
    }
}
