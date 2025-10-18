using System.Diagnostics.Metrics;
using Application.Common.Constants;
using Application.Common.Requests;
using Application.Common.UseCases;
using Domain.Notifications;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Notifications;

public sealed record GetNotificationRequest(Guid CorrelationId, int Id) : BaseRequest(CorrelationId);

public sealed class GetNotificationRequestValidator : AbstractValidator<GetNotificationRequest>
{
    public GetNotificationRequestValidator()
    {
        RuleFor(r => r.CorrelationId).NotEmpty();
        RuleFor(r => r.Id).GreaterThan(0);
    }
}

public sealed class GetNotificationUseCase(IServiceProvider serviceProvider)
    : BaseInOutUseCase<GetNotificationRequest, BaseResponse<NotificationDto>, Notification, GetNotificationUseCase>(
        serviceProvider,
        serviceProvider.GetRequiredService<IValidator<GetNotificationRequest>>()
    )
{
    public static readonly Counter<int> NotificationRetrieved = DefaultConfigurations.Meter
        .CreateCounter<int>("notification.retrieved", "notifications", "Number of notifications retrieved");

    public override async Task<BaseResponse<NotificationDto>> HandleInternalAsync(
        GetNotificationRequest request,
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

        NotificationRetrieved.Add(1);

        return new(new(
            notification.Id,
            notification.NotificationType,
            notification.NotificationStatus,
            notification.Message,
            notification.CreatedAt,
            notification.UpdatedAt,
            notification.CreatedBy,
            notification.UpdatedBy
        ), true);
    }
}
