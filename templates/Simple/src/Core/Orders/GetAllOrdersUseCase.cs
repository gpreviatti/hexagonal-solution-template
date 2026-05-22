using Core.Common.Helpers;
using Core.Common.Requests;
using Core.Common.UseCases;
using Core.Orders;

namespace Core.Orders;

public sealed class GetAllOrdersUseCase(IServiceProvider serviceProvider): BaseInOutUseCase<BasePaginatedRequest, BasePaginatedResponse<OrderDto>>(serviceProvider)
{
    public override async Task<BasePaginatedResponse<OrderDto>> HandleInternalAsync(
        BasePaginatedRequest request,
        CancellationToken cancellationToken
    )
    {
        var (orders, totalRecords) = await Repository.GetAllPaginatedAsync<Order, OrderDto>(
            request.CorrelationId,
            request.Page,
            request.PageSize,
            o => new()
            {
                Id = o.Id,
                Total = o.Total
            },
            cancellationToken,
            request.SortBy,
            request.SortDescending,
            request.SearchByValues
        );

        if (orders is null || !orders.Any())
        {
            Logs.NotFound(Logger, request.CorrelationId, nameof(orders));
            return new(false, 0, 0, [], "No orders found.");
        }

        var totalPages = (int) Math.Ceiling(totalRecords / (double) request.PageSize);

        return new(true, totalPages, totalRecords, orders);
    }
}
