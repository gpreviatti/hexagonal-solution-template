using Domain.Orders;

namespace UnitTests.Domain;

public sealed class OrderTests
{
    [Fact(DisplayName = nameof(GivenANewOrderWhenItemsAreProvidedThenShouldSetTotalWithSuccess))]
    public void GivenANewOrderWhenItemsAreProvidedThenShouldSetTotalWithSuccess()
    {
        /// Arrange
        var items = new List<Item>()
        {
            new("Computer", "Desktop", 900),
            new("Mouse", "Razer", 100),
            new("Headphone", "Logitech", 100),
        };
        Order order = new("Amazing Computer", items);
        var initialUpdatedAt = order.UpdatedAt;

        /// Act
        var result = order.SetTotal();

        // Assert
        Assert.NotNull(order);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Empty(result.Message);
        Assert.NotEqual(0, order.Total);
        Assert.NotEqual(initialUpdatedAt, order.UpdatedAt);
        Assert.Equal(items.Sum(i => i.Value), order.Total);
    }

    [Fact(DisplayName = nameof(GivenANewOrderWhenItemsIsEmptyThenShouldReturnFailure))]
    public void GivenANewOrderWhenItemsIsEmptyThenShouldReturnFailure()
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
