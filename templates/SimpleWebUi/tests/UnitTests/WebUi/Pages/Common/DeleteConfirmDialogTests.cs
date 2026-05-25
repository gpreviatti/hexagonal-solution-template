using Bunit;
using Microsoft.AspNetCore.Components;
using WebUi.Pages.Common;

namespace UnitTests.WebUi.Pages.Common;

public sealed class DeleteConfirmDialogTests : BaseComponentFixture
{
    [Fact(DisplayName = nameof(GivenInitialRenderThenDialogIsHidden))]
    public void GivenInitialRenderThenDialogIsHidden()
    {
        var component = Render<DeleteConfirmDialog>(p => p
            .Add(c => c.OrderId, 1));

        Assert.Empty(component.FindAll(".modal"));
    }

    [Fact(DisplayName = nameof(GivenShowCalledThenDialogIsVisible))]
    public void GivenShowCalledThenDialogIsVisible()
    {
        var component = Render<DeleteConfirmDialog>(p => p
            .Add(c => c.OrderId, 42));

        component.Instance.Show();
        component.Render();

        Assert.NotEmpty(component.FindAll(".modal"));
    }

    [Fact(DisplayName = nameof(GivenShowCalledThenOrderIdIsDisplayed))]
    public void GivenShowCalledThenOrderIdIsDisplayed()
    {
        var component = Render<DeleteConfirmDialog>(p => p
            .Add(c => c.OrderId, 99));

        component.Instance.Show();
        component.Render();

        Assert.Contains("#99", component.Find(".modal-body").TextContent);
    }

    [Fact(DisplayName = nameof(GivenCancelClickThenDialogIsHiddenAndCallbackFired))]
    public void GivenCancelClickThenDialogIsHiddenAndCallbackFired()
    {
        var cancelled = false;

        var component = Render<DeleteConfirmDialog>(p => p
            .Add(c => c.OrderId, 1)
            .Add(c => c.OnCancelled, EventCallback.Factory.Create(this, () => cancelled = true)));

        component.Instance.Show();
        component.Render();

        component.Find(".btn-secondary").Click();

        Assert.Empty(component.FindAll(".modal"));
        Assert.True(cancelled);
    }

    [Fact(DisplayName = nameof(GivenConfirmClickThenDialogIsHiddenAndCallbackFired))]
    public void GivenConfirmClickThenDialogIsHiddenAndCallbackFired()
    {
        var confirmed = false;

        var component = Render<DeleteConfirmDialog>(p => p
            .Add(c => c.OrderId, 1)
            .Add(c => c.OnConfirmed, EventCallback.Factory.Create(this, () => confirmed = true)));

        component.Instance.Show();
        component.Render();

        component.Find(".btn-danger").Click();

        Assert.Empty(component.FindAll(".modal"));
        Assert.True(confirmed);
    }
}
