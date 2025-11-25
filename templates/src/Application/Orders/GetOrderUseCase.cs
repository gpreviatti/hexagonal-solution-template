using Application.Common.Constants;
using Application.Common.Requests;
using Application.Common.UseCases;
using Domain.Orders;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Orders;

public sealed record GetOrderRequest(Guid CorrelationId, int Id) : BaseRequest(CorrelationId);

public sealed class GetOrderRequestValidator : AbstractValidator<GetOrderRequest>
{
    public GetOrderRequestValidator()
    {
        RuleFor(r => r.Id).NotEmpty();
    }
}

public sealed class GetOrderUseCase(IServiceProvider serviceProvider) : BaseInOutUseCase<GetOrderRequest, BaseResponse<OrderDto>, Order, GetOrderUseCase>(
    serviceProvider,
    serviceProvider.GetRequiredService<IValidator<GetOrderRequest>>()
)
{
    public override async Task<BaseResponse<OrderDto>> HandleInternalAsync(
        GetOrderRequest request,
        CancellationToken cancellationToken
    )
    {
        var order = await _repository.GetByIdAsNoTrackingAsync<Order>(request.Id, request.CorrelationId, cancellationToken, o => o.Items);

        if (order is null || order.Equals(default(Order)))
        {
            logger.LogWarning(
                DefaultApplicationMessages.DefaultApplicationMessage + "Order not found.",
                ClassName, HandleMethodName, request.CorrelationId
            );
            return new(false, null, "Order not found.");
        }

        return new(true, order);
    }
}
