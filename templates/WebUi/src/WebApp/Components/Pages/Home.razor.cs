using System.Diagnostics;
using Contracts.Common;
using Contracts.Orders;
using Infrastructure.Common;
using Infrastructure.Http;

namespace WebApp.Components.Pages;

public partial class Home(IServiceProvider serviceProvider)
{
    private readonly IBaseHttpService _httpService = serviceProvider
        .GetRequiredKeyedService<IBaseHttpService>(ServicesKey.Orders.ToString());
    private readonly ILogger<Home> _logger = serviceProvider.GetRequiredService<ILogger<Home>>();
    private OrderSummaryDto? _summary;
    private IEnumerable<OrderDto>? _orders;
    private readonly string _resourceUrl = nameof(ServicesKey.Orders);

    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(
            GetOrderSummary(),
            GetOrders()
        );
    }

    private async Task GetOrders()
    {
        using var activity = DefaultConfigurations.ActivitySource.StartActivity(nameof(GetOrders), ActivityKind.Client);

        var response = await _httpService.SendAsync<BaseResponse<IEnumerable<OrderDto>>>(
            _resourceUrl,
            HttpMethod.Get,
            CancellationToken.None
        );

        if (response is null || !response.Success)
        {
            Logs.FailedOperation(_logger, "Failed to retrieve orders.");
            return;
        }

        _orders = response?.Data;
    }

    private async Task GetOrderSummary()
    {
        using var activity = DefaultConfigurations.ActivitySource.StartActivity(nameof(GetOrderSummary), ActivityKind.Client);

        var response = await _httpService.SendAsync<GetOrderSummaryResponse>(
            _resourceUrl + "/summary",
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
