using Hexagonal.Solution.Template.Application.Orders.Create;

namespace Hexagonal.Solution.Template.Application.Tests.Orders.Create;

public class CreateOrderUseCaseTest : IClassFixture<CreateOrderUseCaseFixture>
{
    private readonly CreateOrderUseCaseFixture _fixture;

    public CreateOrderUseCaseTest(CreateOrderUseCaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearReceivedCalls();
    }

    [Fact]
    public async Task HandleInternalAsyncStateUnderTestExpectedBehavior()
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

        _fixture.domainService.Received(1);
        _fixture.logger.Received(1);
    }
}
