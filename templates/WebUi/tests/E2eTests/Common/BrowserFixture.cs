namespace E2eTests.Common;

public class BrowserFixture : IAsyncLifetime
{
    private IBrowser? _browser;
    private IBrowserContext? _browserContext;
    private IPage? _page;
    private readonly Configurations _config;

    public IPage Page => _page ?? throw new InvalidOperationException("Page not initialized. Ensure InitializeAsync has been called.");

    public BrowserFixture()
    {
        _config = new Configurations();
    }

    public async Task InitializeAsync()
    {
        using var playwright = await Playwright.CreateAsync();

        _browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            SlowMo = 100
        });

        _browserContext = await _browser.NewContextAsync();

        _page = await _browserContext.NewPageAsync();

        _page.SetDefaultNavigationTimeout(_config.NavigationTimeoutMs);
        _page.SetDefaultTimeout(_config.WaitForSelectorTimeoutMs);
    }

    public async Task DisposeAsync()
    {
        if (_page is not null)
        {
            await _page.CloseAsync();
        }

        if (_browserContext is not null)
        {
            await _browserContext.CloseAsync();
        }

        if (_browser is not null)
        {
            await _browser.CloseAsync();
        }
    }
}
