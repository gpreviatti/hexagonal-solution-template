using Application.Common.Messages;
using Application.Common.Repositories;
using Application.Common.UseCases;
using Domain.Orders;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Orders.Get;
public sealed class GetOrderUseCase(IServiceProvider serviceProvider) : BaseInOutUseCase<GetOrderRequest, OrderDto>(
    serviceProvider,
    serviceProvider.GetService<IValidator<GetOrderRequest>>()
), IGetOrderUseCase
{
    private readonly IBaseRepository<Order> _orderRepository = serviceProvider.GetRequiredService<IBaseRepository<Order>>();

    public override async Task<BaseResponse<OrderDto>> HandleInternalAsync(
        GetOrderRequest request,
        CancellationToken cancellationToken
    )
    {
        var response = new BaseResponse<OrderDto>();

        var order = await _orderRepository
            .GetByIdAsNoTrackingAsync(request.Id, cancellationToken);

        response.SetData(new(
            order.Id,
            order.Description,
            order.Total
        ));

        logger.Information("Use case was executed with success", response);

        return response;
    }
}
