using FluentValidation;
using FluentValidation.Results;
using Hexagonal.Solution.Template.Application.Orders.Create;
using Hexagonal.Solution.Template.Domain.Common;
using Hexagonal.Solution.Template.Domain.Orders;
using Hexagonal.Solution.Template.Domain.Orders.Services;

namespace Hexagonal.Solution.Template.Application.Tests.Orders.Create;

public class CreateOrderUseCaseFixture : BaseFixture
{
    public Mock<IValidator<CreateOrderRequest>> mockValidator = new();
    public Mock<ICreateOrderService> mockDomainService = new();

    public ICreateOrderUseCase useCase;

    public CreateOrderUseCaseFixture()
    {
        useCase = new CreateOrderUseCase(
            mockLogger.Object,
            mockValidator.Object,
            mockDomainService.Object
        );
    }

    public void ClearInvocations()
    {
        mockLogger.Invocations.Clear();
        mockDomainService.Invocations.Clear();
        mockValidator.Invocations.Clear();
    }

    public void SetSuccessfulValidator(CreateOrderRequest request)
    {
        var validationResult = new ValidationResult();
        mockValidator
            .Setup(v => v.ValidateAsync(request, cancellationToken))
            .ReturnsAsync(validationResult);
    }

    public void SetFailedValidator(CreateOrderRequest request)
    {
        var validationResult = new ValidationResult()
        {
            Errors = [ 
                new ValidationFailure("Description", "Description is required")
            ]
        };
        mockValidator
            .Setup(v => v.ValidateAsync(request, cancellationToken))
            .ReturnsAsync(validationResult);
    }

    public void SetSuccessfulDomainService()
    {
        var order = autoFixture
            .Create<Order>();

        var result = Result.Ok(order);

        mockDomainService
            .Setup(d => d.Handle(It.IsAny<string>(), It.IsAny<List<Item>>()))
            .Returns(result);
    }

    public void SetFailedDomainService()
    {
        var result = Result.Fail<Order>("Unable to create order");

        mockDomainService
            .Setup(d => d.Handle(It.IsAny<string>(), It.IsAny<List<Item>>()))
            .Returns(result);
    }
}
