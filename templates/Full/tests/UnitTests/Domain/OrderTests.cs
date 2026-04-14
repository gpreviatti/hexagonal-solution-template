using Domain.Common.Exceptions;
using Domain.Orders;

namespace UnitTests.Domain;

public sealed class ItemTests
{
    [Fact(DisplayName = nameof(GivenANewItemWhenValidPropertiesThenShouldCreateWithSuccess))]
    public void GivenANewItemWhenValidPropertiesThenShouldCreateWithSuccess()
    {
        // Arrange, Act
        var item = new Item("Mouse", "Razer", 100m);

        // Assert
        Assert.NotNull(item);
        Assert.Equal("Mouse", item.Name);
        Assert.Equal("Razer", item.Description);
        Assert.Equal(100m, item.Value);
        Assert.Equal("System", item.CreatedBy);
    }

    [Fact(DisplayName = nameof(GivenANewItemWithNullCreatedByThenShouldUseSystemDefault))]
    public void GivenANewItemWithNullCreatedByThenShouldUseSystemDefault()
    {
        // Arrange, Act
        var item = new Item("Mouse", "Razer", 100m, null, null);

        // Assert
        Assert.NotNull(item);
        Assert.Equal("Mouse", item.Name);
        Assert.Equal("Razer", item.Description);
        Assert.Equal(100m, item.Value);
        Assert.Equal("System", item.CreatedBy);
    }

    [Fact(DisplayName = nameof(GivenANewItemWithProvidedCreatedByThenShouldUseProvidedValue))]
    public void GivenANewItemWithProvidedCreatedByThenShouldUseProvidedValue()
    {
        // Arrange, Act
        var item = new Item("Mouse", "Razer", 100m, "John Doe", "America/New_York");

        // Assert
        Assert.NotNull(item);
        Assert.Equal("Mouse", item.Name);
        Assert.Equal("Razer", item.Description);
        Assert.Equal(100m, item.Value);
        Assert.Equal("John Doe", item.CreatedBy);
        Assert.Equal("America/New_York", item.CreatedByTimezoneId);
    }

    [Fact(DisplayName = nameof(GivenANewItemWithEmptyDescriptionThenShouldCreateWithSuccess))]
    public void GivenANewItemWithEmptyDescriptionThenShouldCreateWithSuccess()
    {
        // Arrange, Act
        var item = new Item("Mouse", "", 100m);

        // Assert
        Assert.NotNull(item);
        Assert.Equal("Mouse", item.Name);
        Assert.Equal("", item.Description);
        Assert.Equal(100m, item.Value);
    }

    [Fact(DisplayName = nameof(GivenANewItemWithNegativeValueThenShouldThrowDomainException))]
    public void GivenANewItemWithNegativeValueThenShouldThrowDomainException()
    {
        // Arrange, Act
        var exception = Assert.Throws<DomainException>(() => new Item("Mouse", "Razer", -100m));

        // Assert
        Assert.NotNull(exception);
        Assert.Equal("Item value cannot be zero or negative.", exception.Message);
    }

    [Fact(DisplayName = nameof(GivenANewItemWithVerySmallPositiveValueThenShouldCreateWithSuccess))]
    public void GivenANewItemWithVerySmallPositiveValueThenShouldCreateWithSuccess()
    {
        // Arrange, Act
        var item = new Item("Mouse", "Razer", 0.01m);

        // Assert
        Assert.NotNull(item);
        Assert.Equal(0.01m, item.Value);
    }

    [Fact(DisplayName = nameof(GivenANewItemWithLargeValueThenShouldCreateWithSuccess))]
    public void GivenANewItemWithLargeValueThenShouldCreateWithSuccess()
    {
        // Arrange, Act
        var item = new Item("Computer", "Desktop", 9999.99m);

        // Assert
        Assert.NotNull(item);
        Assert.Equal(9999.99m, item.Value);
    }
}

