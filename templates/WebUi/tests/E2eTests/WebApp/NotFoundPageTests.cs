using E2eTests.Common;

namespace E2eTests.WebApp;

public class NotFoundPageFixture : BrowserFixture
{
    public async Task GotoInvalidPageAsync() =>
    await Page.GotoAsync($"{WebAppUrl}/this-page-does-not-exist", new() { WaitUntil = WaitUntilState.NetworkIdle });

    public async Task<string> GetHeadingTextAsync()
    {
        var element = await Page.QuerySelectorAsync("h3");
        Assert.NotNull(element);

        var textContent = await element.TextContentAsync();
        return textContent?.Trim() ?? string.Empty;
    }

    public async Task<string> GetMessageTextAsync()
    {
        var element = await Page.QuerySelectorAsync("p");
        Assert.NotNull(element);

        var textContent = await element.TextContentAsync();
        return textContent?.Trim() ?? string.Empty;
    }

    public async Task<IReadOnlyList<IElementHandle>> GetAllElementsByTagAsync(string tagName) =>
        await Page.QuerySelectorAllAsync(tagName);
}

public sealed class NotFoundPageTests : NotFoundPageFixture
{
    [Fact(DisplayName = nameof(GivenNotFoundPageWhenNavigatingToInvalidRouteThenShouldDisplayNotFoundMessageAsync))]
    public async Task GivenNotFoundPageWhenNavigatingToInvalidRouteThenShouldDisplayNotFoundMessageAsync()
    {
        // Arrange, Act
        await GotoInvalidPageAsync();
        var heading = await GetHeadingTextAsync();
        var message = await GetMessageTextAsync();

        // Assert
        Assert.NotEmpty(heading);
        Assert.NotEmpty(message);
        Assert.Equal("Not Found", heading);
        Assert.Contains("content you are looking for does not exist", message, StringComparison.Ordinal);
    }

    [Fact(DisplayName = nameof(GivenNotFoundPageThenShouldDisplayCorrectHeadingAsync))]
    public async Task GivenNotFoundPageThenShouldDisplayCorrectHeadingAsync()
    {
        // Arrange, Act
        await GotoInvalidPageAsync();
        var heading = await GetHeadingTextAsync();

        // Assert
        Assert.NotNull(heading);
        Assert.Equal("Not Found", heading);
    }

    [Fact(DisplayName = nameof(GivenNotFoundPageThenShouldDisplayHelpfulErrorMessageAsync))]
    public async Task GivenNotFoundPageThenShouldDisplayHelpfulErrorMessageAsync()
    {
        // Arrange, Act
        await GotoInvalidPageAsync();
        var message = await GetMessageTextAsync();

        // Assert
        Assert.NotNull(message);
        Assert.False(string.IsNullOrWhiteSpace(message), "Error message should not be empty");
        Assert.Contains("Sorry", message, StringComparison.Ordinal);
        Assert.Contains("does not exist", message, StringComparison.Ordinal);
    }

    [Fact(DisplayName = nameof(GivenNotFoundPageWhenNavigatingDirectlyToNotFoundRouteThenShouldDisplayPageAsync))]
    public async Task GivenNotFoundPageWhenNavigatingDirectlyToNotFoundRouteThenShouldDisplayPageAsync()
    {
        // Arrange, Act
        await GotoInvalidPageAsync();

        var heading = await GetHeadingTextAsync();
        var message = await GetMessageTextAsync();

        // Assert
        Assert.NotEmpty(heading);
        Assert.NotEmpty(message);
        Assert.Equal("Not Found", heading);
    }
}
