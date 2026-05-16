using Bunit;
using Contracts.Common;
using Contracts.Orders;
using Infrastructure.Common;
using Microsoft.Extensions.DependencyInjection;
using UnitTests.Common;
using WebApp.Components.Pages;

namespace UnitTests.WebApp;

public class OrderComponentTestFixture : BaseComponentFixture
{
    public OrderComponentTestFixture()
    {
        Services.AddKeyedSingleton(ServicesKey.Orders.ToString(), HttpServiceMock.Object);
    }

    public static GetOrderSummaryResponse GetValidOrderSummaryResponse() => new(
        success: true,
        data: new OrderSummaryDto(17, 900.50m, "BRL"),
        message: "Order summary retrieved."
    );

    public static GetOrderSummaryResponse GetInvalidOrderSummaryResponse() => new(
        success: false,
        data: null,
        message: "Failed to retrieve order summary."
    );

    public static BaseResponse<IEnumerable<OrderDto>> GetValidOrdersResponse() => new(
        success: true,
        data:
        [
            new OrderDto
            {
                Id = 1,
                Description = "Order 1",
                Total = 100.00m,
                Items = [new ItemDto { Id = 1, Name = "Item 1", Description = "Product A", Value = 50.00m }]
            },
            new OrderDto
            {
                Id = 2,
                Description = "Order 2",
                Total = 200.00m,
                Items = [new ItemDto { Id = 2, Name = "Item 2", Description = "Product B", Value = 25.00m }]
            }
        ],
        message: "Orders retrieved."
    );

    public static BaseResponse<IEnumerable<OrderDto>> GetInvalidOrdersResponse() => new(
        success: false,
        data: null,
        message: "Failed to retrieve orders."
    );
}

public sealed class OrderComponentTests : IClassFixture<OrderComponentTestFixture>
{
    private readonly OrderComponentTestFixture _fixture;

    public OrderComponentTests(OrderComponentTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearInvocations();
    }

    [Fact(DisplayName = nameof(GivenOrderComponentWhenRenderedWithValidDataThenShouldDisplayOrderSummaryAndTable))]
    public void GivenOrderComponentWhenRenderedWithValidDataThenShouldDisplayOrderSummaryAndTable()
    {
        // Arrange
        _fixture.SetupHttpServiceMock("Orders", HttpMethod.Get, OrderComponentTestFixture.GetValidOrdersResponse());
        _fixture.SetupHttpServiceMock("Orders/summary", HttpMethod.Get, OrderComponentTestFixture.GetValidOrderSummaryResponse());

        // Act
        var component = _fixture.Render<Order>();

        // Assert
        component.Find("[data-testid='total-orders']").TextContent.Contains("17");
        component.Find("[data-testid='total-revenue']").TextContent.Contains("900.50 BRL");

        Assert.Equal(2, component.FindAll("[data-testid='orders-table'] tbody tr").Count);
        _fixture.HttpServiceMock.Verify(x => x.SendAsync<BaseResponse<IEnumerable<OrderDto>>>("Orders", HttpMethod.Get, CancellationToken.None, null), Times.Once);
        _fixture.HttpServiceMock.Verify(x => x.SendAsync<GetOrderSummaryResponse>("Orders/summary", HttpMethod.Get, CancellationToken.None, null), Times.Once);
    }

    [Fact(DisplayName = nameof(GivenOrderComponentWhenLoadedThenShouldDisplayOrderData))]
    public void GivenOrderComponentWhenLoadedThenShouldDisplayOrderData()
    {
        // Arrange
        _fixture.SetupHttpServiceMock("Orders", HttpMethod.Get, OrderComponentTestFixture.GetValidOrdersResponse());
        _fixture.SetupHttpServiceMock("Orders/summary", HttpMethod.Get, OrderComponentTestFixture.GetValidOrderSummaryResponse());

        // Act
        var component = _fixture.Render<Order>();

        // Assert
        Assert.Equal("1", component.Find("[data-testid='order-id-1']").TextContent);
        Assert.Equal("Order 1", component.Find("[data-testid='order-customer-1']").TextContent);
        Assert.Equal("100.00", component.Find("[data-testid='order-amount-1']").TextContent);

        Assert.Equal("2", component.Find("[data-testid='order-id-2']").TextContent);
        Assert.Equal("Order 2", component.Find("[data-testid='order-customer-2']").TextContent);
        Assert.Equal("200.00", component.Find("[data-testid='order-amount-2']").TextContent);
    }

