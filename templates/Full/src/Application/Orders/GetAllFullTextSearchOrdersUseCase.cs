using Application.Common.Helpers;
using Application.Common.Requests;
using Application.Common.UseCases;
using Domain.Orders;

namespace Application.Orders;

public sealed class GetAllFullTextSearchOrdersUseCase(IServiceProvider serviceProvider): BaseInOutUseCase<BaseFullTextSearchPaginatedRequest, BasePaginatedResponse<OrderDto>>(serviceProvider)
{
    public override async Task<BasePaginatedResponse<OrderDto>> HandleInternalAsync(
        BaseFullTextSearchPaginatedRequest request,
        CancellationToken cancellationToken
    )
    {
        var (orders, totalRecords) = await Repository.GetAllFullTextSearchPaginatedAsync<Order, OrderDto>(
            request.CorrelationId,
            request.Page,
            request.PageSize,
            o => new()
            {
                Id = o.Id,
                Description = o.Description,
                Total = o.Total,
                PeriodSinceWasCreated = o.GetPeriodSinceWasCreated()
            },
            cancellationToken,
            request.SortBy,
            request.SortDescending,
            nameof(Order.Description),
            request.SearchValue
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
