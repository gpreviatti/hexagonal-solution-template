using Contracts.Orders;

namespace UnitTests.Orders;

public sealed class CreateOrderRequestTests
{
    [Fact(DisplayName = nameof(GivenACreateOrderItemRequestWhenInstantiatedThenShouldAssignProperties))]
    public void GivenACreateOrderItemRequestWhenInstantiatedThenShouldAssignProperties()
    {
        var request = new CreateOrderItemRequest("Item 1", "Description 1", 120.5m);

        Assert.Equal("Item 1", request.Name);
        Assert.Equal("Description 1", request.Description);
        Assert.Equal(120.5m, request.Value);
    }

    [Fact(DisplayName = nameof(GivenACreateOrderRequestWhenInstantiatedThenShouldAssignProperties))]
    public void GivenACreateOrderRequestWhenInstantiatedThenShouldAssignProperties()
    {
        var correlationId = Guid.NewGuid();
        CreateOrderItemRequest[] items =
        [
            new("Item 1", "Description 1", 100m),
            new("Item 2", "Description 2", 200m)
        ];

        var request = new CreateOrderRequest(correlationId, "Order description", items);

        Assert.Equal(correlationId, request.CorrelationId);
        Assert.Equal("Order description", request.Description);
        Assert.Equal(items, request.Items);
    }
}
