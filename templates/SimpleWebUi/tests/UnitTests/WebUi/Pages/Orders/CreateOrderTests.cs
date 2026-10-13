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

    [Fact(DisplayName = nameof(GivenInitialRenderThenOneItemRowIsPresentAsync))]
    public async Task GivenInitialRenderThenOneItemRowIsPresentAsync()
    {
        var component = _fixture.Render<CreateOrder>();

        Assert.Contains("Item 1", component.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Item 2", component.Markup, StringComparison.Ordinal);
    }

    [Fact(DisplayName = nameof(GivenAddItemClickThenSecondItemRowAppearsAsync))]
    public async Task GivenAddItemClickThenSecondItemRowAppearsAsync()
    {
        var component = _fixture.Render<CreateOrder>();

        await component.Find(Selectors.ButtonOutlineSuccess).ClickAsync();

        Assert.Contains("Item 2", component.Markup, StringComparison.Ordinal);
    }

    [Fact(DisplayName = nameof(GivenOneItemThenRemoveButtonIsHiddenAsync))]
    public async Task GivenOneItemThenRemoveButtonIsHiddenAsync()
    {
        var component = _fixture.Render<CreateOrder>();

        Assert.Empty(component.FindAll(Selectors.ButtonOutlineDanger));
    }

    [Fact(DisplayName = nameof(GivenRemoveItemClickThenRowIsRemovedAsync))]
    public async Task GivenRemoveItemClickThenRowIsRemovedAsync()
    {
        var component = _fixture.Render<CreateOrder>();

        await component.Find(Selectors.ButtonOutlineSuccess).ClickAsync();
        Assert.Contains("Item 2", component.Markup, StringComparison.Ordinal);

        await component.FindAll(Selectors.ButtonOutlineDanger)[0].ClickAsync();

        Assert.DoesNotContain("Item 2", component.Markup, StringComparison.Ordinal);
    }

    [Fact(DisplayName = nameof(GivenSuccessfulSubmitThenNavigatesToOrdersAsync))]
    public async Task GivenSuccessfulSubmitThenNavigatesToOrdersAsync()
    {
        _fixture.MockCreateOrder
            .Setup(u => u.HandleAsync(It.IsAny<CreateOrderRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseResponse<OrderDto>(true, new OrderDto { Id = 55, Description = "New", Total = 10m, PeriodSinceWasCreated = "now" }));

        var component = _fixture.Render<CreateOrder>();

        await component.Find(Selectors.InputFormControl).ChangeAsync("My new order");
        await component.Find(Selectors.ButtonSubmit).ClickAsync();

        await Task.Delay(50);

        var nav = _fixture.Services.GetRequiredService<BunitNavigationManager>();
        Assert.EndsWith("/orders", nav.Uri, StringComparison.Ordinal);
    }

    [Fact(DisplayName = nameof(GivenFailedSubmitThenErrorAlertIsShownAsync))]
    public async Task GivenFailedSubmitThenErrorAlertIsShownAsync()
    {
        _fixture.MockCreateOrder
            .Setup(u => u.HandleAsync(It.IsAny<CreateOrderRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseResponse<OrderDto>(false, null, "Creation failed"));

        var component = _fixture.Render<CreateOrder>();

        await component.Find(Selectors.InputFormControl).ChangeAsync("My order");
        await component.Find(Selectors.ButtonSubmit).ClickAsync();

        await Task.Delay(50);

        Assert.Contains("Creation failed", component.Markup, StringComparison.Ordinal);
    }

    [Fact(DisplayName = nameof(GivenSubmitInProgressThenButtonIsDisabledAsync))]
    public async Task GivenSubmitInProgressThenButtonIsDisabledAsync()
    {
        var tcs = new TaskCompletionSource<BaseResponse<OrderDto>>();
        _fixture.MockCreateOrder
            .Setup(u => u.HandleAsync(It.IsAny<CreateOrderRequest>(), It.IsAny<CancellationToken>()))
            .Returns(tcs.Task);

        var component = _fixture.Render<CreateOrder>();

        await component.Find(Selectors.InputFormControl).ChangeAsync("My order");
        await component.Find(Selectors.ButtonSubmit).ClickAsync();

        Assert.True(component.Find(Selectors.ButtonSubmit).HasAttribute("disabled"));

        tcs.SetResult(new BaseResponse<OrderDto>(true, new OrderDto { Id = 1, Description = "x", Total = 1, PeriodSinceWasCreated = "now" }));
    }
}
