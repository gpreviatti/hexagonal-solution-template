using E2eTests.Common;

namespace E2eTests.WebUi;

public class OrderDetailPageFixture : BrowserFixture
{
    public async Task NavigateToFirstOrderAsync()
    {
        await Page.NavigateAsync($"{WebAppUrl}/orders");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var viewButton = await Page.QuerySelectorAsync("tbody tr:first-child a.btn-outline-primary");
        Assert.NotNull(viewButton);
        await viewButton.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
}

public sealed class OrderDetailPageTests(OrderDetailPageFixture fixture) : IClassFixture<OrderDetailPageFixture>
{
    private readonly OrderDetailPageFixture _fixture = fixture;

    [Fact(DisplayName = nameof(GivenOrderDetailPageWhenNavigatingWithValidIdThenHeadingContainsOrderIdAsync))]
    public async Task GivenOrderDetailPageWhenNavigatingWithValidIdThenHeadingContainsOrderIdAsync()
    {
        await _fixture.NavigateToFirstOrderAsync();
        await _fixture.Page.WaitForPageLoadAsync();

        var heading = await _fixture.Page.GetHeadingTextAsync();

        Assert.Contains("Order", heading, StringComparison.Ordinal);
        Assert.Contains("#", heading, StringComparison.Ordinal);
    }

    [Fact(DisplayName = nameof(GivenOrderDetailPageWhenLoadedThenOrderCardIsVisibleAsync))]
    public async Task GivenOrderDetailPageWhenLoadedThenOrderCardIsVisibleAsync()
    {
        await _fixture.NavigateToFirstOrderAsync();
        await _fixture.Page.WaitForPageLoadAsync();

        var cardVisible = await _fixture.Page.IsOrderCardVisibleAsync();

        Assert.True(cardVisible, "Order details card should be visible");
    }

    [Fact(DisplayName = nameof(GivenOrderDetailPageWhenLoadedThenDescriptionTotalAndCreatedArePresentAsync))]
    public async Task GivenOrderDetailPageWhenLoadedThenDescriptionTotalAndCreatedArePresentAsync()
    {
        await _fixture.NavigateToFirstOrderAsync();
        await _fixture.Page.WaitForPageLoadAsync();

        var bodyText = await _fixture.Page.TextContentAsync("body");

        Assert.Contains("Description", bodyText, StringComparison.Ordinal);
        Assert.Contains("Total", bodyText, StringComparison.Ordinal);
        Assert.Contains("Created", bodyText, StringComparison.Ordinal);
    }

    [Fact(DisplayName = nameof(GivenOrderDetailPageWhenLoadedThenEitherItemsTableOrNoItemsTextIsShownAsync))]
    public async Task GivenOrderDetailPageWhenLoadedThenEitherItemsTableOrNoItemsTextIsShownAsync()
    {
        await _fixture.NavigateToFirstOrderAsync();
        await _fixture.Page.WaitForPageLoadAsync();

        var hasRows = (await _fixture.Page.QuerySelectorAllAsync("table tbody tr")).Count > 0;
        var bodyContent = await _fixture.Page.TextContentAsync("body");
        var hasNoItemsText = bodyContent?.Contains("No items.") ?? false;

        Assert.True(hasRows || hasNoItemsText, "Either items table or 'No items.' text must be shown");
    }

    [Fact(DisplayName = nameof(GivenOrderDetailPageWhenBackButtonClickedThenNavigatesToOrderListAsync))]
    public async Task GivenOrderDetailPageWhenBackButtonClickedThenNavigatesToOrderListAsync()
    {
        await _fixture.NavigateToFirstOrderAsync();
        await _fixture.Page.WaitForPageLoadAsync();

        await _fixture.Page.ClickAndWaitForLoadAsync(Selectors.LinkButtonOutlineSecondary);

        Assert.EndsWith("/orders", _fixture.Page.Url, StringComparison.Ordinal);
    }

    [Fact(DisplayName = nameof(GivenOrderDetailPageWhenEditButtonClickedThenNavigatesToEditPageAsync))]
    public async Task GivenOrderDetailPageWhenEditButtonClickedThenNavigatesToEditPageAsync()
    {
        await _fixture.NavigateToFirstOrderAsync();
        await _fixture.Page.WaitForPageLoadAsync();

        await _fixture.Page.ClickAndWaitForLoadAsync(Selectors.ButtonOutlinePrimary);

        Assert.Matches(@"/orders/\d+/edit$", _fixture.Page.Url);
    }

    [Fact(DisplayName = nameof(GivenOrderDetailPageWhenNavigatingToInvalidIdThenErrorIsShownAsync))]
    public async Task GivenOrderDetailPageWhenNavigatingToInvalidIdThenErrorIsShownAsync()
    {
        await _fixture.Page.NavigateAsync($"{_fixture.WebAppUrl}/orders/99999");
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorVisible = await _fixture.Page.QuerySelectorAsync(Selectors.Alert) is not null;
        var cardVisible = await _fixture.Page.IsOrderCardVisibleAsync();

        Assert.True(errorVisible || !cardVisible, "An error should be shown for a non-existent order");
    }

    [Fact(DisplayName = nameof(GivenOrderDetailPageWhenLoadedThenDeleteButtonIsPresentAsync))]
    public async Task GivenOrderDetailPageWhenLoadedThenDeleteButtonIsPresentAsync()
    {
        await _fixture.NavigateToFirstOrderAsync();
        await _fixture.Page.WaitForPageLoadAsync();

        var deleteButton = await _fixture.Page.QuerySelectorAsync(Selectors.ButtonOutlineDanger);

        Assert.NotNull(deleteButton);
    }
}
