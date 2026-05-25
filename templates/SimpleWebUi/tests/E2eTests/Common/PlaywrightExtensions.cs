namespace E2eTests.Common;

/// <summary>
/// Extension methods for Playwright IPage to reduce code duplication and improve readability
/// </summary>
public static class PlaywrightExtensions
{
    /// <summary>
    /// Click an element by selector and wait for navigation/load
    /// </summary>
    public static async Task ClickAndWaitForLoadAsync(
        this IPage page,
        string selector,
        LoadState loadState = LoadState.NetworkIdle,
        int timeoutMs = 30000
    )
    {
        await page.ClickAsync(selector);
        await page.WaitForLoadStateAsync(loadState, new PageWaitForLoadStateOptions { Timeout = timeoutMs });
    }

    /// <summary>
    /// Navigate to the home page
    /// </summary>
    public static async Task NavigateAsync(this IPage page, string url, int timeoutMs = 30000) => await page.GotoAsync(url, new()
    {
        WaitUntil = WaitUntilState.NetworkIdle,
        Timeout = timeoutMs
    });

    /// <summary>
    /// Get text content from the first h3 element on the page, commonly used for page headings
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    public static async Task<string> GetHeadingTextAsync(this IPage page)
    {
        var element = await page.QuerySelectorAsync(Selectors.PageHeading);
        Assert.NotNull(element);

        var textContent = await element.TextContentAsync();
        return textContent?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Wait for the main heading (h1) to be present, indicating that the page has loaded. This is a common pattern for our pages, but can be customized as needed.
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    public static async Task WaitForPageLoadAsync(this IPage page, int timeoutMs = 10000) => await page.WaitForSelectorAsync("h1", new() { Timeout = timeoutMs });

    /// <summary>
    /// Click the submit button and wait for navigation/load to complete. This is a common action on our forms, but can be customized as needed.
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    public static async Task ClickSubmitAsync(this IPage page) => await page.ClickAsync(Selectors.ButtonSubmit);

    /// <summary>
    /// Get all row elements from a table. This can be used to verify the number of rows or to extract data from the table. The selector can be customized as needed.
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    public static async Task<IReadOnlyList<IElementHandle>> GetTableRowsAsync(this IPage page) => await page.QuerySelectorAllAsync(Selectors.TableRows);

    /// <summary>
    /// Wait for the form to be ready by waiting for the submit button to be present. This is a common pattern for our forms, but can be customized as needed.
    /// </summary>
    /// <param name="page"></param>
    /// <param name="timeoutMs"></param>
    /// <returns></returns>
    public static async Task WaitForFormAsync(this IPage page, int timeoutMs = 10000) => await page.WaitForSelectorAsync(Selectors.ButtonSubmit, new() { Timeout = timeoutMs });

    /// <summary>
    /// Check if the main table is visible on the page. This can be used to verify that data is being displayed correctly. The selector can be customized as needed.
    /// </summary>
    /// <param name="page"></param>
    /// <param name="timeoutMs"></param>
    /// <returns></returns>
    public static async Task<bool> IsTableVisibleAsync(this IPage page, int timeoutMs = 10000)
    {
        try
        {
            await page.WaitForSelectorAsync(Selectors.Table, new() { Timeout = timeoutMs });
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Check if the empty state message is visible on the page. This can be used to verify that the correct message is shown when there is no data. The implementation can be customized as needed based on how the empty state is rendered in the application.
    /// </summary>
    /// <param name="page"></param>
    /// <param name="timeoutMs"></param>
    /// <returns></returns>
    public static async Task<bool> IsOrderCardVisibleAsync(this IPage page, int timeoutMs = 10000)
    {
        try
        {
            await page.WaitForSelectorAsync(".card", new() { Timeout = timeoutMs });
            return true;
        }
        catch
        {
            return false;
        }
    }
}
