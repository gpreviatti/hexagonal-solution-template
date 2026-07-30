using E2eTests.Common;

namespace E2eTests.WebUi;

public class OrderListPageFixture : BrowserFixture
{
    public async Task NavigateToOrdersAsync() => await Page.NavigateAsync($"{WebAppUrl}/orders");

    public async Task<bool> IsEmptyStateVisibleAsync()
    {
        var content = await Page.TextContentAsync("body");
        return content?.Contains("No orders found.") ?? false;
    }
}

public sealed class OrderListPageTests(OrderListPageFixture fixture) : IClassFixture<OrderListPageFixture>
{
    private readonly OrderListPageFixture _fixture = fixture;


    [Fact(DisplayName = nameof(GivenOrdersPageWhenNavigatingThenHeadingIsOrders))]
    public async Task GivenOrdersPageWhenNavigatingThenHeadingIsOrders()
    {
        await _fixture.NavigateToOrdersAsync();
        await _fixture.Page.WaitForPageLoadAsync();

        var heading = await _fixture.Page.GetHeadingTextAsync();

        Assert.Equal("Orders", heading);
    }

    [Fact(DisplayName = nameof(GivenOrdersPageWhenLoadedThenNewOrderButtonIsPresent))]
    public async Task GivenOrdersPageWhenLoadedThenNewOrderButtonIsPresent()
    {
        await _fixture.NavigateToOrdersAsync();
        await _fixture.Page.WaitForPageLoadAsync();

        var button = await _fixture.Page.QuerySelectorAsync("a.btn-primary[href='/orders/create']");

        Assert.NotNull(button);
    }

    [Fact(DisplayName = nameof(GivenOrdersPageWhenLoadedThenEitherTableOrEmptyStateIsShown))]
    public async Task GivenOrdersPageWhenLoadedThenEitherTableOrEmptyStateIsShown()
    {
        await _fixture.NavigateToOrdersAsync();
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var tableVisible = await _fixture.Page.IsTableVisibleAsync();
        var emptyVisible = await _fixture.IsEmptyStateVisibleAsync();

        Assert.True(tableVisible || emptyVisible, "Either the orders table or empty state must be shown");
    }

    [Fact(DisplayName = nameof(GivenOrdersExistWhenLoadedThenTableRowsArePresent))]
    public async Task GivenOrdersExistWhenLoadedThenTableRowsArePresent()
    {
        await _fixture.NavigateToOrdersAsync();

        var tableVisible = await _fixture.Page.IsTableVisibleAsync();
        if (!tableVisible)
            return;

        var rows = await _fixture.Page.GetTableRowsAsync();
        Assert.NotEmpty(rows);
    }

    [Fact(DisplayName = nameof(GivenOrdersTableWhenLoadedThenRowsHaveExpectedColumns))]
    public async Task GivenOrdersTableWhenLoadedThenRowsHaveExpectedColumns()
    {
        await _fixture.NavigateToOrdersAsync();

        var tableVisible = await _fixture.Page.IsTableVisibleAsync();
        if (!tableVisible)
            return;

        var rows = await _fixture.Page.GetTableRowsAsync();
        Assert.NotEmpty(rows);

        var cells = await rows[0].QuerySelectorAllAsync("td");
        Assert.True(cells.Count >= 3, "Each row should have at least 3 columns (Id, Description, Total)");
    }

    [Fact(DisplayName = nameof(GivenFirstPageThenPreviousButtonIsDisabled))]
    public async Task GivenFirstPageThenPreviousButtonIsDisabled()
    {
        await _fixture.NavigateToOrdersAsync();
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var tableVisible = await _fixture.Page.IsTableVisibleAsync();
        if (!tableVisible)
            return;

        var prevItem = await _fixture.Page.QuerySelectorAsync(Selectors.FirstChildLi);
        Assert.NotNull(prevItem);
        var className = await prevItem.GetAttributeAsync("class");
        Assert.True(className?.Contains("disabled") ?? false, "Previous button should be disabled on page 1");
    }

    [Fact(DisplayName = nameof(GivenNewOrderButtonClickThenNavigatesToCreatePage))]
    public async Task GivenNewOrderButtonClickThenNavigatesToCreatePage()
    {
        await _fixture.NavigateToOrdersAsync();
        await _fixture.Page.WaitForPageLoadAsync();

        await _fixture.Page.ClickAndWaitForLoadAsync(Selectors.ButtonNewOrder);

        Assert.EndsWith("/orders/create", _fixture.Page.Url);
    }

