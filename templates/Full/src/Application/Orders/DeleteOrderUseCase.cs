using Application.Common.Helpers;
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
        {
            Logs.NotFound(Logger, correlationId, nameof(order));

            response = new(false, "Order not found.");

            CreateNotification(correlationId, NotificationStatus.Failed, request.DeletedBy, _notificationType, response);

            UseCaseFailedMetric.Add(1);

            return response;
        }

        var deleteResult = order.Delete(request.DeletedBy, request.TimezoneId);
        if (deleteResult.IsFailure)
        {
            Logs.FailedOperation(Logger, correlationId, deleteResult.Message);

            response = new(false, deleteResult.Message);

            CreateNotification(correlationId, NotificationStatus.Failed, request.DeletedBy, _notificationType, response);

            UseCaseFailedMetric.Add(1);

            return response;
        }

        if (await Repository.UpdateAsync(order, correlationId, cancellationToken) == 0)
        {
            Logs.FailedOperation(Logger, correlationId, "Failed to delete order. No rows affected.");

            response = new(false, "Failed to delete order.");

            CreateNotification(correlationId, NotificationStatus.Failed, request.DeletedBy, _notificationType, response);

            UseCaseFailedMetric.Add(1);

            return response;
        }

        response = new(true);

        CreateNotification(correlationId, NotificationStatus.Success, request.DeletedBy, _notificationType, response);

        return response;
    }
}
