using Microsoft.Playwright.Xunit;

namespace E2eTests.Common;

public class BrowserFixture : PageTest
{
    public int NavigationTimeoutMs { get; set; } = int.TryParse(Environment.GetEnvironmentVariable("NAVIGATION_TIMEOUT_MS"), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var navTimeout) ? navTimeout : 30000;
    public int WaitForSelectorTimeoutMs { get; set; } = int.TryParse(Environment.GetEnvironmentVariable("WAIT_FOR_SELECTOR_TIMEOUT_MS"), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var waitTimeout) ? waitTimeout : 10000;
    public string WebAppUrl { get; set; } = Environment.GetEnvironmentVariable("WEB_APP_URL") ?? "http://localhost:5013";

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        Page.SetDefaultNavigationTimeout(NavigationTimeoutMs);
        Page.SetDefaultTimeout(WaitForSelectorTimeoutMs);
        await Page.GotoAsync(WebAppUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
    }

    public override async Task DisposeAsync()
    {
        Console.WriteLine("After each test cleanup");
        await base.DisposeAsync();
    }
}
