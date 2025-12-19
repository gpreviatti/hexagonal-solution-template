using Application.Common.Constants;
using Application.Common.Repositories;
using Application.Common.Requests;
using Application.Common.UseCases;
using Domain.Orders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Orders;

public sealed class GetAllOrdersUseCase(IServiceProvider serviceProvider)
    : BaseInOutUseCase<BasePaginatedRequest, BasePaginatedResponse<OrderDto>>(serviceProvider)
{
    private readonly IBaseRepository<Order> _repository = serviceProvider
        .GetRequiredService<IBaseRepository<Order>>();

    public override async Task<BasePaginatedResponse<OrderDto>> HandleInternalAsync(
        BasePaginatedRequest request,
        CancellationToken cancellationToken
    )
    {
        var (orders, totalRecords) = await _repository.GetAllPaginatedAsync(
            request.CorrelationId,
            request.Page,
            request.PageSize,
            o => new OrderDto
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
                DefaultApplicationMessages.DefaultApplicationMessage + "No orders found.",
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
