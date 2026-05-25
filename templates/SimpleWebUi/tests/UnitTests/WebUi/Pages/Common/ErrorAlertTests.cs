using Bunit;
using Microsoft.AspNetCore.Components;
using WebUi.Pages.Common;

namespace UnitTests.WebUi.Pages.Common;

public sealed class ErrorAlertTests : BaseComponentFixture
{
    [Fact(DisplayName = nameof(GivenShowIsFalseThenAlertIsHidden))]
    public void GivenShowIsFalseThenAlertIsHidden()
    {
        var component = Render<ErrorAlert>(p => p
            .Add(c => c.Show, false)
            .Add(c => c.Message, "Some error"));

        Assert.Empty(component.FindAll(".alert"));
    }

    [Fact(DisplayName = nameof(GivenNullMessageThenAlertIsHidden))]
    public void GivenNullMessageThenAlertIsHidden()
    {
        var component = Render<ErrorAlert>(p => p
            .Add(c => c.Show, true)
            .Add(c => c.Message, null));

        Assert.Empty(component.FindAll(".alert"));
    }

    [Fact(DisplayName = nameof(GivenEmptyMessageThenAlertIsHidden))]
    public void GivenEmptyMessageThenAlertIsHidden()
    {
        var component = Render<ErrorAlert>(p => p
            .Add(c => c.Show, true)
            .Add(c => c.Message, string.Empty));

        Assert.Empty(component.FindAll(".alert"));
    }

    [Fact(DisplayName = nameof(GivenShowIsTrueAndMessageIsSetThenAlertIsRendered))]
    public void GivenShowIsTrueAndMessageIsSetThenAlertIsRendered()
    {
        var component = Render<ErrorAlert>(p => p
            .Add(c => c.Show, true)
            .Add(c => c.Message, "Something went wrong"));

        var alert = component.Find(".alert");
        Assert.Contains("Something went wrong", alert.TextContent);
    }

    [Fact(DisplayName = nameof(GivenDismissClickThenShowChangedIsFalse))]
    public void GivenDismissClickThenShowChangedIsFalse()
    {
        var showChangedValue = true;

        var component = Render<ErrorAlert>(p => p
            .Add(c => c.Show, true)
            .Add(c => c.Message, "Error!")
            .Add(c => c.ShowChanged, EventCallback.Factory.Create<bool>(this, v => showChangedValue = v)));

        component.Find(".btn-close").Click();

        Assert.False(showChangedValue);
    }

    [Fact(DisplayName = nameof(GivenDismissClickThenAlertIsHidden))]
    public void GivenDismissClickThenAlertIsHidden()
    {
        var component = Render<ErrorAlert>(p => p
            .Add(c => c.Show, true)
            .Add(c => c.Message, "Error!"));

        component.Find(".btn-close").Click();

        Assert.Empty(component.FindAll(".alert"));
    }
}
