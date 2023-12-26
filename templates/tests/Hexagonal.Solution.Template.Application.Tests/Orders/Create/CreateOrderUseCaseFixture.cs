using FluentValidation;
using FluentValidation.Results;
using Hexagonal.Solution.Template.Application.Orders.Create;
using Hexagonal.Solution.Template.Application.Tests.Common;
using Hexagonal.Solution.Template.Domain.Common;
using Hexagonal.Solution.Template.Domain.Orders;
using Hexagonal.Solution.Template.Domain.Orders.Services;

namespace Hexagonal.Solution.Template.Application.Tests.Orders.Create;

public class CreateOrderUseCaseFixture : BaseApplicationFixture
{
    public Mock<IValidator<CreateOrderRequest>> mockValidator = new();
    public Mock<ICreateOrderService> mockDomainService = new();
    public Mock<IOrderRepository> mockRepository = new();

    public ICreateOrderUseCase useCase;

    public CreateOrderUseCaseFixture()
    {
        MockServiceProviderServices();

        useCase = new CreateOrderUseCase(mockServiceProvider.Object);
    }

    public void MockServiceProviderServices()
    {
        mockServiceProvider
            .Setup(r => r.GetService(typeof(IValidator<CreateOrderRequest>)))
            .Returns(mockValidator.Object);

        mockServiceProvider
            .Setup(r => r.GetService(typeof(ICreateOrderService)))
            .Returns(mockDomainService.Object);

        mockServiceProvider
            .Setup(r => r.GetService(typeof(IOrderRepository)))
            .Returns(mockRepository.Object);
    }

    public void ClearInvocations()
    {
        mockLogger.Invocations.Clear();
        mockDomainService.Invocations.Clear();
        mockValidator.Invocations.Clear();
        mockRepository.Invocations.Clear();
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
            .Setup(d => d.Handle(It.IsAny<string>(), It.IsAny<ICollection<Item>>()))
            .Returns(result);
    }

    public void SetSuccessfulRepository()
    {
        var order = autoFixture
            .Create<Order>();

        var result = Result.Ok(order);

        mockRepository
            .Setup(d => d.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()));
    }

    public void SetFailedDomainService()
    {
        var result = Result.Fail<Order>("Unable to create order");

        mockDomainService
            .Setup(d => d.Handle(It.IsAny<string>(), It.IsAny<ICollection<Item>>()))
            .Returns(result);
    }

    public void VerifyDomainService(int times)
    {
        mockDomainService.Verify(
            d => d.Handle(It.IsAny<string>(), It.IsAny<ICollection<Item>>()), 
            Times.Exactly(times)
        );
    }

    public void VerifyRepository(int times)
    {
        mockRepository.Verify(
            d => d.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Exactly(times)
        );
    }
}
