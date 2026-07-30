using Contracts.Orders;

namespace UnitTests.Orders;

public sealed class OrderDtoTests
{
    [Fact(DisplayName = nameof(GivenAnOrderDtoWhenPropertiesAreAssignedThenShouldExposeExpectedValues))]
    public void GivenAnOrderDtoWhenPropertiesAreAssignedThenShouldExposeExpectedValues()
    {
        ItemDto[] items =
        [
            new()
            {
                Id = 5,
                Name = "Item A",
                Description = "Item A description",
                Value = 45.7m
            }
        ];

        var orderDto = new OrderDto
        {
            Id = 10,
            Description = "Order A",
            Total = 45.7m,
            Items = items
        };

        Assert.Equal(10, orderDto.Id);
        Assert.Equal("Order A", orderDto.Description);
        Assert.Equal(45.7m, orderDto.Total);
        Assert.Equal(items, orderDto.Items);
    }

    [Fact(DisplayName = nameof(GivenAnOrderDtoWhenItemsIsNullThenShouldExposeNullItems))]
    public void GivenAnOrderDtoWhenItemsIsNullThenShouldExposeNullItems()
    {
        var orderDto = new OrderDto
        {
            Id = 1,
            Description = "Order with no items",
            Total = 0m,
            Items = null
        };

        Assert.Null(orderDto.Items);
    }

    [Fact(DisplayName = nameof(GivenAnOrderDtoWhenMultipleItemsAreAssignedThenShouldExposeAllItems))]
    public void GivenAnOrderDtoWhenMultipleItemsAreAssignedThenShouldExposeAllItems()
    {
        ItemDto[] items =
        [
            new() { Id = 1, Name = "Item A", Description = "Desc A", Value = 10m },
            new() { Id = 2, Name = "Item B", Description = "Desc B", Value = 20m },
            new() { Id = 3, Name = "Item C", Description = "Desc C", Value = 30m }
        ];

        var orderDto = new OrderDto
        {
            Id = 5,
            Description = "Multi-item order",
            Total = 60m,
            Items = items
        };

        Assert.Equal(3, orderDto.Items!.Count);
        Assert.Equal(60m, orderDto.Total);
    }

    [Fact(DisplayName = nameof(GivenAnItemDtoWhenPropertiesAreAssignedThenShouldExposeExpectedValues))]
    public void GivenAnItemDtoWhenPropertiesAreAssignedThenShouldExposeExpectedValues()
    {
        var itemDto = new ItemDto
        {
            Id = 2,
            Name = "Item B",
            Description = "Item B description",
            Value = 99.99m
        };

        Assert.Equal(2, itemDto.Id);
        Assert.Equal("Item B", itemDto.Name);
        Assert.Equal("Item B description", itemDto.Description);
        Assert.Equal(99.99m, itemDto.Value);
    }
}
