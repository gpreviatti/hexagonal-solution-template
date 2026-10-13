using E2eTests.Common;

namespace E2eTests.WebApp;

public sealed class HomePageTests : BrowserFixture
{
    [Fact(DisplayName = nameof(GivenHomePageWhenStartsThenShouldDisplayHomePageContentAsync))]
    public async Task GivenHomePageWhenStartsThenShouldDisplayHomePageContentAsync()
    {
        // Arrange, Act, Assert
        var heading = await Page.TextContentAsync("title");
        Assert.Equal("Home", heading);
    }
}
