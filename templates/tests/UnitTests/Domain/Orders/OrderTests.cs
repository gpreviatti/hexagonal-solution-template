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
        Assert.NotEqual(0, order.Total);
        Assert.NotNull(order.UpdatedAt);
        Assert.Equal(items.Sum(i => i.Value), order.Total);
    }

    [Fact]
    public void GivenAOrderWithoutItemsThenShouldSetTotalAsZeroWithSuccess()
    {
        /// Arrange
        var order = new Order();

        /// Act
        var result = order.Create("Amazing Computer");

        // Assert
        Assert.NotNull(order);
        Assert.NotNull(result);
        Assert.NotNull(order.UpdatedAt);
        Assert.Equal(0, order.Total);
        Assert.True(result.Success);
        Assert.Equal(0, order.Total);
        Assert.NotNull(order.UpdatedAt);
    }
}
