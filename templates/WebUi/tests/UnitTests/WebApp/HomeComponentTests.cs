using Bunit;
using Contracts.Common;
using Contracts.Orders;
using Infrastructure.Common;
using Microsoft.Extensions.DependencyInjection;
using UnitTests.Common;
using WebApp.Components.Pages;

namespace UnitTests.WebApp;

public class HomeComponentTestFixture : BaseComponentFixture
{
    public HomeComponentTestFixture()
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
        data: [
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

public sealed class HomeComponentTests : IClassFixture<HomeComponentTestFixture>
{
    private readonly HomeComponentTestFixture _fixture;

    public HomeComponentTests(HomeComponentTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearInvocations();
    }

    [Fact]
    public void GivenHomeComponentWhenRenderedThenShouldDisplayOrderSummary()
    {
        // Arrange
        _fixture.SetupHttpServiceMock("Orders", HttpMethod.Get, HomeComponentTestFixture.GetValidOrdersResponse());
        _fixture.SetupHttpServiceMock("Orders/summary", HttpMethod.Get, HomeComponentTestFixture.GetValidOrderSummaryResponse());

        // Act
        var component = _fixture.Render<Home>();

        // Assert
        component.Markup.Contains("Hexagonal Web UI Dashboard");
        component.Find("[data-testid='total-orders']").TextContent.Contains("17");
        component.Find("[data-testid='total-revenue']").TextContent.Contains("900.50 BRL");

        Assert.Equal(2, component.FindAll("[data-testid='orders-table'] tbody tr").Count);
        Assert.Equal("1", component.Find("[data-testid='order-id-1']").TextContent);
        Assert.Equal("Order 1", component.Find("[data-testid='order-customer-1']").TextContent);
        Assert.Equal("100.00", component.Find("[data-testid='order-amount-1']").TextContent);

        _fixture.MockLogger.VerifyError("Failed to retrieve orders.", 0);
        _fixture.MockLogger.VerifyError("Failed to retrieve order summary.", 0);
        _fixture.HttpServiceMock.Verify(x => x.SendAsync<BaseResponse<IEnumerable<OrderDto>>>("Orders", HttpMethod.Get, CancellationToken.None, null), Times.Once);
        _fixture.HttpServiceMock.Verify(x => x.SendAsync<GetOrderSummaryResponse>("Orders/summary", HttpMethod.Get, CancellationToken.None, null), Times.Once);
    }

    [Fact]
    public void GivenHomeComponentWhenViewItemsButtonClickedThenShouldDisplayOrderItems()
    {
        // Arrange
        _fixture.SetupHttpServiceMock("Orders", HttpMethod.Get, HomeComponentTestFixture.GetValidOrdersResponse());
        _fixture.SetupHttpServiceMock("Orders/summary", HttpMethod.Get, HomeComponentTestFixture.GetValidOrderSummaryResponse());

        // Act
        var component = _fixture.Render<Home>();
        component.Find("[data-testid='view-items-1']").Click();

        // Assert
        Assert.Single(component.FindAll("[data-testid='order-item-row']"));
        component.Find("[data-testid='order-items-section']");
        Assert.Contains("Item 1", component.Markup);
        Assert.Contains("Product A", component.Markup);
        Assert.Contains("50.00", component.Markup);
    }

    [Fact]
    public void GivenHomeComponentWhenRenderedAndOrdersFailThenShouldReturnDefaultValues()
    {
        // Arrange
        _fixture.SetupHttpServiceMock("Orders", HttpMethod.Get, HomeComponentTestFixture.GetInvalidOrdersResponse());
        _fixture.SetupHttpServiceMock("Orders/summary", HttpMethod.Get, HomeComponentTestFixture.GetValidOrderSummaryResponse());

        // Act
        var component = _fixture.Render<Home>();

        // Assert
        component.Markup.Contains("Hexagonal Web UI Dashboard");
        component.Find("[data-testid='total-orders']").TextContent.Contains("17");
        component.Find("[data-testid='total-revenue']").TextContent.Contains("900.50 BRL");
        component.Find("[data-testid='loading-orders']").TextContent.Contains("Loading orders...");

        _fixture.MockLogger.VerifyWarning("Failed to retrieve orders.");
        _fixture.MockLogger.VerifyError("Failed to retrieve order summary.", 0);
        _fixture.HttpServiceMock.Verify(x => x.SendAsync<BaseResponse<IEnumerable<OrderDto>>>("Orders", HttpMethod.Get, CancellationToken.None, null), Times.Once);
        _fixture.HttpServiceMock.Verify(x => x.SendAsync<GetOrderSummaryResponse>("Orders/summary", HttpMethod.Get, CancellationToken.None, null), Times.Once);
    }

    [Fact]
    public void GivenHomeComponentWhenRenderedAndBothCallsFailThenShouldReturnDefaultValues()
    {
        // Arrange
        _fixture.SetupHttpServiceMock("Orders", HttpMethod.Get, HomeComponentTestFixture.GetInvalidOrdersResponse());
        _fixture.SetupHttpServiceMock("Orders/summary", HttpMethod.Get, HomeComponentTestFixture.GetInvalidOrderSummaryResponse());

        // Act
        var component = _fixture.Render<Home>();

        // Assert
        component.Markup.Contains("Hexagonal Web UI Dashboard");
        component.Find("[data-testid='loading-summary']").TextContent.Contains("Loading summary...");

        Assert.DoesNotContain("data-testid=\"loading-orders\"", component.Markup);

        _fixture.MockLogger.VerifyWarning("Failed to retrieve order summary.");
        _fixture.MockLogger.VerifyWarning("Failed to retrieve orders.");
        _fixture.HttpServiceMock.Verify(x => x.SendAsync<BaseResponse<IEnumerable<OrderDto>>>("Orders", HttpMethod.Get, CancellationToken.None, null), Times.Once);
        _fixture.HttpServiceMock.Verify(x => x.SendAsync<GetOrderSummaryResponse>("Orders/summary", HttpMethod.Get, CancellationToken.None, null), Times.Once);
    }
}
