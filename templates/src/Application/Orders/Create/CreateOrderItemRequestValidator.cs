using FluentValidation;

namespace Application.Orders.Create;
public sealed class CreateOrderItemRequestValidator : AbstractValidator<CreateOrderItemRequest>
{
    public CreateOrderItemRequestValidator()
    {
        RuleFor(r => r.Name).NotEmpty();
        RuleFor(r => r.Value).NotEmpty();
    }
}
