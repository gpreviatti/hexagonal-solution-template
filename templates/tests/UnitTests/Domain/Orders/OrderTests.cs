using Domain.Orders;

namespace UnitTests.Domain.Orders;

public sealed class OrderTests
{
    [Fact]
    public void GivenAOrderWithItemsThenShouldSetTotalWithSuccess()
    {
        /// Arrange
        var items = new List<Item>()
        {
            new(1, "Computer", "Desktop", 900, DateTime.UtcNow),
            new(1, "Mouse", "Razer", 100, DateTime.UtcNow),
            new(1, "Headphone", "Logitech", 100, DateTime.UtcNow),
        };
        var order = new Order();

        /// Act
        var result = order.Create("Amazing Computer", items);

        // Assert
        Assert.NotNull(order);
        Assert.NotNull(result);
        Assert.NotNull(order.UpdatedAt);
        Assert.True(result.Success);
        Assert.Empty(result.Message);
        Assert.NotEqual(0, order.Total);
        Assert.NotNull(order.UpdatedAt);
        Assert.Equal(items.Sum(i => i.Value), order.Total);
    }

    [Fact]
    public void GivenAOrderWithNoItemsThenShouldReturnFailure()
    {
        /// Arrange
        var order = new Order();

        /// Act
        var result = order.Create("Amazing Computer", Array.Empty<Item>());

        // Assert
        Assert.NotNull(order);
        Assert.NotNull(result);
        Assert.True(result.IsFailure);
        Assert.Equal("Order must have at least one item.", result.Message);
    }
}
