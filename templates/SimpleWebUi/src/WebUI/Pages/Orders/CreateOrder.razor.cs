using System.ComponentModel.DataAnnotations;
using Core.Common.Requests;
using Core.Common.UseCases;
using Core.Orders;
using Microsoft.AspNetCore.Components;

namespace WebUi.Pages.Orders;

public partial class CreateOrder : IDisposable
{
    [Inject] private IBaseInOutUseCase<CreateOrderRequest, BaseResponse<OrderDto>> CreateOrderUseCase { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    [MinLength(1, ErrorMessage = "Description is required")]
    public string Description { get; set; } = string.Empty;

    private List<CreateOrderItemRequest> _items = [];
    private bool _submitting;
    private bool _showError;
    private string? _errorMessage;

    protected override void OnInitialized() => _items.Add(new(string.Empty, string.Empty, 0));

    private void AddItem() => _items.Add(new(string.Empty, string.Empty, 0));

    private void RemoveItem(int index)
    {
        if (_items.Count > 1)
            _items.RemoveAt(index);
    }

    private void UpdateItemName(int index, string value) =>
        _items[index] = _items[index] with { Name = value };

    private void UpdateItemDescription(int index, string value) =>
        _items[index] = _items[index] with { Description = value };

    private void UpdateItemValue(int index, string value)
    {
        if (decimal.TryParse(value, out var parsed))
            _items[index] = _items[index] with { Value = parsed };
    }

    private async Task SubmitAsync()
    {
        _submitting = true;
        _showError = false;

        var result = await CreateOrderUseCase.HandleAsync(
            new(Guid.NewGuid(), Description, [.. _items]),
            _cancellationTokenSource.Token
        );

        _submitting = false;

        if (!result.Success)
        {
            _errorMessage = result.Message;
            _showError = true;
            return;
        }

        Nav.NavigateTo($"/orders");
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        GC.SuppressFinalize(this);
    }
}
