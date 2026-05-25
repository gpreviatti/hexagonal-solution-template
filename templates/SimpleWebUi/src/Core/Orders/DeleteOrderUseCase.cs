using Core.Common.Requests;
using Core.Common.UseCases;
using Core.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace Core.Orders;

public sealed record DeleteOrderRequest(
    Guid CorrelationId,
    int OrderId,
    string DeletedBy = "",
    string TimezoneId = ""
) : BaseRequest(CorrelationId, DeletedBy, TimezoneId);

public sealed class DeleteOrderUseCase(IServiceProvider serviceProvider) : BaseInOutUseCase<DeleteOrderRequest, BaseResponse>(serviceProvider)
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
            return await HandleFailedResponse<BaseResponse>(
                correlationId, _notificationType,
                request.DeletedBy, "Order not found."
            );

        var deleteResult = order.Delete(request.DeletedBy, request.TimezoneId);
        if (deleteResult.IsFailure)
            return await HandleFailedResponse<BaseResponse>(
                correlationId, _notificationType,
                request.DeletedBy, deleteResult.Message
            );

        if (await Repository.UpdateAsync(order, correlationId, cancellationToken) == 0)
            return await HandleFailedResponse<BaseResponse>(
                correlationId, _notificationType,
                request.DeletedBy, "Failed to delete order."
            );

        response = new(true);

        await HandleNotification(correlationId, NotificationStatus.Success, request.DeletedBy, _notificationType, response);

        return response;
    }
}
