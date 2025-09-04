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
    private const string ClassName = nameof(GetAllOrdersUseCase);
    public static Counter<int> OrdersListed = DefaultConfigurations.Meter
        .CreateCounter<int>("orders.listed", "orders", "Number of times orders were listed");

    public override async Task<BasePaginatedResponse<OrderDto>> HandleInternalAsync(
        BasePaginatedRequest request,
        CancellationToken cancellationToken
    )
    {
        string methodName = nameof(HandleInternalAsync);
        var response = new BasePaginatedResponse<OrderDto>();

        var cacheKey = $"Orders-Page-{request.Page}-Size-{request.PageSize}";

        var (orders, totalRecords) = await _cache.GetOrCreateAsync(
            cacheKey,
            async cancellationToken => await _repository.GetAllPaginatedAsync(
                request.Page,
                request.PageSize,
                cancellationToken,
                request.SortBy,
                request.SortDescending,
                request.SearchPropertyName,
                request.SearchValue
            ),
            cancellationToken
        );

        if (orders is null || !orders.Any())
        {
            logger.LogWarning(
                DefaultApplicationMessages.DefaultApplicationMessage + "No orders found.",
                ClassName,
                methodName,
                request.CorrelationId
            );
            response.SetMessage("No orders found.");
            return response;
        }

        var totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);

        var orderDtos = orders.Select(o => new OrderDto(
            o.Id,
            o.Description,
            o.Total
        ));

        response.SetData(
            totalRecords,
            request.PageSize,
            orderDtos
        );

        logger.LogInformation(
            DefaultApplicationMessages.FinishedExecutingUseCase,
            ClassName,
            methodName,
            request.CorrelationId
        );

        OrdersListed.Add(1);

        return response;
    }
}