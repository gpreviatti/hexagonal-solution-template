using Contracts.Common;
using Contracts.Orders;
using Infrastructure.Common;
using Infrastructure.Http;
using Microsoft.AspNetCore.Components;

namespace WebApp.Components.Pages;

public partial class Home(IServiceProvider serviceProvider) : ComponentBase
{
    private readonly IBaseHttpService _ordersHttpService = serviceProvider
        .GetRequiredKeyedService<IBaseHttpService>(ServicesKey.Orders.ToString());
    private readonly ILogger<Home> _logger = serviceProvider.GetRequiredService<ILogger<Home>>();
    private OrderSummaryDto? _summary;
    private IEnumerable<OrderDto>? _orders;
    private OrderDto? _selectedOrder;
    private IReadOnlyCollection<ItemDto>? _selectedOrderItems;
    private bool _showOrderItems;
    private readonly string _resourceUrl = nameof(ServicesKey.Orders);

    private bool HasSelectedOrderItems => _selectedOrderItems is { Count: > 0 };

    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(
            GetOrderSummary(),
            GetOrders()
        );
    }

    private async Task GetOrders()
    {
        using var activity = DefaultConfigurations.ActivitySource.StartActivity(nameof(GetOrders));

        var response = await _ordersHttpService.SendAsync<BaseResponse<IEnumerable<OrderDto>>>(
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
        using var activity = DefaultConfigurations.ActivitySource.StartActivity(nameof(GetOrderSummary));

        var response = await _ordersHttpService.SendAsync<GetOrderSummaryResponse>(
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

    private void ToggleOrderItems(OrderDto order)
    {
        if (_selectedOrder?.Id == order.Id && _showOrderItems)
        {
            _showOrderItems = false;
            _selectedOrder = null;
            _selectedOrderItems = null;
            return;
        }

        _selectedOrder = order;
        _selectedOrderItems = order.Items ?? [];
        _showOrderItems = true;
    }

    private bool IsOrderSelected(OrderDto order) => _showOrderItems && _selectedOrder?.Id == order.Id;
}
