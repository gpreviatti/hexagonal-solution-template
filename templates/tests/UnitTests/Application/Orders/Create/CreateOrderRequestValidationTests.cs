using Application.Orders;
using FluentValidation;
using FluentValidation.TestHelper;
using GPreviatti.Util.JsonResourceAttribute;

namespace UnitTests.Application.Orders.Create;

public sealed class CreateOrderRequestValidationFixture
{
    public IValidator<CreateOrderRequest> validator = new CreateOrderRequestValidator();
}


public sealed class CreateOrderRequestValidationTests(CreateOrderRequestValidationFixture fixture) : IClassFixture<CreateOrderRequestValidationFixture>
{
    private readonly CreateOrderRequestValidationFixture _fixture = fixture;

    [Theory(DisplayName = nameof(Given_A_Valid_Request_Then_Pass))]
    [JsonResourceData("create-order-request-tests.json", "valid")]
    public async Task Given_A_Valid_Request_Then_Pass(CreateOrderRequest request)
    {
        // Arrange, Act
        var result = await _fixture.validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory(DisplayName = nameof(Given_A_Invalid_Request_Then_Fails))]
    [JsonResourceData("create-order-request-tests.json", "invalid")]
    public async Task Given_A_Invalid_Request_Then_Fails(CreateOrderRequest request, string propertyName)
    {
        // Arrange, Act
        var result = await _fixture.validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(propertyName);
    }
}