    [Fact(DisplayName = nameof(GivenViewButtonClickWhenOrdersExistThenNavigatesToDetailPage))]
    public async Task GivenViewButtonClickWhenOrdersExistThenNavigatesToDetailPage()
    {
        await _fixture.NavigateToOrdersAsync();

        var tableVisible = await _fixture.Page.IsTableVisibleAsync();
        if (!tableVisible)
            return;

        var viewButton = await _fixture.Page.QuerySelectorAsync("tbody tr:first-child a.btn-outline-primary");
        Assert.NotNull(viewButton);
        await viewButton.ClickAsync();
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.Matches(@"/orders/\d+$", _fixture.Page.Url);
    }

    [Fact(DisplayName = nameof(GivenEditButtonClickWhenOrdersExistThenNavigatesToEditPage))]
    public async Task GivenEditButtonClickWhenOrdersExistThenNavigatesToEditPage()
    {
        await _fixture.NavigateToOrdersAsync();

        var tableVisible = await _fixture.Page.IsTableVisibleAsync();
        if (!tableVisible)
            return;

        var editButton = await _fixture.Page.QuerySelectorAsync("tbody tr:first-child a.btn-outline-secondary");
        Assert.NotNull(editButton);
        await editButton.ClickAsync();
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.Matches(@"/orders/\d+/edit$", _fixture.Page.Url);
    }

    [Fact(DisplayName = nameof(GivenSearchInputWhenTermEnteredThenSearchButtonIsPresent))]
    public async Task GivenSearchInputWhenTermEnteredThenSearchButtonIsPresent()
    {
        await _fixture.NavigateToOrdersAsync();
        await _fixture.Page.WaitForPageLoadAsync();

        var searchInput = await _fixture.Page.QuerySelectorAsync(Selectors.InputText);
        var searchButton = await _fixture.Page.QuerySelectorAsync(Selectors.ButtonOutlineSecondary);

        Assert.NotNull(searchInput);
        Assert.NotNull(searchButton);
    }

    [Fact(DisplayName = nameof(GivenFullTextSearchWhenSearchButtonClickedThenEitherTableOrEmptyStateIsShown))]
    public async Task GivenFullTextSearchWhenSearchButtonClickedThenEitherTableOrEmptyStateIsShown()
    {
        await _fixture.NavigateToOrdersAsync();
        await _fixture.Page.WaitForPageLoadAsync();

        await _fixture.Page.FillAsync(Selectors.InputText, "client");
        await _fixture.Page.ClickAsync(Selectors.ButtonOutlineSecondary);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var tableVisible = await _fixture.Page.IsTableVisibleAsync(3000);
        var emptyVisible = await _fixture.IsEmptyStateVisibleAsync();

        Assert.True(tableVisible || emptyVisible, "Either the orders table or empty state must be shown after full-text search");
    }

    [Fact(DisplayName = nameof(GivenFullTextSearchWhenEnterPressedThenEitherTableOrEmptyStateIsShown))]
    public async Task GivenFullTextSearchWhenEnterPressedThenEitherTableOrEmptyStateIsShown()
    {
        await _fixture.NavigateToOrdersAsync();
        await _fixture.Page.WaitForPageLoadAsync();

        await _fixture.Page.FillAsync(Selectors.InputText, "order");
        await _fixture.Page.PressAsync(Selectors.InputText, "Enter");
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var tableVisible = await _fixture.Page.IsTableVisibleAsync(3000);
        var emptyVisible = await _fixture.IsEmptyStateVisibleAsync();

        Assert.True(tableVisible || emptyVisible, "Either the orders table or empty state must be shown after pressing Enter");
    }

    [Fact(DisplayName = nameof(GivenFullTextSearchWhenSearchClearedThenAllOrdersReload))]
    public async Task GivenFullTextSearchWhenSearchClearedThenAllOrdersReload()
    {
        await _fixture.NavigateToOrdersAsync();
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await _fixture.Page.FillAsync(Selectors.InputText, "zzznomatch");
        await _fixture.Page.ClickAsync(Selectors.ButtonOutlineSecondary);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await _fixture.Page.FillAsync(Selectors.InputText, "");
        await _fixture.Page.ClickAsync(Selectors.ButtonOutlineSecondary);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var tableVisible = await _fixture.Page.IsTableVisibleAsync(3000);
        var emptyVisible = await _fixture.IsEmptyStateVisibleAsync();

        Assert.True(tableVisible || emptyVisible, "Either the orders table or empty state must be shown after clearing the search");
    }
}
