using System.Diagnostics.Metrics;
using Application.Common.Constants;
using Application.Common.Requests;
using Application.Common.UseCases;
using Domain.Notifications;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Notifications;

public sealed record UpdateNotificationRequest(
    Guid CorrelationId,
    int Id,
    string NotificationType,
    NotificationTypeStatus NotificationTypeStatus,
    string Message = ""
) : BaseRequest(CorrelationId);

public sealed class UpdateNotificationRequestValidator : AbstractValidator<UpdateNotificationRequest>
{
    public UpdateNotificationRequestValidator()
    {
        RuleFor(r => r.CorrelationId).NotEmpty();
        RuleFor(r => r.Id).GreaterThan(0);
        RuleFor(r => r.NotificationType).NotEmpty();
        RuleFor(r => r.NotificationTypeStatus).IsInEnum();
    }
}

public sealed class UpdateNotificationUseCase(IServiceProvider serviceProvider) 
    : BaseInOutUseCase<UpdateNotificationRequest, BaseResponse<NotificationDto>, Notification, UpdateNotificationUseCase>(
        serviceProvider,
        serviceProvider.GetService<IValidator<UpdateNotificationRequest>>()
    )
{
    public static Counter<int> NotificationUpdated = DefaultConfigurations.Meter
        .CreateCounter<int>("notification.updated", "notifications", "Number of notifications updated");

    public override async Task<BaseResponse<NotificationDto>> HandleInternalAsync(
        UpdateNotificationRequest request,
        CancellationToken cancellationToken
    )
    {
        var notification = await _repository.GetByIdAsNoTrackingAsync(request.Id, cancellationToken, Array.Empty<System.Linq.Expressions.Expression<Func<Notification, object>>>());

        if (notification is null)
        {
            logger.LogWarning(
                DefaultApplicationMessages.DefaultApplicationMessage + "Notification not found.",
                ClassName, 
                HandleMethodName, 
                request.CorrelationId
            );
            return new(null, false, "Notification not found.");
        }

        notification.NotificationType = request.NotificationType;
        notification.NotificationTypeStatus = request.NotificationTypeStatus;
        notification.Message = request.Message ?? string.Empty;
        notification.SetUpdatedAt();

        var updateResult = await _repository.UpdateAsync(notification, cancellationToken);
        if (updateResult == 0)
        {
            logger.LogWarning(
                DefaultApplicationMessages.DefaultApplicationMessage + "Failed to update notification.",
                ClassName, 
                HandleMethodName, 
                request.CorrelationId
            );
            return new(null, false, "Failed to update notification.");
        }

        NotificationUpdated.Add(1);

        return new(new(
            notification.Id,
            notification.NotificationType,
            notification.NotificationTypeStatus,
            notification.Message,
            notification.CreatedAt,
            notification.UpdatedAt,
            notification.CreatedBy,
            notification.UpdatedBy
        ), true);
    }
}