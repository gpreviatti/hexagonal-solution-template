using Application.Common.Requests;
using Application.Common.UseCases;
using Domain.Common.Enums;
using Domain.Orders;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders;

public sealed record DeleteOrderRequest(
    Guid CorrelationId,
    int OrderId,
    string DeletedBy = "",
    string TimezoneId = ""
) : BaseRequest(CorrelationId, DeletedBy, TimezoneId);

public sealed class DeleteOrderRequestValidator : AbstractValidator<DeleteOrderRequest>
{
    public DeleteOrderRequestValidator()
    {
        RuleFor(r => r.CorrelationId).NotEmpty();
        RuleFor(r => r.OrderId).NotEmpty();
    }
}

public sealed class DeleteOrderUseCase(IServiceProvider serviceProvider)
    : BaseInOutUseCase<DeleteOrderRequest, BaseResponse>(serviceProvider)
{
    private readonly NotificationType _notificationType = NotificationType.OrderDeleted;

    public override async Task<BaseResponse> HandleInternalAsync(
        DeleteOrderRequest request,
        CancellationToken cancellationToken
    )
    {
        var correlationId = request.CorrelationId;
        BaseResponse response;

        var order = await Repository.GetQueryable<Order>(correlationId)
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken);

        if (order is null)
            return HandleFailedResponse<DeleteOrderRequest, BaseResponse>(
                request, correlationId, _notificationType,
                request.DeletedBy, "Order not found."
            );

        var deleteResult = order.Delete(request.DeletedBy, request.TimezoneId);
        if (deleteResult.IsFailure)
            return HandleFailedResponse<DeleteOrderRequest, BaseResponse>(
                request, correlationId, _notificationType,
                request.DeletedBy, deleteResult.Message
            );

        if (await Repository.UpdateAsync(order, correlationId, cancellationToken) == 0)
            return HandleFailedResponse<DeleteOrderRequest, BaseResponse>(
                request, correlationId, _notificationType,
                request.DeletedBy, "Failed to delete order."
            );

        response = new(true);

        HandleNotification(correlationId, NotificationStatus.Success, request.DeletedBy, _notificationType, response);

        return response;
    }
}
