using Hexagonal.Solution.Template.Application.Orders.Create;

namespace Hexagonal.Solution.Template.Application.Tests.Orders.Create;

public class CreateOrderUseCaseTest : IClassFixture<CreateOrderUseCaseFixture>
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
        var request = _fixture.autoFixture.Create<CreateOrderRequest>();
        _fixture.SetSuccessfulValidator(request);
        _fixture.SetSuccessfulDomainService();

        // Act
        var result = await _fixture.useCase.Handle(
            request,
            _fixture.cancellationToken
        );

        // Assert
        result.Data.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().BeNullOrEmpty();
        _fixture.VerifyDomainService(1);
        _fixture.VerifyLoggerInformation(1);
        _fixture.VerifyLoggerError(0);
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
        _fixture.VerifyDomainService(0);
        _fixture.VerifyLoggerInformation(0);
        _fixture.VerifyLoggerError(1);
    }

    [Fact]
    public async Task GivenAValidRequestWhenCreateOrderFailsThenFails()
    {
        // Arrange
        var request = _fixture.autoFixture.Create<CreateOrderRequest>();
        _fixture.SetSuccessfulValidator(request);
        _fixture.SetFailedDomainService();

        // Act
        var result = await _fixture.useCase.Handle(
            request,
            _fixture.cancellationToken
        );

        // Assert
        result.Data.Should().BeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().NotBeNullOrEmpty();
        _fixture.VerifyDomainService(1);
        _fixture.VerifyLoggerInformation(0);
        _fixture.VerifyLoggerError(1);
    }
}
