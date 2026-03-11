using Application.Common.Requests;
using Application.Common.UseCases;
using Application.Common.Helpers;
using Domain.Orders;

namespace Application.Orders;

public sealed class GetAllOrdersUseCase(IServiceProvider serviceProvider)
    : BaseInOutUseCase<BasePaginatedRequest, BasePaginatedResponse<OrderDto>>(serviceProvider)
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
            Logs.NotFound(Logger, HandleMethodName, request.CorrelationId, nameof(orders));
            return new(false, 0, 0, [], "No orders found.");
        }

        var totalPages = (int) Math.Ceiling(totalRecords / (double) request.PageSize);

        return new(true, totalPages, totalRecords, orders);
    }
}
