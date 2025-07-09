using Application.Common.Messages;
using Application.Common.UseCases;
using Domain.Orders;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Orders.Get;
public sealed class GetOrderUseCase(IServiceProvider serviceProvider) : BaseInOutUseCase<GetOrderRequest, OrderDto, Order>(
    serviceProvider,
    serviceProvider.GetService<IValidator<GetOrderRequest>>()
), IGetOrderUseCase
{
    public override async Task<BaseResponse<OrderDto>> HandleInternalAsync(
        GetOrderRequest request,
        CancellationToken cancellationToken
    )
    {
        var response = new BaseResponse<OrderDto>();

        var order = await _repository
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
