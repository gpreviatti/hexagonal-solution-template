using Domain.Orders;

namespace UnitTests.Domain.Orders;

public sealed class OrderTests
{
    [Fact]
    public void SetTotalWithItemsThenShouldSetWithSuccess()
    {
        /// Arrange
        var items = new List<Item>()
        {
            new("Computer", "Desktop", 900),
            new("Mouse", "Razer", 100),
            new("Headphone", "Logitech", 100),
        };
        Order order = new("Amazing Computer", items);

        /// Act
        var result = order.SetTotal();

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
    public void SetTotalWithNoItemsThenShouldReturnFailure()
    {
        /// Arrange
        Order order = new("Amazing Computer", Array.Empty<Item>());

        /// Act
        var result = order.SetTotal();

        // Assert
        Assert.NotNull(order);
        Assert.NotNull(result);
        Assert.True(result.IsFailure);
        Assert.Equal("Order must have at least one item.", result.Message);
    }
}
