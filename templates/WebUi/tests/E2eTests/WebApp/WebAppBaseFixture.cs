using E2eTests.Common;

namespace E2eTests.WebApp;

[CollectionDefinition(nameof(WebAppBaseFixture))]
public sealed class WebAppBaseFixtureCollectionDefinition : IClassFixture<WebAppBaseFixture>;

public class WebAppBaseFixture : IAsyncLifetime
{
    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;
    public IPage Page { get; private set; } = null!;
    public Configurations Configurations { get; } = new();

    public async Task InitializeAsync()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            Args = ["--no-sandbox"]
        });
        Page = await Browser.NewPageAsync();
        Configurations.WebAppUrl = Environment.GetEnvironmentVariable("BASE_WEBAPP_URL") ?? "http://localhost:5013";
    }

    /// <summary>
    /// Navigate to the home page
    /// </summary>
    public async Task NavigateAsync() => await Page.GotoAsync(Configurations.WebAppUrl, new PageGotoOptions
    {
        WaitUntil = WaitUntilState.NetworkIdle,
        Timeout = Configurations.NavigationTimeoutMs
    });

    /// <summary>
    /// Get text content from a table cell
    /// </summary>
    public static async Task<string> GetTableCellTextAsync(IElementHandle row, int cellIndex)
    {
        var cells = await row.QuerySelectorAllAsync("td");
        Assert.True(cellIndex < cells.Count, $"Cell index {cellIndex} out of range. Total cells: {cells.Count}");

        var textContent = await cells[cellIndex].TextContentAsync();
        return textContent?.Trim() ?? string.Empty;
    }

    public async Task DisposeAsync()
    {
        await Page.CloseAsync();
        await Browser.CloseAsync();
        Playwright.Dispose();
    }
}
