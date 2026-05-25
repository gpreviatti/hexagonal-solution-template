using Bunit;
using Bunit.TestDoubles;
using Core.Common.Requests;
using Core.Common.UseCases;
using Core.Orders;
using Microsoft.Extensions.DependencyInjection;
using WebUi.Pages.Orders;

namespace UnitTests.WebUi.Pages.Orders;

public sealed class CreateOrderFixture : BaseComponentFixture
{
    public Mock<IBaseInOutUseCase<CreateOrderRequest, BaseResponse<OrderDto>>> MockCreateOrder { get; } = new();

    public CreateOrderFixture() => Services.AddSingleton(MockCreateOrder.Object);

    public void ClearInvocations() => MockCreateOrder.Invocations.Clear();
}

public sealed class CreateOrderTests : IClassFixture<CreateOrderFixture>
{
    private readonly CreateOrderFixture _fixture;

    public CreateOrderTests(CreateOrderFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearInvocations();
    }

    [Fact(DisplayName = nameof(GivenInitialRenderThenOneItemRowIsPresent))]
    public void GivenInitialRenderThenOneItemRowIsPresent()
    {
        var component = _fixture.Render<CreateOrder>();

        Assert.Contains("Item 1", component.Markup);
        Assert.DoesNotContain("Item 2", component.Markup);
    }

    [Fact(DisplayName = nameof(GivenAddItemClickThenSecondItemRowAppears))]
    public void GivenAddItemClickThenSecondItemRowAppears()
    {
        var component = _fixture.Render<CreateOrder>();

        component.Find(Selectors.ButtonOutlineSuccess).Click();

        Assert.Contains("Item 2", component.Markup);
    }

    [Fact(DisplayName = nameof(GivenOneItemThenRemoveButtonIsHidden))]
    public void GivenOneItemThenRemoveButtonIsHidden()
    {
        var component = _fixture.Render<CreateOrder>();

        Assert.Empty(component.FindAll(Selectors.ButtonOutlineDanger));
    }

    [Fact(DisplayName = nameof(GivenRemoveItemClickThenRowIsRemoved))]
    public void GivenRemoveItemClickThenRowIsRemoved()
    {
        var component = _fixture.Render<CreateOrder>();

        component.Find(Selectors.ButtonOutlineSuccess).Click();
        Assert.Contains("Item 2", component.Markup);

        component.FindAll(Selectors.ButtonOutlineDanger)[0].Click();

        Assert.DoesNotContain("Item 2", component.Markup);
    }

    [Fact(DisplayName = nameof(GivenSuccessfulSubmitThenNavigatesToOrders))]
    public async Task GivenSuccessfulSubmitThenNavigatesToOrders()
    {
        _fixture.MockCreateOrder
            .Setup(u => u.HandleAsync(It.IsAny<CreateOrderRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseResponse<OrderDto>(true, new OrderDto { Id = 55, Description = "New", Total = 10m, PeriodSinceWasCreated = "now" }));

        var component = _fixture.Render<CreateOrder>();

        component.Find(Selectors.InputFormControl).Change("My new order");
        component.Find(Selectors.ButtonSubmit).Click();

        await Task.Delay(50);

        var nav = _fixture.Services.GetRequiredService<BunitNavigationManager>();
        Assert.EndsWith("/orders", nav.Uri);
    }

    [Fact(DisplayName = nameof(GivenFailedSubmitThenErrorAlertIsShown))]
    public async Task GivenFailedSubmitThenErrorAlertIsShown()
    {
        _fixture.MockCreateOrder
            .Setup(u => u.HandleAsync(It.IsAny<CreateOrderRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseResponse<OrderDto>(false, null, "Creation failed"));

        var component = _fixture.Render<CreateOrder>();

        component.Find(Selectors.InputFormControl).Change("My order");
        component.Find(Selectors.ButtonSubmit).Click();

        await Task.Delay(50);

        Assert.Contains("Creation failed", component.Markup);
    }

    [Fact(DisplayName = nameof(GivenSubmitInProgressThenButtonIsDisabled))]
    public void GivenSubmitInProgressThenButtonIsDisabled()
    {
        var tcs = new TaskCompletionSource<BaseResponse<OrderDto>>();
        _fixture.MockCreateOrder
            .Setup(u => u.HandleAsync(It.IsAny<CreateOrderRequest>(), It.IsAny<CancellationToken>()))
            .Returns(tcs.Task);

        var component = _fixture.Render<CreateOrder>();

        component.Find(Selectors.InputFormControl).Change("My order");
        component.Find(Selectors.ButtonSubmit).Click();

        Assert.True(component.Find(Selectors.ButtonSubmit).HasAttribute("disabled"));

        tcs.SetResult(new BaseResponse<OrderDto>(true, new OrderDto { Id = 1, Description = "x", Total = 1, PeriodSinceWasCreated = "now" }));
    }
}