public sealed class OrderTests
{
    [Fact(DisplayName = nameof(GivenANewOrderWhenItemsAreProvidedThenShouldCreatedWithSuccess))]
    public void GivenANewOrderWhenItemsAreProvidedThenShouldCreatedWithSuccess()
    {
        // Arrange
        var items = new List<Item>()
        {
            new("Computer", "Desktop", 900),
            new("Mouse", "Razer", 100),
            new("Headphone", "Logitech", 100),
        };

        // Act
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
        // Arrange
        var items = new List<Item>()
        {
            new("Computer", "Desktop", 900),
            new("Mouse", "Razer", 100),
            new("Headphone", "Logitech", 100),
        };

        // Act
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
        // Arrange, Act
        var result = Order.Create("Amazing Computer", Array.Empty<Item>());

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsFailure);
        Assert.Equal("Order must have at least one item.", result.Message);
    }

    [Fact(DisplayName = nameof(GivenANewOrderWhenCreatedAtWasNotSetThenShouldReturnFailurePeriod))]
    public void GivenANewOrderWhenCreatedAtWasNotSetThenShouldReturnFailurePeriod()
    {
        // Arrange
        var order = new Order();

        // Act
        var result = order.GetPeriodSinceWasCreated();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("CreatedAt was not set.", result);
    }

    [Theory(DisplayName = nameof(GivenANewOrderWhenRequestingPeriodThenShouldReturnExpectedUnit))]
    [InlineData(30, "seconds ago")]
    [InlineData(5 * 60, "minutes ago")]
    [InlineData(5 * 60 * 60, "hours ago")]
    [InlineData(10 * 24 * 60 * 60, "days ago")]
    [InlineData(120 * 24 * 60 * 60, "months ago")]
    [InlineData(800 * 24 * 60 * 60, "years ago")]
    public void GivenANewOrderWhenRequestingPeriodThenShouldReturnExpectedUnit(int secondsAgo, string expectedUnit)
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddSeconds(-secondsAgo);
        var order = new Order
        {
            CreatedAt = createdAt
        };

        // Act
        var result = order.GetPeriodSinceWasCreated();

        // Assert
        Assert.NotNull(result);
        Assert.EndsWith(expectedUnit, result);
    }

    // Boundary tests for PeriodSinceWasCreated to kill boundary mutation operators

    [Theory(DisplayName = nameof(GivenANewOrderWhenRequestingPeriodBoundaryThenShouldReturnExpectedUnit))]
    [InlineData(-59, "seconds ago")]
    [InlineData(-60, "minutes ago")]
    [InlineData(-61, "minutes ago")]
    [InlineData(-(59 * 60), "minutes ago")]
    [InlineData(-(60 * 60), "hours ago")]
    [InlineData(-(61 * 60), "hours ago")]
    [InlineData(-(23 * 60 * 60), "hours ago")]
    [InlineData(-(24 * 60 * 60), "days ago")]
    [InlineData(-(25 * 60 * 60), "days ago")]
    [InlineData(-29, "days ago")] // 29 days as reference
    [InlineData(-30, "months ago")]
    [InlineData(-31, "months ago")]
    [InlineData(-364, "months ago")]
    [InlineData(-365, "years ago")]
    [InlineData(-366, "years ago")]
    public void GivenANewOrderWhenRequestingPeriodBoundaryThenShouldReturnExpectedUnit(int secondsOrDaysAgo, string expectedUnit)
    {
        // Arrange
        var order = new Order
        {
            CreatedAt = secondsOrDaysAgo < -1000
                ? DateTime.UtcNow.AddDays(secondsOrDaysAgo)
                : DateTime.UtcNow.AddSeconds(secondsOrDaysAgo)
        };

        // Act
        var result = order.GetPeriodSinceWasCreated();

        // Assert
        Assert.NotNull(result);
        Assert.Contains(expectedUnit, result);
    }

    [Fact(DisplayName = nameof(GivenANewOrderWithExact60SecondsBoundaryThenShouldReturnMinutes))]
    public void GivenANewOrderWithExact60SecondsBoundaryThenShouldReturnMinutes()
    {
        for (var attempt = 0; attempt < 250; attempt++)
        {
            var order = new Order { CreatedAt = DateTime.UtcNow.AddSeconds(-60) };

            var result = order.GetPeriodSinceWasCreated();

            Assert.DoesNotContain("seconds ago", result);
            Assert.Contains("minutes ago", result);
        }
    }

    [Fact(DisplayName = nameof(GivenANewOrderWithExact60MinutesBoundaryThenShouldReturnHours))]
    public void GivenANewOrderWithExact60MinutesBoundaryThenShouldReturnHours()
    {
        for (var attempt = 0; attempt < 250; attempt++)
        {
            var order = new Order { CreatedAt = DateTime.UtcNow.AddMinutes(-60) };

            var result = order.GetPeriodSinceWasCreated();

            Assert.DoesNotContain("minutes ago", result);
            Assert.Contains("hours ago", result);
        }
    }

    [Fact(DisplayName = nameof(GivenANewOrderWithExact24HoursBoundaryThenShouldReturnDays))]
    public void GivenANewOrderWithExact24HoursBoundaryThenShouldReturnDays()
    {
        for (var attempt = 0; attempt < 250; attempt++)
        {
            var order = new Order { CreatedAt = DateTime.UtcNow.AddHours(-24) };

            var result = order.GetPeriodSinceWasCreated();

            Assert.DoesNotContain("hours ago", result);
            Assert.Contains("days ago", result);
        }
    }

    [Fact(DisplayName = nameof(GivenANewOrderWithExact30DaysBoundaryThenShouldReturnMonths))]
    public void GivenANewOrderWithExact30DaysBoundaryThenShouldReturnMonths()
    {
        for (var attempt = 0; attempt < 250; attempt++)
        {
            var order = new Order { CreatedAt = DateTime.UtcNow.AddDays(-30) };

            var result = order.GetPeriodSinceWasCreated();

            Assert.DoesNotContain("days ago", result);
            Assert.Contains("months ago", result);
        }
    }

    [Fact(DisplayName = nameof(GivenANewOrderWithExact365DaysBoundaryThenShouldReturnYears))]
    public void GivenANewOrderWithExact365DaysBoundaryThenShouldReturnYears()
    {
        for (var attempt = 0; attempt < 250; attempt++)
        {
            var order = new Order { CreatedAt = DateTime.UtcNow.AddDays(-365) };

            var result = order.GetPeriodSinceWasCreated();

            Assert.DoesNotContain("months ago", result);
            Assert.Contains("years ago", result);
        }
    }

    [Theory(DisplayName = nameof(GivenANewOrderWithMultipleTimeUnitsThenShouldReturnCorrectValue))]
    [InlineData(-60, "2 months ago")]  // 60 days
    [InlineData(-730, "2 years ago")]  // 730 days
    public void GivenANewOrderWithMultipleTimeUnitsThenShouldReturnCorrectValue(int daysAgo, string expectedResult)
    {
        // Arrange
        var order = new Order { CreatedAt = DateTime.UtcNow.AddDays(daysAgo) };

        // Act
        var result = order.GetPeriodSinceWasCreated();

        // Assert
        Assert.Equal(expectedResult, result);
    }
}

