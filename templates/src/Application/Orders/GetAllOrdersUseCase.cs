using Application.Common.Constants;
using Application.Common.Requests;
using Application.Common.UseCases;
using Domain.Orders;
using Microsoft.Extensions.Logging;

namespace Application.Orders;

public sealed class GetAllOrdersUseCase(IServiceProvider serviceProvider)
    : BaseInOutUseCase<BasePaginatedRequest, BasePaginatedResponse<OrderDto>>(serviceProvider)
{
    public override async Task<BasePaginatedResponse<OrderDto>> HandleInternalAsync(
        BasePaginatedRequest request,
        CancellationToken cancellationToken
    )
    {
        var (orders, totalRecords) = await _repository.GetAllPaginatedAsync<Order, OrderDto>(
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
            logger.LogWarning(
                "[{ClassName}] | [{MethodName}] | [{CorrelationId}] | No orders found.",
                ClassName,
                HandleMethodName,
                request.CorrelationId
            );
            return new(false, 0, 0, [], "No orders found.");
        }

        var totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);

        return new(true, totalPages, totalRecords, orders);
    }
}
