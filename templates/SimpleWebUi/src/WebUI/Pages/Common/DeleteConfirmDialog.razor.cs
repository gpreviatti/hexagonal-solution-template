using Microsoft.AspNetCore.Components;

namespace WebUi.Pages.Common;

public partial class DeleteConfirmDialog
{
    [Parameter] public int OrderId { get; set; }
    [Parameter] public EventCallback OnConfirmed { get; set; }
    [Parameter] public EventCallback OnCancelled { get; set; }

    private bool _visible;

    public void Show() => _visible = true;

    private async Task CancelAsync()
    {
        _visible = false;
        await OnCancelled.InvokeAsync();
    }

    private async Task ConfirmAsync()
    {
        _visible = false;
        await OnConfirmed.InvokeAsync();
    }
}
