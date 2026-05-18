using E2eTests.Common;

namespace E2eTests.WebApp;

public class OrderPageFixture : BrowserFixture
{
    private const string LoadingSummarySelector = "[data-testid='loading-summary']";
    private const string SummaryTotalOrdersSelector = "[data-testid='total-orders']";
    private const string SummaryRevenueSelector = "[data-testid='total-revenue']";
    private const string OrdersTableSelector = "[data-testid='orders-table']";
    private const string OrdersTableRowsSelector = "[data-testid='orders-table'] tbody tr";
    private const string OrderItemsTableSelector = "[data-testid='order-items-table']";
    private const string OrderItemsTableRowsSelector = "[data-testid='order-items-table'] tbody tr";

    public async Task WaitForSummaryAsync() => await Page.WaitForSelectorAsync(
        SummaryTotalOrdersSelector,
        new() { Timeout = WaitForSelectorTimeoutMs }
    );

    public async Task<bool> IsLoadingDisplayedAsync()
    {
        try
        {
            await Page.WaitForSelectorAsync(LoadingSummarySelector, new() { Timeout = 1000 });
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetSummaryTotalOrdersAsync()
    {
        var element = await Page.QuerySelectorAsync(SummaryTotalOrdersSelector);
        Assert.NotNull(element);

        var textContent = await element.TextContentAsync();
        return textContent?.Trim() ?? string.Empty;
    }

    public async Task<string> GetSummaryRevenueAsync()
    {
        var element = await Page.QuerySelectorAsync(SummaryRevenueSelector);
        Assert.NotNull(element);

        var textContent = await element.TextContentAsync();
        return textContent?.Trim() ?? string.Empty;
    }

    public async Task<IReadOnlyList<IElementHandle>> GetOrderTableRowsAsync() => await Page.QuerySelectorAllAsync(OrdersTableRowsSelector);

    public async Task<bool> IsOrdersTableVisibleAsync()
    {
        try
        {
            await Page.WaitForSelectorAsync(OrdersTableSelector, new() { Timeout = WaitForSelectorTimeoutMs });
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task ExpandOrderItemsAsync(int rowIndex = 0)
    {
        var rows = await GetOrderTableRowsAsync();
        Assert.NotEmpty(rows);
        Assert.True(rowIndex < rows.Count, $"Row index {rowIndex} out of range. Total rows: {rows.Count}");

        var row = rows[rowIndex];

        var buttons = await row.QuerySelectorAllAsync("button[data-testid^='view-items-']");
        Assert.NotEmpty(buttons);

        await buttons[0].ClickAsync();

        await Page.WaitForSelectorAsync(OrderItemsTableSelector, new() { Timeout = WaitForSelectorTimeoutMs });
    }

    public async Task<IReadOnlyList<IElementHandle>> GetOrderItemsTableRowsAsync() => await Page.QuerySelectorAllAsync(OrderItemsTableRowsSelector);

    public async Task<bool> IsOrderItemsTableVisibleAsync()
    {
        try
        {
            await Page.WaitForSelectorAsync(OrderItemsTableSelector, new() { Timeout = WaitForSelectorTimeoutMs });
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public sealed class OrderPageTests : OrderPageFixture
{

    [Fact(DisplayName = nameof(GivenOrderPageWhenNavigatingThenShouldLoadSummaryAndOrdersTable))]
    public async Task GivenOrderPageWhenNavigatingThenShouldLoadSummaryAndOrdersTable()
    {
        // Arrange, Act
        var isLoading = await IsLoadingDisplayedAsync();
        Assert.False(isLoading, "Loading state should not be displayed after navigation");

        // Assert
        await WaitForSummaryAsync();
        var totalOrdersText = await GetSummaryTotalOrdersAsync();
        Assert.False(string.IsNullOrEmpty(totalOrdersText));

        var isTableVisible = await IsOrdersTableVisibleAsync();
        Assert.True(isTableVisible);

        var rows = await GetOrderTableRowsAsync();
        Assert.NotEmpty(rows);
    }

    [Fact(DisplayName = nameof(GivenOrderPageWhenLoadedThenShouldDisplayOrderSummary))]
    public async Task GivenOrderPageWhenLoadedThenShouldDisplayOrderSummary()
    {
        // Arrange, Act
        await WaitForSummaryAsync();

        // Assert
        var totalOrdersText = await GetSummaryTotalOrdersAsync();
        Assert.NotEmpty(totalOrdersText);

        var revenueText = await GetSummaryRevenueAsync();
        Assert.NotEmpty(revenueText);

        Assert.False(string.IsNullOrWhiteSpace(totalOrdersText));
        Assert.False(string.IsNullOrWhiteSpace(revenueText));
    }

    [Fact(DisplayName = nameof(GivenOrderPageWhenLoadedThenShouldDisplayOrdersTableWithData))]
    public async Task GivenOrderPageWhenLoadedThenShouldDisplayOrdersTableWithData()
    {
        // Arrange, Act
        await WaitForSummaryAsync();

        // Assert
        var isTableVisible = await IsOrdersTableVisibleAsync();
        Assert.True(isTableVisible, "Orders table should be visible");

        var rows = await GetOrderTableRowsAsync();
        Assert.NotEmpty(rows);
        Assert.True(rows.Count >= 1, "Should have at least one order row");

        var firstRow = rows[0];
        var cells = await firstRow.QuerySelectorAllAsync("td");
        Assert.True(cells.Count >= 3, "Each row should have at least 3 columns (Id, Description, Total)");

        var firstCellText = await PlaywrightExtensions.GetTableCellTextAsync(firstRow, 0);
        var secondCellText = await PlaywrightExtensions.GetTableCellTextAsync(firstRow, 1);

        Assert.False(string.IsNullOrWhiteSpace(firstCellText), "First cell (Id) should have content");
        Assert.False(string.IsNullOrWhiteSpace(secondCellText), "Second cell (Description) should have content");
    }

    [Fact(DisplayName = nameof(GivenOrderRowWhenExpandingItemsThenShouldDisplayOrderItemsTable))]
    public async Task GivenOrderRowWhenExpandingItemsThenShouldDisplayOrderItemsTable()
    {
        // Arrange, Act
        await WaitForSummaryAsync();

        var orderRows = await GetOrderTableRowsAsync();
        Assert.NotEmpty(orderRows);

        await ExpandOrderItemsAsync(rowIndex: 0);

        // Assert
        var isItemsTableVisible = await IsOrderItemsTableVisibleAsync();
        Assert.True(isItemsTableVisible, "Order items table should be visible after expansion");

        var itemRows = await GetOrderItemsTableRowsAsync();
        Assert.NotEmpty(itemRows);
        Assert.True(itemRows.Count >= 1, "Should have at least one item row");

        var firstItemRow = itemRows[0];
        var itemCells = await firstItemRow.QuerySelectorAllAsync("td");
        Assert.True(itemCells.Count >= 4, "Each item row should have at least 4 columns (Id, Name, Description, Value)");

        var itemIdText = await PlaywrightExtensions.GetTableCellTextAsync(firstItemRow, 0);
        var itemNameText = await PlaywrightExtensions.GetTableCellTextAsync(firstItemRow, 1);

        Assert.False(string.IsNullOrWhiteSpace(itemIdText), "Item Id should have content");
        Assert.False(string.IsNullOrWhiteSpace(itemNameText), "Item Name should have content");
    }
}
