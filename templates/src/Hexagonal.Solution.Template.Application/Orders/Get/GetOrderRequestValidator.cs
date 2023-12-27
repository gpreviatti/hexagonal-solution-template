using FluentValidation;

namespace Hexagonal.Solution.Template.Application.Orders.Get;
public class GetOrderRequestValidator : AbstractValidator<GetOrderRequest>
{
    public GetOrderRequestValidator()
    {
        RuleFor(r => r.Id).NotEmpty();
    }
}
