using FluentValidation;
using Hexagonal.Solution.Template.Application.Orders.Create;

namespace Hexagonal.Solution.Template.Application.Tests.Orders.Create;
public class CreateOrderRequestValidationFixture
{
    public IValidator<CreateOrderRequest> validator;

    public CreateOrderRequestValidationFixture()
    {
        validator = new CreateOrderRequestValidator();
    }
}
