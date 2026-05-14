namespace E2eTests.WebApp;

[Collection(nameof(WebAppBaseFixture))]
public sealed class HomePageTests(HomePageFixture homePageFixture) : IClassFixture<HomePageFixture>, IAsyncLifetime
{
    private readonly HomePageFixture _homePageFixture = homePageFixture;

    public async Task InitializeAsync() => await _homePageFixture.InitializeAsync();

    public async Task DisposeAsync() => await _homePageFixture.DisposeAsync();

    /// <summary>
    /// Test 1: Page Load - Verify loading states and data appears
    /// Navigate to home → verify loading states shown → verify summary appears → verify orders table populated
    /// </summary>
    [Fact(DisplayName = "Home Page - Page Load displays loading states and data")]
    public async Task GivenHomePageWhenNavigatingThenShouldLoadSummaryAndOrdersTable()
    {
        // Arrange
        Assert.NotNull(_homePageFixture);

        // Act
        await _homePageFixture.NavigateAsync();

        // Assert - Loading state should appear initially
        var isLoading = await _homePageFixture.IsLoadingDisplayedAsync();
        Assert.False(isLoading, "Loading state should not be displayed after navigation");

        // Wait for summary to load
        await _homePageFixture.WaitForSummaryAsync();
        var totalOrdersText = await _homePageFixture.GetSummaryTotalOrdersAsync();
        Assert.False(string.IsNullOrEmpty(totalOrdersText));

        // Verify orders table is visible
        var isTableVisible = await _homePageFixture.IsOrdersTableVisibleAsync();
        Assert.True(isTableVisible);

        var rows = await _homePageFixture.GetOrderTableRowsAsync();
        Assert.NotEmpty(rows);
    }

    /// <summary>
    /// Test 2: Order Summary - Verify summary displays correct total orders count and revenue
    /// </summary>
    [Fact(DisplayName = "Home Page - Order Summary displays total orders and revenue")]
    public async Task GivenHomePageWhenLoadedThenShouldDisplayOrderSummary()
    {
        // Arrange
        Assert.NotNull(_homePageFixture);

        // Act
        await _homePageFixture.NavigateAsync();
        await _homePageFixture.WaitForSummaryAsync();

        // Assert
        var totalOrdersText = await _homePageFixture.GetSummaryTotalOrdersAsync();
        Assert.NotEmpty(totalOrdersText);

        var revenueText = await _homePageFixture.GetSummaryRevenueAsync();
        Assert.NotEmpty(revenueText);

        // Verify both values are not just whitespace
        Assert.False(string.IsNullOrWhiteSpace(totalOrdersText));
        Assert.False(string.IsNullOrWhiteSpace(revenueText));
    }

    /// <summary>
    /// Test 3: Orders Table - Verify table has expected columns and at least 1 row
    /// </summary>
    [Fact(DisplayName = "Home Page - Orders Table displays expected columns and rows")]
    public async Task GivenHomePageWhenLoadedThenShouldDisplayOrdersTableWithData()
    {
        // Arrange
        Assert.NotNull(_homePageFixture);

        // Act
        await _homePageFixture.NavigateAsync();
        await _homePageFixture.WaitForSummaryAsync();

        // Assert
        var isTableVisible = await _homePageFixture.IsOrdersTableVisibleAsync();
        Assert.True(isTableVisible, "Orders table should be visible");

        var rows = await _homePageFixture.GetOrderTableRowsAsync();
        Assert.NotEmpty(rows);
        Assert.True(rows.Count >= 1, "Should have at least one order row");

        // Verify first row has content (Id, Description, Total columns at minimum)
        var firstRow = rows[0];
        var cells = await firstRow.QuerySelectorAllAsync("td");
        Assert.True(cells.Count >= 3, "Each row should have at least 3 columns (Id, Description, Total)");

        // Get cell texts to verify data exists
        var firstCellText = await WebAppBaseFixture.GetTableCellTextAsync(firstRow, 0);
        var secondCellText = await WebAppBaseFixture.GetTableCellTextAsync(firstRow, 1);

        Assert.False(string.IsNullOrWhiteSpace(firstCellText), "First cell (Id) should have content");
        Assert.False(string.IsNullOrWhiteSpace(secondCellText), "Second cell (Description) should have content");
    }

    /// <summary>
    /// Test 4: Expand Items - Click "View items" on first order → verify items table appears
    /// </summary>
    [Fact(DisplayName = "Home Page - Expand Items displays order items table")]
    public async Task GivenOrderRowWhenExpandingItemsThenShouldDisplayOrderItemsTable()
    {
        // Arrange
        Assert.NotNull(_homePageFixture);

        // Act
        await _homePageFixture.NavigateAsync();
        await _homePageFixture.WaitForSummaryAsync();

        var orderRows = await _homePageFixture.GetOrderTableRowsAsync();
        Assert.NotEmpty(orderRows);

        // Expand first order's items
        await _homePageFixture.ExpandOrderItemsAsync(rowIndex: 0);

        // Assert - Items table should be visible
        var isItemsTableVisible = await _homePageFixture.IsOrderItemsTableVisibleAsync();
        Assert.True(isItemsTableVisible, "Order items table should be visible after expansion");

        var itemRows = await _homePageFixture.GetOrderItemsTableRowsAsync();
        Assert.NotEmpty(itemRows);
        Assert.True(itemRows.Count >= 1, "Should have at least one item row");

        // Verify item row has content (Id, Name, Description, Value columns)
        var firstItemRow = itemRows[0];
        var itemCells = await firstItemRow.QuerySelectorAllAsync("td");
        Assert.True(itemCells.Count >= 4, "Each item row should have at least 4 columns (Id, Name, Description, Value)");

        // Verify cells have content
        var itemIdText = await WebAppBaseFixture.GetTableCellTextAsync(firstItemRow, 0);
        var itemNameText = await WebAppBaseFixture.GetTableCellTextAsync(firstItemRow, 1);

        Assert.False(string.IsNullOrWhiteSpace(itemIdText), "Item Id should have content");
        Assert.False(string.IsNullOrWhiteSpace(itemNameText), "Item Name should have content");
    }
}
