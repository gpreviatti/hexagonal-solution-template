using Bunit;
using Contracts.Orders;
using UnitTests.Common;
using WebApp.Components.Pages;

namespace UnitTests.WebApp;

public class HomeComponentTestFixture : BaseComponentFixture
{
}

public sealed class HomeComponentTests(HomeComponentTestFixture fixture) : IClassFixture<HomeComponentTestFixture>
{
    private readonly HomeComponentTestFixture _fixture = fixture;

    [Fact]
    public void GivenHomeComponentWhenRenderedThenShouldDisplayOrderSummary()
    {
        // Arrange
        _fixture.SetupHttpServiceMock(new GetOrderSummaryResponse(
            success: true,
            data: new OrderSummaryDto(17, 900.50m, "BRL"),
            message: "Order summary retrieved.")
        );

        // Act
        var component = _fixture.RenderComponent<Home>();

        // Assert
        component.Markup.Contains("Hexagonal Web UI Dashboard");
        component.Find("[data-testid='total-orders']").TextContent.Contains("17");
        component.Find("[data-testid='total-revenue']").TextContent.Contains("900.50 BRL");
        _fixture.MockLogger.VerifyError("Failed to retrieve order summary.", 0);
    }

    [Fact]
    public void GivenHomeComponentWhenRenderedAndOrderSummaryFailsThenShouldReturnDefaultValues()
    {
        // Arrange
        _fixture.SetupHttpServiceMock(new GetOrderSummaryResponse(
            success: false,
            data: null,
            message: "Failed to retrieve order summary.")
        );

        // Act
        var component = _fixture.RenderComponent<Home>();

        // Assert
        component.Markup.Contains("Hexagonal Web UI Dashboard");
        _fixture.MockLogger.VerifyError("Failed to retrieve order summary.");
    }
}