public sealed class OrderUpdateTests
{
    private static Order CreateOrder() => Order.Create(
        "Original Order",
        [new("Item 1", "Desc 1", 100m)],
        "System"
    ).Value;

    [Fact(DisplayName = nameof(GivenAnOrderWhenUpdatedWithValidDataThenShouldSucceed))]
    public void GivenAnOrderWhenUpdatedWithValidDataThenShouldSucceed()
    {
        // Arrange
        var order = CreateOrder();
        var newItems = new List<Item> { new("New Item", "New Desc", 250m) };

        // Act
        var result = order.Update("Updated Description", newItems, "Editor", "America/New_York");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Updated Description", order.Description);
        Assert.Equal(250m, order.Total);
        Assert.Equal("Editor", order.UpdatedBy);
        Assert.Equal("America/New_York", order.UpdatedByTimezoneId);
    }

    [Fact(DisplayName = nameof(GivenAnOrderWhenUpdatedThenTotalIsRecalculated))]
    public void GivenAnOrderWhenUpdatedThenTotalIsRecalculated()
    {
        // Arrange
        var order = CreateOrder();
        var newItems = new List<Item>
        {
            new("Item A", "Desc A", 300m),
            new("Item B", "Desc B", 200m)
        };

        // Act
        var result = order.Update("Multi-item Order", newItems);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(500m, order.Total);
        Assert.Equal(2, order.Items.Count);
    }

