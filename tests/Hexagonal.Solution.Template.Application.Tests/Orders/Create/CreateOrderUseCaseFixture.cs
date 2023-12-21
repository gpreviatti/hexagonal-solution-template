using FluentValidation;
using FluentValidation.Results;
using Hexagonal.Solution.Template.Application.Orders.Create;
using Hexagonal.Solution.Template.Domain.Common;
using Hexagonal.Solution.Template.Domain.Orders;
using Hexagonal.Solution.Template.Domain.Orders.Services;

namespace Hexagonal.Solution.Template.Application.Tests.Orders.Create;

public class CreateOrderUseCaseFixture : BaseFixture
{
    public IValidator<CreateOrderRequest> validator = Substitute.For<IValidator<CreateOrderRequest>>();
    public ICreateOrderService domainService = Substitute.For<ICreateOrderService>();

    public ICreateOrderUseCase useCase;

    public CreateOrderUseCaseFixture()
    {
        useCase = new CreateOrderUseCase(
            logger,
            validator,
            domainService
        );
    }

    public void ClearReceivedCalls()
    {
        logger.ClearReceivedCalls();
        domainService.ClearReceivedCalls();
        validator.ClearReceivedCalls();
    }

    public void SetSuccessfulValidator(CreateOrderRequest request)
    {
        var validationResult = new ValidationResult();
        validator
            .ValidateAsync(request)
            .Returns(validationResult);
    }

    public void SetSuccessfulDomainService()
    {
        var order = autoFixture
            .Create<Order>();

        var result = Result.Ok(order);

        domainService
            .Handle(Arg.Any<string>(), Arg.Any<List<Item>>())
            .ReturnsForAnyArgs(result);
    }
}
