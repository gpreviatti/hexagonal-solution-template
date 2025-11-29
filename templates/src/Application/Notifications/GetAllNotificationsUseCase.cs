using Application.Common.Constants;
using Application.Common.Repositories;
using Application.Common.Requests;
using Application.Common.UseCases;
using Domain.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Notifications;

public sealed class GetAllNotificationsUseCase(IServiceProvider serviceProvider) 
    : BaseInOutUseCase<BasePaginatedRequest, BasePaginatedResponse<NotificationDto>>(serviceProvider)
{
    private readonly IBaseRepository<Notification> _repository = serviceProvider
        .GetRequiredService<IBaseRepository<Notification>>();
    public override async Task<BasePaginatedResponse<NotificationDto>> HandleInternalAsync(
        BasePaginatedRequest request,
        CancellationToken cancellationToken
    )
    {
        var (notifications, totalRecords) = await _repository.GetAllPaginatedAsync(
            request.CorrelationId,
            request.Page,
            request.PageSize,
            cancellationToken,
            request.SortBy,
            request.SortDescending,
            request.SearchByValues,
            selector: n => new NotificationDto
            {
                Id = n.Id,
                Message = n.Message,
                NotificationType = n.NotificationType,
                NotificationStatus = n.NotificationStatus,
            }
        );

        if (notifications is null || !notifications.Any())
        {
            logger.LogWarning(
                DefaultApplicationMessages.DefaultApplicationMessage + "No notifications found.",
                ClassName,
                HandleMethodName,
                request.CorrelationId
            );
            return new(false, 0, 0, [], "No notifications found.");
        }

        var totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);

        var notificationDtos = notifications.Select(notification => notification);

        return new(true, totalPages, totalRecords, notificationDtos);
    }
}
