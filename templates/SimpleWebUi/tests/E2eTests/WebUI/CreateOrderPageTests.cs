using E2eTests.Common;

namespace E2eTests.WebUi;

public class CreateOrderPageFixture : BrowserFixture
{
    public async Task NavigateToCreateOrderAsync() => await Page.NavigateAsync($"{WebAppUrl}/orders/create");
    public async Task WaitForPageLoadAsync() => await Page.WaitForSelectorAsync("h1", new() { Timeout = WaitForSelectorTimeoutMs });
    public async Task<int> GetItemRowCountAsync() => (await Page.QuerySelectorAllAsync(Selectors.ItemRow)).Count;
}

public sealed class CreateOrderPageTests(CreateOrderPageFixture fixture) : IClassFixture<CreateOrderPageFixture>
{
    private readonly CreateOrderPageFixture _fixture = fixture;

    [Fact(DisplayName = nameof(GivenCreateOrderPageWhenNavigatingThenHeadingIsNewOrder))]
    public async Task GivenCreateOrderPageWhenNavigatingThenHeadingIsNewOrder()
    {
        await _fixture.NavigateToCreateOrderAsync();
        await _fixture.WaitForPageLoadAsync();

        var heading = await _fixture.Page.GetHeadingTextAsync();

        Assert.Equal("New Order", heading);
    }

    [Fact(DisplayName = nameof(GivenCreateOrderPageWhenLoadedThenDescriptionInputIsPresent))]
    public async Task GivenCreateOrderPageWhenLoadedThenDescriptionInputIsPresent()
    {
        await _fixture.NavigateToCreateOrderAsync();
        await _fixture.WaitForPageLoadAsync();

        var input = await _fixture.Page.QuerySelectorAsync("input.form-control[placeholder='Order description']");

        Assert.NotNull(input);
    }

    [Fact(DisplayName = nameof(GivenCreateOrderPageWhenLoadedThenOneItemRowIsPresent))]
    public async Task GivenCreateOrderPageWhenLoadedThenOneItemRowIsPresent()
    {
        await _fixture.NavigateToCreateOrderAsync();
        await _fixture.WaitForPageLoadAsync();

        var count = await _fixture.GetItemRowCountAsync();

        Assert.Equal(1, count);
    }

    [Fact(DisplayName = nameof(GivenCreateOrderPageWhenLoadedThenRemoveButtonIsNotVisible))]
    public async Task GivenCreateOrderPageWhenLoadedThenRemoveButtonIsNotVisible()
    {
        await _fixture.NavigateToCreateOrderAsync();
        await _fixture.WaitForPageLoadAsync();

        var isVisible = await _fixture.Page.QuerySelectorAsync(Selectors.ButtonOutlineDanger) is not null;

        Assert.False(isVisible, "Remove button should not be visible with only one item");
    }

    [Fact(DisplayName = nameof(GivenAddItemClickThenSecondItemRowAppears))]
    public async Task GivenAddItemClickThenSecondItemRowAppears()
    {
        await _fixture.NavigateToCreateOrderAsync();
        await _fixture.WaitForPageLoadAsync();

        await _fixture.Page.ClickAsync(Selectors.ButtonOutlineSuccess);
        await _fixture.Page.WaitForFunctionAsync($"() => document.querySelectorAll('{Selectors.ItemRow}').length === 2");

        var count = await _fixture.GetItemRowCountAsync();
        Assert.Equal(2, count);
    }

    [Fact(DisplayName = nameof(GivenTwoItemsWhenRemoveClickedThenOneItemRemains))]
    public async Task GivenTwoItemsWhenRemoveClickedThenOneItemRemains()
    {
        await _fixture.NavigateToCreateOrderAsync();
        await _fixture.WaitForPageLoadAsync();

        await _fixture.Page.ClickAsync(Selectors.ButtonOutlineSuccess);
        await _fixture.Page.WaitForFunctionAsync($"() => document.querySelectorAll('{Selectors.ItemRow}').length === 2");

        await _fixture.Page.ClickAsync(Selectors.ButtonOutlineDanger);
        await _fixture.Page.WaitForFunctionAsync($"() => document.querySelectorAll('{Selectors.ItemRow}').length === 1");

        var count = await _fixture.GetItemRowCountAsync();
        Assert.Equal(1, count);
    }

    [Fact(DisplayName = nameof(GivenValidFormWhenSubmittedThenNavigatesToOrdersList))]
    public async Task GivenValidFormWhenSubmittedThenNavigatesToOrdersList()
    {
        await _fixture.NavigateToCreateOrderAsync();
        await _fixture.WaitForPageLoadAsync();

        var descriptionLocator = _fixture.Page.Locator(Selectors.InputOrderDescription);
        await descriptionLocator.ClickAsync();
        await descriptionLocator.FillAsync("E2E Test Order");
        await descriptionLocator.PressAsync("Tab");
        await _fixture.Page.WaitForTimeoutAsync(300);

        await _fixture.Page.FillAsync(Selectors.ItemNameInput, "Test Item");
        await _fixture.Page.FillAsync(Selectors.InputNumber, "9.99");

        await _fixture.Page.ClickSubmitAsync();
        await _fixture.Page.WaitForURLAsync("**/orders", new() { Timeout = 10000 });

        Assert.EndsWith("/orders", _fixture.Page.Url);
    }

    [Fact(DisplayName = nameof(GivenBackButtonClickThenNavigatesToOrdersList))]
    public async Task GivenBackButtonClickThenNavigatesToOrdersList()
    {
        await _fixture.NavigateToCreateOrderAsync();
        await _fixture.WaitForPageLoadAsync();

        await _fixture.Page.ClickAndWaitForLoadAsync(Selectors.LinkButtonOutlineSecondary);

        Assert.EndsWith("/orders", _fixture.Page.Url);
    }

    [Fact(DisplayName = nameof(GivenCreateOrderPageWhenLoadedThenSubmitButtonIsPresentAndEnabled))]
    public async Task GivenCreateOrderPageWhenLoadedThenSubmitButtonIsPresentAndEnabled()
    {
        await _fixture.NavigateToCreateOrderAsync();
        await _fixture.WaitForPageLoadAsync();

        var button = await _fixture.Page.QuerySelectorAsync("button[type='submit']");
        Assert.NotNull(button);

        var disabled = await button.GetAttributeAsync("disabled");
        Assert.False(disabled is not null, "Submit button should be enabled initially");
    }
}
