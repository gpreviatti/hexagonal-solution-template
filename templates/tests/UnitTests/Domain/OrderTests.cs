using Domain.Orders;

namespace UnitTests.Domain;

public sealed class OrderTests
{
    [Fact(DisplayName = nameof(Given_A_New_Order_When_Items_Are_Provided_Then_Should_Set_Total_With_Success))]
    public void Given_A_New_Order_When_Items_Are_Provided_Then_Should_Set_Total_With_Success()
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
        Assert.True(result.Success);
        Assert.Empty(result.Message);
        Assert.NotEqual(0, order.Total);
        Assert.Equal(items.Sum(i => i.Value), order.Total);
    }

    [Fact(DisplayName = nameof(Given_A_New_Order_When_Items_Is_Empty_Then_Should_Return_Failure))]
    public void Given_A_New_Order_When_Items_Is_Empty_Then_Should_Return_Failure()
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
