using Application.Orders;
using FluentValidation;
using FluentValidation.TestHelper;

namespace UnitTests.Application.Orders;

public sealed class CreateOrderRequestValidationFixture
{
    public IValidator<CreateOrderRequest> validator = new CreateOrderRequestValidator();

    public CreateOrderRequest GetValidRequest() => new(Guid.NewGuid(), "new order", [
        new("item1", "description1", 10.0m),
        new("item2", "description2", 20.0m)
    ]);
}

public sealed class CreateOrderRequestValidationTests(CreateOrderRequestValidationFixture fixture) : IClassFixture<CreateOrderRequestValidationFixture>
{
    private readonly CreateOrderRequestValidationFixture _fixture = fixture;

    [Fact(DisplayName = nameof(Given_A_Valid_Request_Then_Pass))]
    public async Task Given_A_Valid_Request_Then_Pass()
    {
        // Arrange
        var request = _fixture.GetValidRequest();

        // Act
        var result = await _fixture.validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = nameof(Given_A_Invalid_Request_Then_Fails))]
    public async Task Given_A_Invalid_Request_Then_Fails()
    {
        // Arrange
        var request = _fixture.GetValidRequest() with
        {
            CorrelationId = Guid.Empty,
            Description = string.Empty,
            Items = []
        };
        // Act
        var result = await _fixture.validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor("CorrelationId");
        result.ShouldHaveValidationErrorFor("Description");
        result.ShouldHaveValidationErrorFor("Items");
    }
}
