using Domain.Common;
using Domain.Orders;

namespace UnitTests.Domain;

public sealed class OrderTests
{
    [Fact(DisplayName = nameof(GivenANewOrderWhenItemsAreProvidedThenShouldCreatedWithSuccess))]
    public void GivenANewOrderWhenItemsAreProvidedThenShouldCreatedWithSuccess()
    {
        /// Arrange
        var items = new List<Item>()
        {
            new("Computer", "Desktop", 900),
            new("Mouse", "Razer", 100),
            new("Headphone", "Logitech", 100),
        };

        /// Act
        var result = Order.Create("Amazing Computer", items, "John Doe", "America/New_York");
        var order = result.Value;
        var initialUpdatedAt = order.UpdatedAt;

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Empty(result.Message);
        Assert.IsType<Order>(result.Value);

        Assert.NotNull(order);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Empty(result.Message);
        Assert.NotEqual(0, order.Total);
        Assert.Equal("John Doe", order.CreatedBy);
        Assert.Equal("America/New_York", order.CreatedByTimezoneId);
        Assert.Equal(items.Sum(i => i.Value), order.Total);
    }

    [Fact(DisplayName = nameof(GivenANewOrderWithoutUserAndTimezoneWhenItemsAreProvidedThenShouldCreateWithSuccess))]
    public void GivenANewOrderWithoutUserAndTimezoneWhenItemsAreProvidedThenShouldCreateWithSuccess()
    {
        /// Arrange
        var items = new List<Item>()
        {
            new("Computer", "Desktop", 900),
            new("Mouse", "Razer", 100),
            new("Headphone", "Logitech", 100),
        };

        /// Act
        var result = Order.Create("Amazing Computer", items);
        var order = result.Value;
        var initialUpdatedAt = order.UpdatedAt;

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Empty(result.Message);
        Assert.IsType<Order>(result.Value);

        Assert.NotNull(order);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Empty(result.Message);
        Assert.NotEqual(0, order.Total);
        Assert.Equal(items.Sum(i => i.Value), order.Total);
        Assert.Equal("System", order.CreatedBy);
        Assert.Equal("UTC", order.CreatedByTimezoneId);
        Assert.Equal("System", order.UpdatedBy);
        Assert.Equal("UTC", order.UpdatedByTimezoneId);
    }

    [Fact(DisplayName = nameof(GivenANewItemWithValueZeroThenShouldBeFailure))]
    public void GivenANewItemWithValueZeroThenShouldBeFailure()
    {
        // Arrange, Act
        var exception = Assert.Throws<DomainException>(() => new Item("Mouse", "Razer", 0));

        // Assert
        Assert.NotNull(exception);
        Assert.Equal("Item value cannot be zero or negative.", exception.Message);
    }



    [Fact(DisplayName = nameof(GivenANewOrderWhenItemsIsEmptyThenShouldReturnFailure))]
    public void GivenANewOrderWhenItemsIsEmptyThenShouldReturnFailure()
    {
        /// Arrange, Act
        var result = Order.Create("Amazing Computer", Array.Empty<Item>());

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsFailure);
        Assert.Equal("Order must have at least one item.", result.Message);
    }
}
