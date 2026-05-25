using Bunit;
using Bunit.TestDoubles;
using Core.Common.Requests;
using Core.Common.UseCases;
using Core.Orders;
using Microsoft.Extensions.DependencyInjection;
using WebUi.Pages.Common;
using WebUi.Pages.Orders;

namespace UnitTests.WebUi.Pages.Orders;

public sealed class OrderDetailFixture : BaseComponentFixture
{
    public Mock<IBaseInOutUseCase<GetOrderRequest, BaseResponse<OrderDto>>> MockGetOrder { get; } = new();
    public Mock<IBaseInOutUseCase<DeleteOrderRequest, BaseResponse>> MockDeleteOrder { get; } = new();

    public OrderDetailFixture()
    {
        Services.AddSingleton(MockGetOrder.Object);
        Services.AddSingleton(MockDeleteOrder.Object);
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
        MockDeleteOrder.Invocations.Clear();
    }
}

public sealed class OrderDetailTests : IClassFixture<OrderDetailFixture>
{
    private readonly OrderDetailFixture _fixture;

    public OrderDetailTests(OrderDetailFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearInvocations();
    }

    [Fact(DisplayName = nameof(GivenSuccessfulLoadThenOrderDetailsAreRendered))]
    public void GivenSuccessfulLoadThenOrderDetailsAreRendered()
    {
        _fixture.SetupGetOrderSuccess(OrderDetailFixture.SampleOrder(42));

        var component = _fixture.Render<OrderDetail>(p => p.Add(c => c.Id, 42));

        Assert.Contains("Test Order", component.Markup);
        Assert.Contains("2 days ago", component.Markup);
    }

    [Fact(DisplayName = nameof(GivenOrderWithItemsThenItemsTableIsRendered))]
    public void GivenOrderWithItemsThenItemsTableIsRendered()
    {
        _fixture.SetupGetOrderSuccess(OrderDetailFixture.SampleOrder());

        var component = _fixture.Render<OrderDetail>(p => p.Add(c => c.Id, 1));

        Assert.Contains("Widget", component.Markup);
        Assert.Contains("A widget", component.Markup);
    }

    [Fact(DisplayName = nameof(GivenOrderWithNoItemsThenNoItemsTextIsShown))]
    public void GivenOrderWithNoItemsThenNoItemsTextIsShown()
    {
        _fixture.SetupGetOrderSuccess(OrderDetailFixture.SampleOrder() with { Items = [] });

        var component = _fixture.Render<OrderDetail>(p => p.Add(c => c.Id, 1));

        Assert.Contains("No items.", component.Markup);
    }

    [Fact(DisplayName = nameof(GivenLoadFailsThenErrorAlertIsShown))]
    public void GivenLoadFailsThenErrorAlertIsShown()
    {
        _fixture.MockGetOrder
            .Setup(u => u.HandleAsync(It.IsAny<GetOrderRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseResponse<OrderDto>(false, null, "Not found"));

        var component = _fixture.Render<OrderDetail>(p => p.Add(c => c.Id, 99));

        Assert.Contains("Not found", component.Markup);
    }

    [Fact(DisplayName = nameof(GivenConfirmDeleteThenNavigatesToOrders))]
    public void GivenConfirmDeleteThenNavigatesToOrders()
    {
        _fixture.SetupGetOrderSuccess(OrderDetailFixture.SampleOrder(1));
        _fixture.MockDeleteOrder
            .Setup(u => u.HandleAsync(It.IsAny<DeleteOrderRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseResponse(true));

        var component = _fixture.Render<OrderDetail>(p => p.Add(c => c.Id, 1));

        var dialog = component.FindComponent<DeleteConfirmDialog>();
        dialog.Instance.Show();
        dialog.Render();
        dialog.Find(Selectors.ButtonDanger).Click();

        var nav = _fixture.Services.GetRequiredService<BunitNavigationManager>();
        Assert.EndsWith("/orders", nav.Uri);
    }

    [Fact(DisplayName = nameof(GivenDeleteFailsThenErrorAlertIsShown))]
    public void GivenDeleteFailsThenErrorAlertIsShown()
    {
        _fixture.SetupGetOrderSuccess(OrderDetailFixture.SampleOrder(1));
        _fixture.MockDeleteOrder
            .Setup(u => u.HandleAsync(It.IsAny<DeleteOrderRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseResponse(false, "Cannot delete"));

        var component = _fixture.Render<OrderDetail>(p => p.Add(c => c.Id, 1));

        var dialog = component.FindComponent<DeleteConfirmDialog>();
        dialog.Instance.Show();
        dialog.Render();
        dialog.Find(Selectors.ButtonDanger).Click();

        Assert.Contains("Cannot delete", component.Markup);
    }
}
