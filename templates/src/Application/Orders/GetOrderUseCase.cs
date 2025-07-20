using Application.Common.Messages;
using Application.Common.Requests;
using Application.Common.UseCases;
using Domain.Orders;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Orders;

public sealed record GetOrderRequest(Guid CorrelationId, int Id) : BaseRequest(CorrelationId);

public sealed class GetOrderRequestValidator : AbstractValidator<GetOrderRequest>
{
    public GetOrderRequestValidator()
    {
        RuleFor(r => r.Id).NotEmpty();
    }
}

public sealed class GetOrderUseCase(IServiceProvider serviceProvider) : BaseInOutUseCase<GetOrderRequest, OrderDto, Order>(
    serviceProvider,
    serviceProvider.GetService<IValidator<GetOrderRequest>>()
)
{
    private const string ClassName = nameof(GetOrderUseCase);

    public override async Task<BaseResponse<OrderDto>> HandleInternalAsync(
        GetOrderRequest request,
        CancellationToken cancellationToken
    )
    {
        string methodName = nameof(HandleInternalAsync);
        var response = new BaseResponse<OrderDto>();

        var order = await _repository.GetByIdAsNoTrackingAsync(
            request.Id,
            cancellationToken,
            o => o.Items
        );

        if (order is null || order.Equals(default(Order)))
        {
            logger.Warning(DefaultApplicationMessages.DefaultApplicationMessage + "Order not found.", ClassName, methodName, request.CorrelationId);
            response.SetMessage("Order not found.");
            return response;
        }

        response.SetData(new(
            order.Id,
            order.Description,
            order.Total
        ));

        logger.Information(DefaultApplicationMessages.FinishedExecutingUseCase, ClassName, methodName, request.CorrelationId);

        return response;
    }
}
