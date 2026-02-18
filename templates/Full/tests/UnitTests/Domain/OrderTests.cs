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
        Order order = new("Amazing Computer", items, "John Doe", "America/New_York");
        var initialUpdatedAt = order.UpdatedAt;

        /// Act
        var result = order.SetTotal();

        // Assert
        Assert.NotNull(order);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Empty(result.Message);
        Assert.NotEqual(0, order.Total);
        Assert.Equal("John Doe", order.CreatedBy);
        Assert.Equal("America/New_York", order.CreatedByTimezoneId);
        Assert.NotEqual(initialUpdatedAt, order.UpdatedAt);
        Assert.Equal(items.Sum(i => i.Value), order.Total);
    }

    [Fact(DisplayName = nameof(GivenANewOrderWithoutUserAndTimezoneWhenItemsAreProvidedThenShouldSetTotalWithSuccess))]
    public void GivenANewOrderWithoutUserAndTimezoneWhenItemsAreProvidedThenShouldSetTotalWithSuccess()
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
        Assert.Equal("System", order.CreatedBy);
        Assert.Equal("UTC", order.CreatedByTimezoneId);
        Assert.Equal("System", order.UpdatedBy);
        Assert.Equal("UTC", order.UpdatedByTimezoneId);
    }

    [Fact(DisplayName = nameof(GivenANewOrderWhenItemsIsEmptyThenShouldReturnFailure))]
    public void GivenANewOrderWhenItemsIsEmptyThenShouldReturnFailure()
    {
        /// Arrange
        Order order = new("Amazing Computer", Array.Empty<Item>());

        /// Act
        var result = order.SetTotal("John Doe", "America/New_York");

        // Assert
        Assert.NotNull(order);
        Assert.NotNull(result);
        Assert.True(result.IsFailure);
        Assert.Equal("Order must have at least one item.", result.Message);
    }
}
