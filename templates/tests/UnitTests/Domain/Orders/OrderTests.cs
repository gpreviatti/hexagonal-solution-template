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
        result.Success.Should().BeTrue();
        order.Total.Should().NotBe(0);
        order.UpdatedAt.Should().NotBeNull();
        order.Total.Should().Be(items.Sum(i => i.Value));
    }

    [Fact]
    public void GivenAOrderWithoutItemsThenShouldSetTotalAsZeroWithSuccess()
    {
        /// Arrange
        var order = new Order();

        /// Act
        var result = order.Create("Amazing Computer");

        // Assert
        result.Success.Should().BeTrue();
        order.Total.Should().Be(0);
        order.UpdatedAt.Should().NotBeNull();
    }
}
