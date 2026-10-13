using Bunit;
using Bunit.TestDoubles;
using Core.Common.Requests;
using Core.Common.UseCases;
using Core.Orders;
using Microsoft.Extensions.DependencyInjection;
using WebUi.Pages.Orders;

namespace UnitTests.WebUi.Pages.Orders;

public sealed class EditOrderFixture : BaseComponentFixture
{
    public Mock<IBaseInOutUseCase<GetOrderRequest, BaseResponse<OrderDto>>> MockGetOrder { get; } = new();
    public Mock<IBaseInOutUseCase<UpdateOrderRequest, BaseResponse<OrderDto>>> MockUpdateOrder { get; } = new();

    public EditOrderFixture()
    {
        Services.AddSingleton(MockGetOrder.Object);
        Services.AddSingleton(MockUpdateOrder.Object);
    }

    public static OrderDto SampleOrder(int id = 1) => new()
    {
        Id = id,
        Description = "Test Order",
        Total = 99.99m,
        PeriodSinceWasCreated = "2 days ago",
        Items =
        [
            new ItemDto { Id = 10, Name = "Widget", Description = "A widget", Value = 99.99m }
        ]
    };

    public void SetupGetOrderSuccess(OrderDto order) =>
        MockGetOrder
            .Setup(u => u.HandleAsync(It.IsAny<GetOrderRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseResponse<OrderDto>(true, order));

    public void ClearInvocations()
    {
        MockGetOrder.Invocations.Clear();
        MockUpdateOrder.Invocations.Clear();
    }
}

public sealed class EditOrderTests : IClassFixture<EditOrderFixture>
{
    private readonly EditOrderFixture _fixture;

    public EditOrderTests(EditOrderFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearInvocations();
    }

    [Fact(DisplayName = nameof(GivenExistingOrderThenFormIsPrePopulatedAsync))]
    public async Task GivenExistingOrderThenFormIsPrePopulatedAsync()
    {
        _fixture.SetupGetOrderSuccess(EditOrderFixture.SampleOrder(10));

        var component = _fixture.Render<EditOrder>(p => p.Add(c => c.Id, 10));

        Assert.Equal("Test Order", component.Find(Selectors.InputFormControl).GetAttribute("value"));
    }

    [Fact(DisplayName = nameof(GivenExistingOrderWithItemsThenItemsAreRenderedAsync))]
    public async Task GivenExistingOrderWithItemsThenItemsAreRenderedAsync()
    {
        _fixture.SetupGetOrderSuccess(EditOrderFixture.SampleOrder(1));

        var component = _fixture.Render<EditOrder>(p => p.Add(c => c.Id, 1));

        Assert.Contains("Widget", component.Markup, StringComparison.Ordinal);
        Assert.Contains("Item 1", component.Markup, StringComparison.Ordinal);
    }

    [Fact(DisplayName = nameof(GivenLoadFailsThenErrorAlertIsShownAsync))]
    public async Task GivenLoadFailsThenErrorAlertIsShownAsync()
    {
        _fixture.MockGetOrder
            .Setup(u => u.HandleAsync(It.IsAny<GetOrderRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseResponse<OrderDto>(false, null, "Order not found"));

        var component = _fixture.Render<EditOrder>(p => p.Add(c => c.Id, 99));

        Assert.Contains("Order not found", component.Markup, StringComparison.Ordinal);
    }

    [Fact(DisplayName = nameof(GivenOrderWithNoItemsThenEmptyRowIsAddedAsync))]
    public async Task GivenOrderWithNoItemsThenEmptyRowIsAddedAsync()
    {
        _fixture.SetupGetOrderSuccess(EditOrderFixture.SampleOrder() with { Items = [] });

        var component = _fixture.Render<EditOrder>(p => p.Add(c => c.Id, 1));

        Assert.Contains("Item 1", component.Markup, StringComparison.Ordinal);
    }

    [Fact(DisplayName = nameof(GivenAddItemClickThenNewRowAppearsAsync))]
    public async Task GivenAddItemClickThenNewRowAppearsAsync()
    {
        _fixture.SetupGetOrderSuccess(EditOrderFixture.SampleOrder(1));

        var component = _fixture.Render<EditOrder>(p => p.Add(c => c.Id, 1));

        await component.Find(Selectors.ButtonOutlineSuccess).ClickAsync();

        Assert.Contains("Item 2", component.Markup, StringComparison.Ordinal);
    }

    [Fact(DisplayName = nameof(GivenSuccessfulSubmitThenNavigatesToOrdersAsync))]
    public async Task GivenSuccessfulSubmitThenNavigatesToOrdersAsync()
    {
        _fixture.SetupGetOrderSuccess(EditOrderFixture.SampleOrder(3));

        _fixture.MockUpdateOrder
            .Setup(u => u.HandleAsync(It.IsAny<UpdateOrderRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseResponse<OrderDto>(true, EditOrderFixture.SampleOrder(3) with { Description = "Updated Order" }));

        var component = _fixture.Render<EditOrder>(p => p.Add(c => c.Id, 3));

        await component.Find(Selectors.ButtonSubmit).ClickAsync();

        await Task.Delay(50);

        var nav = _fixture.Services.GetRequiredService<BunitNavigationManager>();
        Assert.EndsWith("/orders", nav.Uri, StringComparison.Ordinal);
    }

    [Fact(DisplayName = nameof(GivenFailedSubmitThenErrorAlertIsShownAsync))]
    public async Task GivenFailedSubmitThenErrorAlertIsShownAsync()
    {
        _fixture.SetupGetOrderSuccess(EditOrderFixture.SampleOrder(1));

        _fixture.MockUpdateOrder
            .Setup(u => u.HandleAsync(It.IsAny<UpdateOrderRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseResponse<OrderDto>(false, null, "Update failed"));

        var component = _fixture.Render<EditOrder>(p => p.Add(c => c.Id, 1));

        await component.Find(Selectors.ButtonSubmit).ClickAsync();

        await Task.Delay(50);

        Assert.Contains("Update failed", component.Markup, StringComparison.Ordinal);
    }

    [Fact(DisplayName = nameof(GivenOneItemThenRemoveButtonIsHiddenAsync))]
    public async Task GivenOneItemThenRemoveButtonIsHiddenAsync()
    {
        _fixture.SetupGetOrderSuccess(EditOrderFixture.SampleOrder(1));

        var component = _fixture.Render<EditOrder>(p => p.Add(c => c.Id, 1));

        Assert.Empty(component.FindAll(Selectors.ButtonOutlineDanger));
    }

    [Fact(DisplayName = nameof(GivenRemoveItemClickThenRowIsRemovedAsync))]
    public async Task GivenRemoveItemClickThenRowIsRemovedAsync()
    {
        _fixture.SetupGetOrderSuccess(EditOrderFixture.SampleOrder(1));

        var component = _fixture.Render<EditOrder>(p => p.Add(c => c.Id, 1));

        await component.Find(Selectors.ButtonOutlineSuccess).ClickAsync();
        Assert.Contains("Item 2", component.Markup, StringComparison.Ordinal);

        await component.FindAll(Selectors.ButtonOutlineDanger)[0].ClickAsync();

        Assert.DoesNotContain("Item 2", component.Markup, StringComparison.Ordinal);
    }
}
