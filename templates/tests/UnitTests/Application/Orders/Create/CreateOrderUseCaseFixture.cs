using Application.Common.UseCases;
using Application.Orders;
using Domain.Common;
using Domain.Orders;
using UnitTests.Application.Common;

namespace UnitTests.Application.Orders.Create;

public class CreateOrderUseCaseFixture : BaseApplicationFixture<Order, CreateOrderRequest>
{
    public IBaseInOutUseCase<CreateOrderRequest, OrderDto> useCase;

    public CreateOrderUseCaseFixture()
    {
        MockServiceProviderServices();
        useCase = new CreateOrderUseCase(mockServiceProvider.Object);
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
