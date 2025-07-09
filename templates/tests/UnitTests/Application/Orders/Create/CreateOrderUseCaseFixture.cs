using Application.Orders.Create;
using Domain.Common;
using Domain.Orders;
using UnitTests.Application.Common;

namespace UnitTests.Application.Orders.Create;

public class CreateOrderUseCaseFixture : BaseApplicationFixture<Order, CreateOrderRequest>
{
    public ICreateOrderUseCase useCase;

    public CreateOrderUseCaseFixture()
    {
        MockServiceProviderServices();

        useCase = new CreateOrderUseCase(mockServiceProvider.Object);
    }

    public new void ClearInvocations()
    {
        base.ClearInvocations();
    }

    public void SetSuccessfulRepository()
    {
        var order = autoFixture
            .Create<Order>();

        var result = Result.Ok(order);

        MockRepository(result);
    }
}
