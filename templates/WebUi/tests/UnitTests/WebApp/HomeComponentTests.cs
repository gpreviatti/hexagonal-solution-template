using Bunit;
using Contracts.Orders;
using UnitTests.Common;
using WebApp.Components.Pages;

namespace UnitTests.WebApp;

public class HomeComponentTestFixture : BaseComponentFixture
{
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
        _fixture.SetupHttpServiceMock(HomeComponentTestFixture.GetValidOrderSummaryResponse());

        // Act
        var component = _fixture.Render<Home>();

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
        _fixture.SetupHttpServiceMock(HomeComponentTestFixture.GetInvalidOrderSummaryResponse());

        // Act
        var component = _fixture.Render<Home>();

        // Assert
        component.Markup.Contains("Hexagonal Web UI Dashboard");
        component.Find("[data-testid='loading-summary']").TextContent.Contains("Loading summary...");
        _fixture.MockLogger.VerifyWarning("Failed to retrieve order summary.");
    }
}
