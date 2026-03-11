using Application.Common.Requests;
using Application.Common.UseCases;
using Application.Common.Helpers;
using Domain.Orders;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders;

public sealed record GetOrderRequest(Guid CorrelationId, int Id) : BaseRequest(CorrelationId);

public sealed class GetOrderRequestValidator : AbstractValidator<GetOrderRequest>
{
    public GetOrderRequestValidator()
    {
        RuleFor(r => r.Id).NotEmpty();
    }
}

public sealed class GetOrderUseCase(IServiceProvider serviceProvider)  : BaseInOutUseCase<GetOrderRequest, BaseResponse<OrderDto>>(serviceProvider)
{
    public override async Task<BaseResponse<OrderDto>> HandleInternalAsync(
        GetOrderRequest request,
        CancellationToken cancellationToken
    )
    {
        var order = await Repository.GetQueryable<Order>(request.CorrelationId)
        .Select(o => new OrderDto
        {
            Id = o.Id,
            Description = o.Description,
            Total = o.Total,
            Items = o.Items.Select(i => new ItemDto
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                Value = i.Value
            }).ToList()
        }).FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (order is null)
        {
            Logs.NotFound(Logger, ClassName, HandleMethodName, request.CorrelationId, nameof(order));
            return new(false, null, "Order not found.");
        }

        return new(true, order);
    }
}
