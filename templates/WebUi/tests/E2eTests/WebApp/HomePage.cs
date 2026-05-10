using E2eTests.Common;

namespace E2eTests.WebApp;

public class HomePage(IPage page)
{
    private readonly IPage _page = page ?? throw new ArgumentNullException(nameof(page));
    private readonly Configurations _config = new();

    // Selectors (using data-testid attributes from Home.razor)
    private const string LoadingSummarySelector = "[data-testid='loading-summary']";
    private const string SummaryTotalOrdersSelector = "[data-testid='total-orders']";
    private const string SummaryRevenueSelector = "[data-testid='total-revenue']";
    private const string OrdersTableSelector = "[data-testid='orders-table']";
    private const string OrdersTableRowsSelector = "[data-testid='orders-table'] tbody tr";
    private const string ViewItemsButtonSelector = "[data-testid^='view-items-']";
    private const string OrderItemsTableSelector = "[data-testid='order-items-table']";
    private const string OrderItemsTableRowsSelector = "[data-testid='order-items-table'] tbody tr";

    /// <summary>
    /// Navigate to the home page
    /// </summary>
    public async Task NavigateAsync() => await _page.GotoAsync(_config.WebAppUrl, new PageGotoOptions
    {
        WaitUntil = WaitUntilState.NetworkIdle,
        Timeout = _config.NavigationTimeoutMs
    });

    /// <summary>
    /// Wait for the summary section to fully load
    /// </summary>
    public async Task WaitForSummaryAsync() =>
        // Wait for loading state to disappear and data to appear
        await _page.WaitForSelectorAsync(SummaryTotalOrdersSelector, new PageWaitForSelectorOptions
        {
            Timeout = _config.APICallTimeoutMs
        });

    /// <summary>
    /// Check if loading indicator is displayed
    /// </summary>
    public async Task<bool> IsLoadingDisplayedAsync()
    {
        try
        {
            await _page.WaitForSelectorAsync(LoadingSummarySelector, new PageWaitForSelectorOptions
            {
                Timeout = 1000 // Quick check
            });
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get the total orders count from summary
    /// </summary>
    public async Task<string> GetSummaryTotalOrdersAsync()
    {
        var element = await _page.QuerySelectorAsync(SummaryTotalOrdersSelector);
        Assert.NotNull(element);
        var textContent = await element.TextContentAsync();
        return textContent?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Get the revenue from summary
    /// </summary>
    public async Task<string> GetSummaryRevenueAsync()
    {
        var element = await _page.QuerySelectorAsync(SummaryRevenueSelector);
        Assert.NotNull(element);
        var textContent = await element.TextContentAsync();
        return textContent?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Get all order table rows
    /// </summary>
    public async Task<IReadOnlyList<IElementHandle>> GetOrderTableRowsAsync() => await _page.QuerySelectorAllAsync(OrdersTableRowsSelector);

    /// <summary>
    /// Verify orders table is visible and populated
    /// </summary>
    public async Task<bool> IsOrdersTableVisibleAsync()
    {
        try
        {
            await _page.WaitForSelectorAsync(OrdersTableSelector, new PageWaitForSelectorOptions
            {
                Timeout = _config.WaitForSelectorTimeoutMs
            });
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Expand order items for a specific row (0-based index)
    /// </summary>
    public async Task ExpandOrderItemsAsync(int rowIndex = 0)
    {
        var rows = await GetOrderTableRowsAsync();
        Assert.NotEmpty(rows);
        Assert.True(rowIndex < rows.Count, $"Row index {rowIndex} out of range. Total rows: {rows.Count}");

        var row = rows[rowIndex];

        // Find the button within the row (it has data-testid="view-items-{orderId}")
        var buttons = await row.QuerySelectorAllAsync("button[data-testid^='view-items-']");
        Assert.NotEmpty(buttons);

        await buttons[0].ClickAsync();

        // Wait for items table to appear
        await _page.WaitForSelectorAsync(OrderItemsTableSelector, new PageWaitForSelectorOptions
        {
            Timeout = _config.WaitForSelectorTimeoutMs
        });
    }

    /// <summary>
    /// Get all order items table rows
    /// </summary>
    public async Task<IReadOnlyList<IElementHandle>> GetOrderItemsTableRowsAsync() => await _page.QuerySelectorAllAsync(OrderItemsTableRowsSelector);

    /// <summary>
    /// Verify order items table is visible
    /// </summary>
    public async Task<bool> IsOrderItemsTableVisibleAsync()
    {
        try
        {
            await _page.WaitForSelectorAsync(OrderItemsTableSelector, new PageWaitForSelectorOptions
            {
                Timeout = _config.WaitForSelectorTimeoutMs
            });
            return true;
        }
        catch
        {
            return false;
        }
    }

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
}
