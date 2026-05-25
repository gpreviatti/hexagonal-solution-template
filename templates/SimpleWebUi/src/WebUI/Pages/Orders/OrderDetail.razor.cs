using Core.Common.Requests;
using Core.Common.UseCases;
using Core.Orders;
using Microsoft.AspNetCore.Components;
using WebUi.Pages.Common;

namespace WebUi.Pages.Orders;

public partial class OrderDetail : IDisposable
{
    [Parameter] public int Id { get; set; }

    [Inject] private IBaseInOutUseCase<GetOrderRequest, BaseResponse<OrderDto>> GetOrder { get; set; } = default!;
    [Inject] private IBaseInOutUseCase<DeleteOrderRequest, BaseResponse> DeleteOrder { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private OrderDto? _order;
    private bool _loading;
    private bool _showError;
    private string? _errorMessage;
    private DeleteConfirmDialog _dialog = default!;

    protected override async Task OnParametersSetAsync()
    {
        _loading = true;
        _showError = false;

        var result = await GetOrder.HandleAsync(
            new(Guid.NewGuid(), Id),
            _cancellationTokenSource.Token
        );

        _loading = false;

        if (!result.Success)
        {
            _errorMessage = result.Message;
            _showError = true;
            return;
        }

        _order = result.Data;
    }

    private async Task ConfirmDeleteAsync()
    {
        var result = await DeleteOrder.HandleAsync(
            new(Guid.NewGuid(), Id),
            _cancellationTokenSource.Token
        );

        if (!result.Success)
        {
            _errorMessage = result.Message;
            _showError = true;
            return;
        }

        Nav.NavigateTo("/orders");
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        GC.SuppressFinalize(this);
    }
}
