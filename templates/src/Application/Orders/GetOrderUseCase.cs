using System.Diagnostics.Metrics;
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
    serviceProvider.GetService<IValidator<GetOrderRequest>>()
)
{
    private const string ClassName = nameof(GetOrderUseCase);
    public static Counter<int> OrderRetrieved = DefaultConfigurations.Meter
        .CreateCounter<int>("order.retrieved", "orders", "Number of orders retrieved");

    public override async Task<BaseResponse<OrderDto>> HandleInternalAsync(
        GetOrderRequest request,
        CancellationToken cancellationToken
    )
    {
        string methodName = nameof(HandleInternalAsync);

        var order = await _cache.GetOrCreateAsync(
            $"Order-{request.Id}",
            async cancellationToken => await _repository.GetByIdAsNoTrackingAsync(request.Id, cancellationToken, o => o.Items),
            cancellationToken
        );

        if (order is null || order.Equals(default(Order)))
        {
            logger.LogWarning(DefaultApplicationMessages.DefaultApplicationMessage + "Order not found.", ClassName, methodName, request.CorrelationId);
            return new(null, false, "Order not found.");
        }

        logger.LogInformation(DefaultApplicationMessages.FinishedExecutingUseCase, ClassName, methodName, request.CorrelationId);

        OrderRetrieved.Add(1);

        return new(new(
            order.Id,
            order.Description,
            order.Total,
            order.CreatedAt,
            order.Items.Select(i => new ItemDto(
                i.Id,
                i.Name,
                i.Description,
                i.Value
            )).ToList()
        ), true);
    }
}
