using System.Diagnostics.Metrics;
using Application.Common.Constants;
using Application.Common.Requests;
using Application.Common.UseCases;
using Domain.Orders;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Orders;

public sealed class GetAllOrdersUseCase(IServiceProvider serviceProvider) : BaseInOutUseCase<BasePaginatedRequest, BasePaginatedResponse<OrderDto>, Order, GetAllOrdersUseCase>(
    serviceProvider,
    serviceProvider.GetService<IValidator<BasePaginatedRequest>>()
)
{
    public static Counter<int> OrdersListed = DefaultConfigurations.Meter
        .CreateCounter<int>("orders.listed", "orders", "Number of times orders were listed");

    public override async Task<BasePaginatedResponse<OrderDto>> HandleInternalAsync(
        BasePaginatedRequest request,
        CancellationToken cancellationToken
    )
    {
        var (orders, totalRecords) = await _repository.GetAllPaginatedAsync(
            request.Page,
            request.PageSize,
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
            return new(0, 0, [], false, "No orders found.");
        }

        var totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);

        var orderDtos = orders.Select(o => new OrderDto(
            o.Id,
            o.Description,
            o.Total,
            o.CreatedAt
        ));

        OrdersListed.Add(1);

        return new(
            totalPages,
            totalRecords,
            orderDtos,
            true
        );
    }
}
