using Bunit;
using Infrastructure.Common;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using WebApp.Components.Pages;

namespace UnitTests.WebApp;

public class HomeComponentTestFixture : BaseComponentFixture
{
    public HomeComponentTestFixture()
    {
        Services.AddKeyedSingleton(ServicesKey.Orders.ToString(), HttpServiceMock.Object);
    }
}

public sealed class HomeComponentTests(HomeComponentTestFixture fixture) : IClassFixture<HomeComponentTestFixture>
{
    private readonly HomeComponentTestFixture _fixture = fixture;

    [Fact]
    public void GivenHomeComponentWhenRenderedThenShouldDisplayOrderSummary()
    {
        // Arrange, Act
        var component = _fixture.Render<Home>();

        // Assert – Home renders the Order component which contains the Orders heading
        Assert.Contains("Home", component.Markup, StringComparison.Ordinal);
        Assert.Contains("Orders", component.Markup, StringComparison.Ordinal);
    }
}
