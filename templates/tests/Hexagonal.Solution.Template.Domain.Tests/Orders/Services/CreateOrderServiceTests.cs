using Hexagonal.Solution.Template.Domain.Orders;
using Hexagonal.Solution.Template.Domain.Orders.Services;

namespace Hexagonal.Solution.Template.Domain.Tests.Orders.Services;

public sealed class CreateOrderServiceTest
{
    private readonly ICreateOrderService _createOrderService;

    public CreateOrderServiceTest()
    {
        _createOrderService = new CreateOrderService();
    }

    [Fact]
    public void HandleStateUnderTestExpectedBehavior()
    {
        // Arrange
        var description = "new order XPTO";
        var items = new List<Item>
        {
            new(1, "Computer", "Desktop", 900),
            new(1, "Mouse", "Razer", 100),
            new(1, "Headphone", "Logitech", 100),
        };

        // Act
        var result = _createOrderService.Handle(
            description,
            items
        );

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Count().Should().Be(items.Count);
        result.Value.Total.Should().NotBe(0);
        result.Message.Should().BeNullOrWhiteSpace();
    }
}
