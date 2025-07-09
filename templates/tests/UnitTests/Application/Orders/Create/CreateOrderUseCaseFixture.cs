using Application.Orders.Create;
using Domain.Common;
using Domain.Orders;
using Domain.Orders.Services;
using UnitTests.Application.Common;

namespace UnitTests.Application.Orders.Create;

public class CreateOrderUseCaseFixture : BaseApplicationFixture<Order, CreateOrderRequest>
{

    public Mock<ICreateOrderService> mockDomainService = new();

    public ICreateOrderUseCase useCase;

    public CreateOrderUseCaseFixture()
    {
        MockServiceProviderServices();

        useCase = new CreateOrderUseCase(mockServiceProvider.Object);
    }

    public new void MockServiceProviderServices()
    {
        base.MockServiceProviderServices();
        mockServiceProvider
            .Setup(r => r.GetService(typeof(ICreateOrderService)))
            .Returns(mockDomainService.Object);
    }

    public new void ClearInvocations()
    {
        base.ClearInvocations();
        mockDomainService.Invocations.Clear();
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

        MockRepository(result);
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
}
