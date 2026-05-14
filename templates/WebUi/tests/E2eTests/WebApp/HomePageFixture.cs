namespace E2eTests.WebApp;

public sealed class HomePageFixture : WebAppBaseFixture
{
    private const string LoadingSummarySelector = "[data-testid='loading-summary']";
    private const string SummaryTotalOrdersSelector = "[data-testid='total-orders']";
    private const string SummaryRevenueSelector = "[data-testid='total-revenue']";
    private const string OrdersTableSelector = "[data-testid='orders-table']";
    private const string OrdersTableRowsSelector = "[data-testid='orders-table'] tbody tr";
    private const string OrderItemsTableSelector = "[data-testid='order-items-table']";
    private const string OrderItemsTableRowsSelector = "[data-testid='order-items-table'] tbody tr";


    /// <summary>
    /// Wait for the summary section to fully load
    /// </summary>
    public async Task WaitForSummaryAsync() => await Page.WaitForSelectorAsync(SummaryTotalOrdersSelector, new PageWaitForSelectorOptions
    {
        Timeout = Configurations.APICallTimeoutMs
    });

    /// <summary>
    /// Check if loading indicator is displayed
    /// </summary>
    public async Task<bool> IsLoadingDisplayedAsync()
    {
        try
        {
            await Page.WaitForSelectorAsync(LoadingSummarySelector, new PageWaitForSelectorOptions
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
        var element = await Page.QuerySelectorAsync(SummaryTotalOrdersSelector);
        Assert.NotNull(element);
        var textContent = await element.TextContentAsync();
        return textContent?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Get the revenue from summary
    /// </summary>
    public async Task<string> GetSummaryRevenueAsync()
    {
        var element = await Page.QuerySelectorAsync(SummaryRevenueSelector);
        Assert.NotNull(element);
        var textContent = await element.TextContentAsync();
        return textContent?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Get all order table rows
    /// </summary>
    public async Task<IReadOnlyList<IElementHandle>> GetOrderTableRowsAsync() => await Page.QuerySelectorAllAsync(OrdersTableRowsSelector);

    /// <summary>
    /// Verify orders table is visible and populated
    /// </summary>
    public async Task<bool> IsOrdersTableVisibleAsync()
    {
        try
        {
            await Page.WaitForSelectorAsync(OrdersTableSelector, new PageWaitForSelectorOptions
            {
                Timeout = Configurations.WaitForSelectorTimeoutMs
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
        await Page.WaitForSelectorAsync(OrderItemsTableSelector, new PageWaitForSelectorOptions
        {
            Timeout = Configurations.WaitForSelectorTimeoutMs
        });
    }

    /// <summary>
    /// Get all order items table rows
    /// </summary>
    public async Task<IReadOnlyList<IElementHandle>> GetOrderItemsTableRowsAsync() => await Page.QuerySelectorAllAsync(OrderItemsTableRowsSelector);

    /// <summary>
    /// Verify order items table is visible
    /// </summary>
    public async Task<bool> IsOrderItemsTableVisibleAsync()
    {
        try
        {
            await Page.WaitForSelectorAsync(OrderItemsTableSelector, new PageWaitForSelectorOptions
            {
                Timeout = Configurations.WaitForSelectorTimeoutMs
            });
            return true;
        }
        catch
        {
            return false;
        }
    }


}