    [Fact(DisplayName = nameof(GivenOrderComponentWhenViewItemsButtonClickedThenShouldDisplayOrderItems))]
    public void GivenOrderComponentWhenViewItemsButtonClickedThenShouldDisplayOrderItems()
    {
        // Arrange
        _fixture.SetupHttpServiceMock("Orders", HttpMethod.Get, OrderComponentTestFixture.GetValidOrdersResponse());
        _fixture.SetupHttpServiceMock("Orders/summary", HttpMethod.Get, OrderComponentTestFixture.GetValidOrderSummaryResponse());

        var component = _fixture.Render<Order>();

        // Act
        var viewItemsButton = component.Find("button[data-testid^='view-items-']");
        viewItemsButton.Click();

        // Assert
        var orderItemsTable = component.Find("[data-testid='order-items-table']");
        Assert.NotNull(orderItemsTable);

        var itemRows = component.FindAll("[data-testid='order-items-table'] tbody tr");
        Assert.NotEmpty(itemRows);
        Assert.Single(itemRows); // First order has 1 item
    }

    [Fact(DisplayName = nameof(GivenOrderComponentWhenViewItemsButtonClickedMultipleTimesThenShouldToggleDisplay))]
    public void GivenOrderComponentWhenViewItemsButtonClickedMultipleTimesThenShouldToggleDisplay()
    {
        // Arrange
        _fixture.SetupHttpServiceMock("Orders", HttpMethod.Get, OrderComponentTestFixture.GetValidOrdersResponse());
        _fixture.SetupHttpServiceMock("Orders/summary", HttpMethod.Get, OrderComponentTestFixture.GetValidOrderSummaryResponse());

        var component = _fixture.Render<Order>();
        var viewItemsButton = component.Find("button[data-testid^='view-items-']");

        // Act - First click to show items
        viewItemsButton.Click();
        var orderItemsSection1 = component.FindAll("[data-testid='order-items-section']");
        Assert.NotEmpty(orderItemsSection1);

        // Act - Second click to hide items
        viewItemsButton.Click();

        // Assert - Section should be removed when hidden
        var orderItemsSection2 = component.FindAll("[data-testid='order-items-section']");
        Assert.Empty(orderItemsSection2);
    }

    [Fact(DisplayName = nameof(GivenOrderComponentWhenHttpServiceFailsThenShouldNotDisplayData))]
    public void GivenOrderComponentWhenHttpServiceFailsThenShouldNotDisplayData()
    {
        // Arrange
        _fixture.SetupHttpServiceMock("Orders", HttpMethod.Get, OrderComponentTestFixture.GetInvalidOrdersResponse());
        _fixture.SetupHttpServiceMock("Orders/summary", HttpMethod.Get, OrderComponentTestFixture.GetInvalidOrderSummaryResponse());

        // Act
        var component = _fixture.Render<Order>();

        // Assert
        var ordersTable = component.FindAll("[data-testid='orders-table']");
        Assert.Empty(ordersTable);

        _fixture.MockLogger.VerifyError("Failed to retrieve orders.", 0);
        _fixture.MockLogger.VerifyError("Failed to retrieve order summary.", 0);
    }

    [Fact(DisplayName = nameof(GivenOrderComponentWhenRenderedThenShouldCallHttpServiceOnce))]
    public void GivenOrderComponentWhenRenderedThenShouldCallHttpServiceOnce()
    {
        // Arrange
        _fixture.SetupHttpServiceMock("Orders", HttpMethod.Get, OrderComponentTestFixture.GetValidOrdersResponse());
        _fixture.SetupHttpServiceMock("Orders/summary", HttpMethod.Get, OrderComponentTestFixture.GetValidOrderSummaryResponse());

        // Act
        _fixture.Render<Order>();

        // Assert
        _fixture.HttpServiceMock.Verify(x => x.SendAsync<BaseResponse<IEnumerable<OrderDto>>>("Orders", HttpMethod.Get, CancellationToken.None, null), Times.Once);
        _fixture.HttpServiceMock.Verify(x => x.SendAsync<GetOrderSummaryResponse>("Orders/summary", HttpMethod.Get, CancellationToken.None, null), Times.Once);
    }
}
