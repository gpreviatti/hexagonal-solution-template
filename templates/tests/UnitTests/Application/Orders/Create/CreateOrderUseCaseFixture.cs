using Application.Orders.Create;
using Domain.Common;
using Domain.Orders;
using UnitTests.Application.Common;

namespace UnitTests.Application.Orders.Create;

public class CreateOrderUseCaseFixture : BaseApplicationFixture<Order, CreateOrderRequest>
{
    public CreateOrderUseCase useCase;

    public CreateOrderUseCaseFixture()
    {
        MockServiceProviderServices();
        useCase = new(mockServiceProvider.Object);
    }

    public new void ClearInvocations()
    {
        base.ClearInvocations();
    }

    public CreateOrderRequest SetValidRequest()
    {
        var items = autoFixture
            .CreateMany<CreateOrderItemRequest>(1);

        return new CreateOrderRequest(Guid.NewGuid(), "AwesomeComputer", [.. items]);
    }

    public void SetSuccessfulRepository()
    {
        var order = autoFixture
            .Create<Order>();

        var result = Result.Ok(order);

        MockRepository(result);
    }
}
