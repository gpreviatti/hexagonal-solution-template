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
        serviceProvider.GetService<IValidator<BasePaginatedRequest>>()
    )
{
    public static Counter<int> NotificationsListed = DefaultConfigurations.Meter
        .CreateCounter<int>("notifications.listed", "notifications", "Number of times notifications were listed");

    public override async Task<BasePaginatedResponse<NotificationDto>> HandleInternalAsync(
        BasePaginatedRequest request,
        CancellationToken cancellationToken
    )
    {
        var (notifications, totalRecords) = await _repository.GetAllPaginatedAsync(
            request.Page,
            request.PageSize,
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
            return new BasePaginatedResponse<NotificationDto>(
                0, 0, [], false, "No notifications found."
            );
        }

        var totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);

        var notificationDtos = notifications.Select(n => new NotificationDto(
            n.Id,
            n.NotificationType,
            n.NotificationTypeStatus,
            n.Message,
            n.CreatedAt,
            n.UpdatedAt,
            n.CreatedBy,
            n.UpdatedBy
        ));

        NotificationsListed.Add(1);

        return new BasePaginatedResponse<NotificationDto>(
            totalPages,
            totalRecords,
            notificationDtos,
            true
        );
    }
}