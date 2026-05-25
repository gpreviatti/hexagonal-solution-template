using Bunit;
using Core.Common.Requests;
using Core.Common.UseCases;
using Core.Orders;
using Microsoft.Extensions.DependencyInjection;
using WebUi.Pages.Orders;

namespace UnitTests.WebUi.Pages.Orders;

public sealed class OrderListFixture : BaseComponentFixture
{
    public Mock<IBaseInOutUseCase<BasePaginatedRequest, BasePaginatedResponse<OrderDto>>> MockGetAllOrders { get; } = new();
    public Mock<IBaseInOutUseCase<DeleteOrderRequest, BaseResponse>> MockDeleteOrder { get; } = new();

    public OrderListFixture()
    {
        Services.AddSingleton(MockGetAllOrders.Object);
        Services.AddSingleton(MockDeleteOrder.Object);
    }

    public void SetupGetAll(BasePaginatedResponse<OrderDto> response) =>
        MockGetAllOrders
            .Setup(u => u.HandleAsync(It.IsAny<BasePaginatedRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

    public static BasePaginatedResponse<OrderDto> SuccessPage(
        IEnumerable<OrderDto> orders,
        int totalPages = 1,
        int totalRecords = 0) =>
        new(true, totalPages, totalRecords == 0 ? orders.Count() : totalRecords, orders);

    public void ClearInvocations()
    {
        MockGetAllOrders.Invocations.Clear();
        MockDeleteOrder.Invocations.Clear();
    }
}

public sealed class OrderListTests : IClassFixture<OrderListFixture>
{
    private readonly OrderListFixture _fixture;

    public OrderListTests(OrderListFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearInvocations();
    }

    [Fact(DisplayName = nameof(GivenNoOrdersThenEmptyStateIsShown))]
    public void GivenNoOrdersThenEmptyStateIsShown()
    {
        _fixture.SetupGetAll(OrderListFixture.SuccessPage([]));

        var component = _fixture.Render<OrderList>();

        Assert.Contains("No orders found.", component.Markup);
    }

    [Fact(DisplayName = nameof(GivenOrdersReturnedThenTableRowsAreRendered))]
    public void GivenOrdersReturnedThenTableRowsAreRendered()
    {
        var orders = new[]
        {
            new OrderDto { Id = 1, Description = "First Order", Total = 100m, PeriodSinceWasCreated = "1 day ago" },
            new OrderDto { Id = 2, Description = "Second Order", Total = 200m, PeriodSinceWasCreated = "2 days ago" }
        };
        _fixture.SetupGetAll(OrderListFixture.SuccessPage(orders, totalRecords: 2));

        var component = _fixture.Render<OrderList>();

        var rows = component.FindAll(Selectors.TableRows);
        Assert.Equal(2, rows.Count);
        Assert.Contains("First Order", rows[0].TextContent);
        Assert.Contains("Second Order", rows[1].TextContent);
    }

    [Fact(DisplayName = nameof(GivenUseCaseFailsThenErrorAlertIsShown))]
    public void GivenUseCaseFailsThenErrorAlertIsShown()
    {
        _fixture.MockGetAllOrders
            .Setup(u => u.HandleAsync(It.IsAny<BasePaginatedRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BasePaginatedResponse<OrderDto>(false, 0, 0, null, "Load failed"));

        var component = _fixture.Render<OrderList>();

        Assert.Contains("Load failed", component.Markup);
    }

    [Fact(DisplayName = nameof(GivenSearchButtonClickThenReloadsFromPageOne))]
    public void GivenSearchButtonClickThenReloadsFromPageOne()
    {
        _fixture.SetupGetAll(OrderListFixture.SuccessPage([]));

        var component = _fixture.Render<OrderList>();

        component.Find(Selectors.InputText).Input("test");
        component.Find(Selectors.ButtonOutlineSecondary).Click();

        _fixture.MockGetAllOrders.Verify(u => u.HandleAsync(
            It.Is<BasePaginatedRequest>(r => r.Page == 1),
            It.IsAny<CancellationToken>()),
            Times.AtLeast(2));
    }

    [Fact(DisplayName = nameof(GivenFirstPageThenPreviousButtonIsDisabled))]
    public void GivenFirstPageThenPreviousButtonIsDisabled()
    {
        var orders = new[] { new OrderDto { Id = 1, Description = "X", Total = 1m, PeriodSinceWasCreated = "now" } };
        _fixture.SetupGetAll(OrderListFixture.SuccessPage(orders, totalPages: 3, totalRecords: 30));

        var component = _fixture.Render<OrderList>();

        var prevItem = component.Find(Selectors.FirstChildLi);
        Assert.Contains("disabled", prevItem.ClassName);
    }

    [Fact(DisplayName = nameof(GivenNextButtonClickThenNavigatesToNextPage))]
    public void GivenNextButtonClickThenNavigatesToNextPage()
    {
        var orders = new[] { new OrderDto { Id = 1, Description = "X", Total = 1m, PeriodSinceWasCreated = "now" } };
        _fixture.SetupGetAll(OrderListFixture.SuccessPage(orders, totalPages: 3, totalRecords: 30));

        var component = _fixture.Render<OrderList>();

        component.Find(Selectors.LastChildLi).Click();

        _fixture.MockGetAllOrders.Verify(u => u.HandleAsync(
            It.Is<BasePaginatedRequest>(r => r.Page == 2),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = nameof(GivenDeleteButtonClickThenModalShowsOrderId))]
    public void GivenDeleteButtonClickThenModalShowsOrderId()
    {
        var orders = new[] { new OrderDto { Id = 7, Description = "Order to delete", Total = 50m, PeriodSinceWasCreated = "now" } };
        _fixture.SetupGetAll(OrderListFixture.SuccessPage(orders, totalRecords: 1));

        var component = _fixture.Render<OrderList>();

        component.Find(Selectors.ButtonOutlineDanger).Click();

        Assert.Contains("#7", component.Find(Selectors.ModalBody).TextContent);
    }

    [Fact(DisplayName = nameof(GivenConfirmDeleteThenUseCaseIsCalledAndListReloads))]
    public void GivenConfirmDeleteThenUseCaseIsCalledAndListReloads()
    {
        var orders = new[] { new OrderDto { Id = 5, Description = "Delete me", Total = 10m, PeriodSinceWasCreated = "now" } };
        _fixture.SetupGetAll(OrderListFixture.SuccessPage(orders, totalRecords: 1));

        _fixture.MockDeleteOrder
            .Setup(u => u.HandleAsync(It.IsAny<DeleteOrderRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseResponse(true));

        var component = _fixture.Render<OrderList>();

        component.Find(Selectors.ButtonOutlineDanger).Click();
        component.Find(Selectors.ButtonDanger).Click();

        _fixture.MockDeleteOrder.Verify(u => u.HandleAsync(
            It.Is<DeleteOrderRequest>(r => r.OrderId == 5),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = nameof(GivenDeleteFailsThenErrorAlertIsShown))]
    public void GivenDeleteFailsThenErrorAlertIsShown()
    {
        var orders = new[] { new OrderDto { Id = 3, Description = "Fail", Total = 1m, PeriodSinceWasCreated = "now" } };
        _fixture.SetupGetAll(OrderListFixture.SuccessPage(orders, totalRecords: 1));

        _fixture.MockDeleteOrder
            .Setup(u => u.HandleAsync(It.IsAny<DeleteOrderRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseResponse(false, "Delete failed"));

        var component = _fixture.Render<OrderList>();

        component.Find(Selectors.ButtonOutlineDanger).Click();
        component.Find(Selectors.ButtonDanger).Click();

        Assert.Contains("Delete failed", component.Markup);
    }
}
