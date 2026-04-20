using Domain.Common.Exceptions;
using Domain.Orders;

namespace UnitTests.Domain.Orders;

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
