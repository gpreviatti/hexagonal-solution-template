using Microsoft.AspNetCore.Components;

namespace WebUi.Pages.Common;

public partial class ErrorAlert
{
    [Parameter] public string? Message { get; set; }
    [Parameter] public bool Show { get; set; }
    [Parameter] public EventCallback<bool> ShowChanged { get; set; }

    private async Task DismissAsync()
    {
        Show = false;
        await ShowChanged.InvokeAsync(false);
    }
}
