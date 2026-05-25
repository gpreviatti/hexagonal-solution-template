using Microsoft.AspNetCore.Components;

namespace WebUi.Pages.Common;

public partial class DeleteConfirmDialog
{
    [Parameter] public int OrderId { get; set; }
    [Parameter] public EventCallback OnConfirmed { get; set; }
    [Parameter] public EventCallback OnCancelled { get; set; }

    private bool _visible;

    public void Show() => _visible = true;

    private void Cancel()
    {
        _visible = false;
        OnCancelled.InvokeAsync();
    }

    private async Task Confirm()
    {
        _visible = false;
        await OnConfirmed.InvokeAsync();
    }
}
