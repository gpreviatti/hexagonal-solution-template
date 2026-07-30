using Core.Common.Requests;
using Core.Common.UseCases;
using Core.Orders;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using WebUi.Pages.Common;

namespace WebUi.Pages.Orders;

public partial class OrderList : IDisposable
{
    [Inject] private IBaseInOutUseCase<BaseFullTextSearchPaginatedRequest, BasePaginatedResponse<OrderDto>> GetAllOrders { get; set; } = default!;
    [Inject] private IBaseInOutUseCase<DeleteOrderRequest, BaseResponse> DeleteOrder { get; set; } = default!;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private List<OrderDto> _orders = [];
    private bool _loading;
    private bool _showError;
    private string? _errorMessage;
    private string _searchDescription = string.Empty;
    private int _currentPage = 1;
    private int _totalPages = 1;
    private int _totalRecords;
    private const int PageSize = 10;

    private int _deleteTargetId;
    private DeleteConfirmDialog _dialog = default!;

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        _loading = true;
        _showError = false;

        var result = await GetAllOrders.HandleAsync(
            new(Guid.NewGuid(), _currentPage, PageSize, SearchValue: _searchDescription),
            _cancellationTokenSource.Token
        );

        _loading = false;

        if (!result.Success)
        {
            _errorMessage = result.Message;
            _showError = true;
            _orders = [];
            return;
        }

        _orders = result.Data?.ToList() ?? [];
        _totalPages = result.TotalPages;
        _totalRecords = result.TotalRecords;
    }

    private async Task SearchAsync()
    {
        _currentPage = 1;
        await LoadAsync();
    }

    private async Task OnSearchKeyUp(KeyboardEventArgs e)
    {
        if (e.Key == "Enter") await SearchAsync();
    }

    private async Task PreviousPage()
    {
        if (_currentPage > 1) { _currentPage--; await LoadAsync(); }
    }

    private async Task NextPage()
    {
        if (_currentPage < _totalPages) { _currentPage++; await LoadAsync(); }
    }

    private void OpenDeleteDialog(int id)
    {
        _deleteTargetId = id;
        _dialog.Show();
    }

    private async Task ConfirmDeleteAsync()
    {
        var result = await DeleteOrder.HandleAsync(
            new(Guid.NewGuid(), _deleteTargetId),
            _cancellationTokenSource.Token
        );

        if (!result.Success)
        {
            _errorMessage = result.Message;
            _showError = true;
            return;
        }

        await LoadAsync();
    }

    public void Dispose()
    {
        _cancellationTokenSource.Dispose();
        GC.SuppressFinalize(this);
    }
}
