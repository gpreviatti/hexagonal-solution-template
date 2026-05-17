using System.ComponentModel.DataAnnotations;
using Application.Common.Helpers;
using Application.Common.Requests;
using Application.Common.UseCases;
using Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Application.Notifications;

public sealed record GetNotificationRequest(
    [Required] Guid CorrelationId,
    [Required] int Id
) : BaseRequest(CorrelationId);

public sealed class GetNotificationUseCase(IServiceProvider serviceProvider)
    : BaseInOutUseCase<GetNotificationRequest, BaseResponse<NotificationDto>>(serviceProvider)
{
    public override async Task<BaseResponse<NotificationDto>> HandleInternalAsync(
        GetNotificationRequest request,
        CancellationToken cancellationToken
    )
    {
        var notification = await Repository.GetQueryable<Notification>(request.CorrelationId)
            .Where(n => n.Id == request.Id)
            .Select(n => new NotificationDto()
            {
                Id = n.Id,
                Message = n.Message,
                NotificationType = n.NotificationType,
                NotificationStatus = n.NotificationStatus
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (notification is null)
        {
            Logs.NotFound(Logger, request.CorrelationId, nameof(notification));
            return new(false, null, "Notification not found.");
        }

        return new(true, notification);
    }
}
