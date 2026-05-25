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

    [Fact(DisplayName = nameof(GivenOrderDetailPageWhenNavigatingWithValidIdThenHeadingContainsOrderId))]
    public async Task GivenOrderDetailPageWhenNavigatingWithValidIdThenHeadingContainsOrderId()
    {
        await _fixture.NavigateToFirstOrderAsync();
        await _fixture.Page.WaitForPageLoadAsync();

        var heading = await _fixture.Page.GetHeadingTextAsync();

        Assert.Contains("Order", heading);
        Assert.Contains("#", heading);
    }

    [Fact(DisplayName = nameof(GivenOrderDetailPageWhenLoadedThenOrderCardIsVisible))]
    public async Task GivenOrderDetailPageWhenLoadedThenOrderCardIsVisible()
    {
        await _fixture.NavigateToFirstOrderAsync();
        await _fixture.Page.WaitForPageLoadAsync();

        var cardVisible = await _fixture.Page.IsOrderCardVisibleAsync();

        Assert.True(cardVisible, "Order details card should be visible");
    }

    [Fact(DisplayName = nameof(GivenOrderDetailPageWhenLoadedThenDescriptionTotalAndCreatedArePresent))]
    public async Task GivenOrderDetailPageWhenLoadedThenDescriptionTotalAndCreatedArePresent()
    {
        await _fixture.NavigateToFirstOrderAsync();
        await _fixture.Page.WaitForPageLoadAsync();

        var bodyText = await _fixture.Page.TextContentAsync("body");

        Assert.Contains("Description", bodyText);
        Assert.Contains("Total", bodyText);
        Assert.Contains("Created", bodyText);
    }

    [Fact(DisplayName = nameof(GivenOrderDetailPageWhenLoadedThenEitherItemsTableOrNoItemsTextIsShown))]
    public async Task GivenOrderDetailPageWhenLoadedThenEitherItemsTableOrNoItemsTextIsShown()
    {
        await _fixture.NavigateToFirstOrderAsync();
        await _fixture.Page.WaitForPageLoadAsync();

        var hasRows = (await _fixture.Page.QuerySelectorAllAsync("table tbody tr")).Count > 0;
        var bodyContent = await _fixture.Page.TextContentAsync("body");
        var hasNoItemsText = bodyContent?.Contains("No items.") ?? false;

        Assert.True(hasRows || hasNoItemsText, "Either items table or 'No items.' text must be shown");
    }

    [Fact(DisplayName = nameof(GivenOrderDetailPageWhenBackButtonClickedThenNavigatesToOrderList))]
    public async Task GivenOrderDetailPageWhenBackButtonClickedThenNavigatesToOrderList()
    {
        await _fixture.NavigateToFirstOrderAsync();
        await _fixture.Page.WaitForPageLoadAsync();

        await _fixture.Page.ClickAndWaitForLoadAsync(Selectors.LinkButtonOutlineSecondary);

        Assert.EndsWith("/orders", _fixture.Page.Url);
    }

    [Fact(DisplayName = nameof(GivenOrderDetailPageWhenEditButtonClickedThenNavigatesToEditPage))]
    public async Task GivenOrderDetailPageWhenEditButtonClickedThenNavigatesToEditPage()
    {
        await _fixture.NavigateToFirstOrderAsync();
        await _fixture.Page.WaitForPageLoadAsync();

        await _fixture.Page.ClickAndWaitForLoadAsync(Selectors.ButtonOutlinePrimary);

        Assert.Matches(@"/orders/\d+/edit$", _fixture.Page.Url);
    }

    [Fact(DisplayName = nameof(GivenOrderDetailPageWhenNavigatingToInvalidIdThenErrorIsShown))]
    public async Task GivenOrderDetailPageWhenNavigatingToInvalidIdThenErrorIsShown()
    {
        await _fixture.Page.NavigateAsync($"{_fixture.WebAppUrl}/orders/99999");
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorVisible = await _fixture.Page.QuerySelectorAsync(Selectors.Alert) is not null;
        var cardVisible = await _fixture.Page.IsOrderCardVisibleAsync();

        Assert.True(errorVisible || !cardVisible, "An error should be shown for a non-existent order");
    }

    [Fact(DisplayName = nameof(GivenOrderDetailPageWhenLoadedThenDeleteButtonIsPresent))]
    public async Task GivenOrderDetailPageWhenLoadedThenDeleteButtonIsPresent()
    {
        await _fixture.NavigateToFirstOrderAsync();
        await _fixture.Page.WaitForPageLoadAsync();

        var deleteButton = await _fixture.Page.QuerySelectorAsync(Selectors.ButtonOutlineDanger);

        Assert.NotNull(deleteButton);
    }
}
