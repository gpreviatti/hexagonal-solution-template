using System.ComponentModel.DataAnnotations;
using Application.Common.Helpers;
using Application.Common.Requests;
using Application.Common.UseCases;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders;

public sealed record GetOrderRequest([Required] Guid CorrelationId, [Required] int Id) : BaseRequest(CorrelationId);

public sealed class GetOrderUseCase(IServiceProvider serviceProvider) : BaseInOutUseCase<GetOrderRequest, BaseResponse<OrderDto>>(serviceProvider)
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
            PeriodSinceWasCreated = o.GetPeriodSinceWasCreated(),
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
            Logs.NotFound(Logger, request.CorrelationId, nameof(order));
            return new(false, null, "Order not found.");
        }

        return new(true, order);
    }
}
