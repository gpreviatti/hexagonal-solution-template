using E2eTests.Common;

namespace E2eTests.WebUi;

public class EditOrderPageFixture : BrowserFixture
{
    public async Task NavigateToFirstOrderEditAsync()
    {
        await Page.NavigateAsync($"{WebAppUrl}/orders");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var editButton = await Page.QuerySelectorAsync("tbody tr:first-child a.btn-outline-secondary");
        Assert.NotNull(editButton);
        await editButton.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task<int> GetItemRowCountAsync() => (await Page.QuerySelectorAllAsync(Selectors.ItemRow)).Count;
}

public sealed class EditOrderPageTests(EditOrderPageFixture fixture) : IClassFixture<EditOrderPageFixture>
{
    private readonly EditOrderPageFixture _fixture = fixture;

    [Fact(DisplayName = nameof(GivenEditOrderPageWhenNavigatingThenHeadingContainsEditOrder))]
    public async Task GivenEditOrderPageWhenNavigatingThenHeadingContainsEditOrder()
    {
        await _fixture.NavigateToFirstOrderEditAsync();
        await _fixture.Page.WaitForPageLoadAsync();

        var heading = await _fixture.Page.GetHeadingTextAsync();

        Assert.Contains("Edit Order", heading);
    }

    [Fact(DisplayName = nameof(GivenEditOrderPageWhenLoadedThenDescriptionIsPrePopulated))]
    public async Task GivenEditOrderPageWhenLoadedThenDescriptionIsPrePopulated()
    {
        await _fixture.NavigateToFirstOrderEditAsync();
        await _fixture.Page.WaitForFormAsync();

        var description = (await _fixture.Page.InputValueAsync(Selectors.InputOrderDescription)).Trim();

        Assert.False(string.IsNullOrWhiteSpace(description), "Description field should be pre-populated");
    }

    [Fact(DisplayName = nameof(GivenEditOrderPageWhenLoadedThenItemRowsArePresent))]
    public async Task GivenEditOrderPageWhenLoadedThenItemRowsArePresent()
    {
        await _fixture.NavigateToFirstOrderEditAsync();
        await _fixture.Page.WaitForFormAsync();

        var count = await _fixture.GetItemRowCountAsync();

        Assert.True(count >= 1, "At least one item row should be present");
    }

    [Fact(DisplayName = nameof(GivenEditOrderPageWhenLoadedThenAddItemButtonIsPresent))]
    public async Task GivenEditOrderPageWhenLoadedThenAddItemButtonIsPresent()
    {
        await _fixture.NavigateToFirstOrderEditAsync();
        await _fixture.Page.WaitForFormAsync();

        var button = await _fixture.Page.QuerySelectorAsync("button.btn-outline-success");

        Assert.NotNull(button);
    }

    [Fact(DisplayName = nameof(GivenAddItemClickThenNewRowAppears))]
    public async Task GivenAddItemClickThenNewRowAppears()
    {
        await _fixture.NavigateToFirstOrderEditAsync();
        await _fixture.Page.WaitForFormAsync();

        var beforeCount = await _fixture.GetItemRowCountAsync();
        await _fixture.Page.ClickAsync(Selectors.ButtonOutlineSuccess);
        await _fixture.Page.WaitForFunctionAsync($"() => document.querySelectorAll('{Selectors.ItemRow}').length === {beforeCount + 1}");

        var afterCount = await _fixture.GetItemRowCountAsync();
        Assert.Equal(beforeCount + 1, afterCount);
    }

    [Fact(DisplayName = nameof(GivenValidFormWhenSubmittedThenNavigatesToOrdersList))]
    public async Task GivenValidFormWhenSubmittedThenNavigatesToOrdersList()
    {
        await _fixture.NavigateToFirstOrderEditAsync();
        await _fixture.Page.WaitForFormAsync();

        var locator = _fixture.Page.Locator(Selectors.InputOrderDescription);
        await locator.ClickAsync();
        await locator.FillAsync("Updated via E2E");
        await locator.PressAsync("Tab");
        await _fixture.Page.WaitForTimeoutAsync(300);

        await _fixture.Page.ClickSubmitAsync();
        await _fixture.Page.WaitForURLAsync("**/orders", new() { Timeout = 10000 });

        Assert.EndsWith("/orders", _fixture.Page.Url);
    }

    [Fact(DisplayName = nameof(GivenBackButtonClickThenNavigatesToOrderDetail))]
    public async Task GivenBackButtonClickThenNavigatesToOrderDetail()
    {
        await _fixture.NavigateToFirstOrderEditAsync();
        await _fixture.Page.WaitForFormAsync();

        await _fixture.Page.ClickAndWaitForLoadAsync(Selectors.LinkButtonOutlineSecondary);

        Assert.Matches(@"/orders/\d+$", _fixture.Page.Url);
    }

    [Fact(DisplayName = nameof(GivenEditOrderPageWhenNavigatingToInvalidIdThenErrorOrLoadingIsShown))]
    public async Task GivenEditOrderPageWhenNavigatingToInvalidIdThenErrorOrLoadingIsShown()
    {
        await _fixture.Page.NavigateAsync($"{_fixture.WebAppUrl}/orders/{int.MaxValue}/edit");
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorVisible = await _fixture.Page.QuerySelectorAsync(Selectors.Alert) is not null;
        var formVisible = await _fixture.Page.QuerySelectorAsync(Selectors.ButtonSubmit) is not null;

        Assert.True(errorVisible || !formVisible, "An error should be shown for a non-existent order");
    }

    [Fact(DisplayName = nameof(GivenOneItemWhenLoadedThenRemoveButtonIsHidden))]
    public async Task GivenOneItemWhenLoadedThenRemoveButtonIsHidden()
    {
        await _fixture.NavigateToFirstOrderEditAsync();
        await _fixture.Page.WaitForFormAsync();

        var count = await _fixture.GetItemRowCountAsync();
        if (count != 1)
            return;

        var isVisible = await _fixture.Page.QuerySelectorAsync(Selectors.ButtonOutlineDanger) is not null;
        Assert.False(isVisible, "Remove button should be hidden when only one item is present");
    }
}