    [Fact(DisplayName = nameof(GivenAnOrderWhenUpdatedWithEmptyItemsThenShouldFail))]
    public void GivenAnOrderWhenUpdatedWithEmptyItemsThenShouldFail()
    {
        // Arrange
        var order = CreateOrder();

        // Act
        var result = order.Update("Updated", []);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Order must have at least one item.", result.Message);
    }

    [Fact(DisplayName = nameof(GivenADeletedOrderWhenUpdatedThenShouldFail))]
    public void GivenADeletedOrderWhenUpdatedThenShouldFail()
    {
        // Arrange
        var order = CreateOrder();
        order.Delete("System");

        // Act
        var result = order.Update("Updated", [new("Item", "Desc", 50m)]);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Cannot update a deleted order.", result.Message);
    }
}

public sealed class OrderDeleteTests
{
    private static Order CreateOrder() => Order.Create(
        "Order to Delete",
        [new("Item 1", "Desc 1", 100m)],
        "System"
    ).Value;

    [Fact(DisplayName = nameof(GivenAnOrderWhenDeletedThenShouldMarkAsDeleted))]
    public void GivenAnOrderWhenDeletedThenShouldMarkAsDeleted()
    {
        // Arrange
        var order = CreateOrder();
        Assert.False(order.IsDeleted);

        // Act
        var result = order.Delete("Admin", "America/New_York");

        // Assert
        Assert.True(result.Success);
        Assert.True(order.IsDeleted);
        Assert.NotNull(order.DeletedAt);
        Assert.Equal("Admin", order.DeletedBy);
        Assert.Equal("Admin", order.UpdatedBy);
    }

    [Fact(DisplayName = nameof(GivenAnAlreadyDeletedOrderWhenDeletedAgainThenShouldFail))]
    public void GivenAnAlreadyDeletedOrderWhenDeletedAgainThenShouldFail()
    {
        // Arrange
        var order = CreateOrder();
        order.Delete("System");

        // Act
        var result = order.Delete("Admin");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Entity is already deleted.", result.Message);
    }

    [Fact(DisplayName = nameof(GivenAnOrderWhenDeletedWithDefaultUserThenShouldUseSystemDefault))]
    public void GivenAnOrderWhenDeletedWithDefaultUserThenShouldUseSystemDefault()
    {
        // Arrange
        var order = CreateOrder();

        // Act
        var result = order.Delete();

        // Assert
        Assert.True(result.Success);
        Assert.True(order.IsDeleted);
        Assert.Equal("System", order.DeletedBy);
        Assert.Equal("System", order.UpdatedBy);
    }

    [Fact(DisplayName = nameof(GivenAnOrderWhenDeletedThenDeletedAtIsSet))]
    public void GivenAnOrderWhenDeletedThenDeletedAtIsSet()
    {
        // Arrange
        var order = CreateOrder();
        var before = DateTime.UtcNow;

        // Act
        order.Delete("Admin");

        // Assert
        var after = DateTime.UtcNow;
        Assert.NotNull(order.DeletedAt);
        Assert.True(order.DeletedAt >= before);
        Assert.True(order.DeletedAt <= after);
    }
}
