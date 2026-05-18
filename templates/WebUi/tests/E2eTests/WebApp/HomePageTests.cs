using E2eTests.Common;

namespace E2eTests.WebApp;

public sealed class HomePageTests : BrowserFixture
{
    [Fact(DisplayName = nameof(GivenHomePageWhenStartsThenShouldDisplayHomePageContent))]
    public async Task GivenHomePageWhenStartsThenShouldDisplayHomePageContent()
    {
        // Arrange, Act, Assert
        var heading = await Page.TextContentAsync("title");
        Assert.Equal("Home", heading);
    }
}
