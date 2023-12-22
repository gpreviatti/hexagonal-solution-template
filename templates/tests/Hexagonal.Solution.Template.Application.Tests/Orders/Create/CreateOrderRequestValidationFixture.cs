using FluentValidation;
using FluentValidation.TestHelper;
using Hexagonal.Solution.Template.Application.Orders.Create;
using Hexagonal.Solution.Template.Tests.Common.Attributes;

namespace Hexagonal.Solution.Template.Application.Tests.Orders.Create;
public class CreateOrderRequestValidationFixture
{
    public IValidator<CreateOrderRequest> validator;

    public CreateOrderRequestValidationFixture()
    {
        validator = new CreateOrderRequestValidator();
    }
}
