using FluentValidation;
using Application.Orders.Create;

namespace UnitTests.Application.Orders.Create;
public class CreateOrderRequestValidationFixture
{
    public IValidator<CreateOrderRequest> validator;

    public CreateOrderRequestValidationFixture()
    {
        validator = new CreateOrderRequestValidator();
    }
}
