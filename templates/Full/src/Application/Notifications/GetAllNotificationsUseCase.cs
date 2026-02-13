using Application.Common.Constants;
using Application.Common.Requests;
using Application.Common.UseCases;
using Application.Common.Helpers;
using Domain.Notifications;
using Microsoft.Extensions.Logging;

namespace Application.Notifications;

public sealed class GetAllNotificationsUseCase(IServiceProvider serviceProvider)
    : BaseInOutUseCase<BasePaginatedRequest, BasePaginatedResponse<NotificationDto>>(serviceProvider)
{
    public override async Task<BasePaginatedResponse<NotificationDto>> HandleInternalAsync(
        BasePaginatedRequest request,
        CancellationToken cancellationToken
    )
    {
        var (notifications, totalRecords) = await Repository.GetAllPaginatedAsync<Notification, NotificationDto>(
            request.CorrelationId,
            request.Page,
            request.PageSize,
            n => new()
            {
                Id = n.Id,
                Message = n.Message,
                NotificationType = n.NotificationType,
                NotificationStatus = n.NotificationStatus,
            },
            cancellationToken,
            request.SortBy,
            request.SortDescending,
            request.SearchByValues
        );

        if (notifications is null || !notifications.Any())
        {
            Logs.NotFound(Logger, ClassName, HandleMethodName, request.CorrelationId, nameof(Notification));
            return new(false, 0, 0, [], "No notifications found.");
        }

        var totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);

        var notificationDtos = notifications.Select(notification => notification);

        return new(true, totalPages, totalRecords, notificationDtos);
    }
}
