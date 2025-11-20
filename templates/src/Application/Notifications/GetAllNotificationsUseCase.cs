using System.Diagnostics.Metrics;
using Application.Common.Constants;
using Application.Common.Requests;
using Application.Common.UseCases;
using Domain.Notifications;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Notifications;

public sealed class GetAllNotificationsUseCase(IServiceProvider serviceProvider)
    : BaseInOutUseCase<BasePaginatedRequest, BasePaginatedResponse<NotificationDto>, Notification, GetAllNotificationsUseCase>(
        serviceProvider,
        serviceProvider.GetRequiredService<IValidator<BasePaginatedRequest>>()
    )
{
    public static readonly Counter<int> NotificationsListed = DefaultConfigurations.Meter
        .CreateCounter<int>("notifications.listed", "notifications", "Number of times notifications were listed");

    public override async Task<BasePaginatedResponse<NotificationDto>> HandleInternalAsync(
        BasePaginatedRequest request,
        CancellationToken cancellationToken
    )
    {
        var (notifications, totalRecords) = await _repository.GetAllPaginatedAsync<Notification>(
            request.Page,
            request.PageSize,
            request.CorrelationId,
            cancellationToken,
            request.SortBy,
            request.SortDescending,
            request.SearchByValues
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

        var notificationDtos = notifications.Select(notification => new NotificationDto(
            notification.Id,
            notification.NotificationType,
            notification.NotificationStatus,
            notification.Message,
            notification.CreatedAt,
            notification.UpdatedAt,
            notification.CreatedBy,
            notification.UpdatedBy
        ));

        NotificationsListed.Add(1);

        return new(true, totalPages, totalRecords, notificationDtos);
    }
}
