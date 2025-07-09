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
        result.Data.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().BeNullOrEmpty();

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
        result.Data.Should().BeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().NotBeNullOrEmpty();

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
        result.Data.Should().BeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().NotBeNullOrEmpty();

        _fixture.VerifyLoggerInformation<BaseResponse<OrderDto>>(0);
        _fixture.VerifyRepository(0);
        _fixture.VerifyLoggerError<CreateOrderRequest>(1);
    }
}
