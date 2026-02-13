using System.Linq.Expressions;
using Application.Common.Constants;
using Application.Common.Repositories;
using Application.Common.Requests;
using Application.Common.UseCases;
using Application.Common.Helpers;
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
    : BaseInOutUseCase<GetNotificationRequest, BaseResponse<NotificationDto>>(serviceProvider)
{
    public override async Task<BaseResponse<NotificationDto>> HandleInternalAsync(
        GetNotificationRequest request,
        CancellationToken cancellationToken
    )
    {
        var notification = await Repository.GetByIdAsNoTrackingAsync<Notification, NotificationDto>(
            request.Id,
            request.CorrelationId,
            n => new()
            {
                Id = n.Id,
                Message = n.Message,
                NotificationType = n.NotificationType,
                NotificationStatus = n.NotificationStatus,
            },
            cancellationToken
        );

        if (notification is null)
        {
            Logs.NotFound(Logger, ClassName, HandleMethodName, request.CorrelationId, nameof(notification));
            return new(false, null, "Notification not found.");
        }

        return new(true, notification);
    }
}
