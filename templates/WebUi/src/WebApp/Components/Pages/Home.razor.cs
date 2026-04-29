using Contracts.Orders;
using Infrastructure.Common;
using Infrastructure.Http;

namespace WebApp.Components.Pages;

public partial class Home(IServiceProvider serviceProvider)
{
    private readonly IBaseHttpService _httpService = serviceProvider.GetRequiredService<IBaseHttpService>();
    private readonly ILogger<Home> _logger = serviceProvider.GetRequiredService<ILogger<Home>>();
    private OrderSummaryDto? _summary;
    private readonly string RESOURCE_URL = "orders/summary";

    protected override async Task OnInitializedAsync()
    {
        await GetOrderSummary();
    }

    private async Task GetOrderSummary()
    {
        var response = await _httpService.SendAsync<GetOrderSummaryResponse>(
            RESOURCE_URL,
            HttpMethod.Get,
            CancellationToken.None
        );

        if (response is null || !response.Success)
        {
            Logs.FailedOperation(_logger, "Failed to retrieve order summary.");
            return;
        }

        _summary = response?.Data;
    }
}
