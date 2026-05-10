namespace E2eTests.Common;

/// <summary>
/// Extension methods for Playwright IPage to reduce code duplication and improve readability
/// </summary>
public static class PlaywrightExtensions
{
    /// <summary>
    /// Wait for table rows to be present and return their count
    /// </summary>
    public static async Task<int> WaitForTableRowsCountAsync(
        this IPage page,
        string tableSelector,
        int expectedMinCount = 1,
        int timeoutMs = 10000)
    {
        var rowSelector = $"{tableSelector} tbody tr";
        var startTime = DateTime.UtcNow;

        while ((DateTime.UtcNow - startTime).TotalMilliseconds < timeoutMs)
        {
            var rows = await page.QuerySelectorAllAsync(rowSelector);
            if (rows.Count >= expectedMinCount)
            {
                return rows.Count;
            }
            await page.WaitForTimeoutAsync(100);
        }

        throw new TimeoutException($"Expected at least {expectedMinCount} rows in table '{tableSelector}' within {timeoutMs}ms");
    }

    /// <summary>
    /// Click an element and wait for navigation/load to complete
    /// </summary>
    public static async Task ClickAndWaitForLoadAsync(
        this IPage page,
        IElementHandle element,
        LoadState loadState = LoadState.NetworkIdle,
        int timeoutMs = 30000)
    {
        await element.ClickAsync();
        await page.WaitForLoadStateAsync(loadState, new PageWaitForLoadStateOptions { Timeout = timeoutMs });
    }

    /// <summary>
    /// Click an element by selector and wait for navigation/load
    /// </summary>
    public static async Task ClickAndWaitForLoadAsync(
        this IPage page,
        string selector,
        LoadState loadState = LoadState.NetworkIdle,
        int timeoutMs = 30000)
    {
        await page.ClickAsync(selector);
        await page.WaitForLoadStateAsync(loadState, new PageWaitForLoadStateOptions { Timeout = timeoutMs });
    }

    /// <summary>
    /// Get text content from all cells in a table row
    /// </summary>
    public static async Task<List<string>> GetRowCellTextsAsync(this IElementHandle row)
    {
        var cells = await row.QuerySelectorAllAsync("td");
        var texts = new List<string>();

        foreach (var cell in cells)
        {
            var textContent = await cell.TextContentAsync();
            texts.Add(textContent?.Trim() ?? string.Empty);
        }

        return texts;
    }

    /// <summary>
    /// Wait for an element to be visible (not just present in DOM)
    /// </summary>
    public static async Task WaitForVisibleAsync(
        this IPage page,
        string selector,
        int timeoutMs = 10000)
    {
        var script = @"
            () => {
                const element = document.querySelector('" + selector + @"');
                if (!element) return false;
                const style = window.getComputedStyle(element);
                return style.display !== 'none' && style.visibility !== 'hidden' && style.opacity !== '0';
            }
        ";

        await page.WaitForFunctionAsync(
            script,
            new PageWaitForFunctionOptions { Timeout = timeoutMs });
    }

    /// <summary>
    /// Get all visible text content from a table
    /// </summary>
    public static async Task<List<List<string>>> GetTableDataAsync(this IPage page, string tableSelector)
    {
        var rowSelector = $"{tableSelector} tbody tr";
        var rows = await page.QuerySelectorAllAsync(rowSelector);
        var tableData = new List<List<string>>();

        foreach (var row in rows)
        {
            var rowTexts = await row.GetRowCellTextsAsync();
            tableData.Add(rowTexts);
        }

        return tableData;
    }
}
