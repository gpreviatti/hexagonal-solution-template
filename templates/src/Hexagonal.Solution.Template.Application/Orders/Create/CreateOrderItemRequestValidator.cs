using FluentValidation;

namespace Hexagonal.Solution.Template.Application.Orders.Create;
public class CreateOrderItemRequestValidator : AbstractValidator<CreateOrderItemRequest>
{
    public CreateOrderItemRequestValidator()
    {
        RuleFor(r => r.Name).NotEmpty();
        RuleFor(r => r.Value).NotEmpty();
    }
}
