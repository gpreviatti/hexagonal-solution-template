# BaseInOutUseCase Template

```csharp
using Application.Common.Helpers;
using Application.Common.Requests;
using Application.Common.UseCases;
using Domain.Common.Enums;
using FluentValidation;

namespace Application.{Context};

public sealed record {UseCaseName}Request(
    Guid CorrelationId,
    int Id,
    string User = "",
    string TimezoneId = ""
) : BaseRequest(CorrelationId, User, TimezoneId);

public sealed class {UseCaseName}RequestValidator : AbstractValidator<{UseCaseName}Request>
{
    public {UseCaseName}RequestValidator()
    {
        RuleFor(r => r.CorrelationId).NotEmpty();
        RuleFor(r => r.Id).GreaterThan(0);
    }
}

public sealed class {UseCaseName}UseCase(IServiceProvider serviceProvider)
    : BaseInOutUseCase<{UseCaseName}Request, BaseResponse<{DtoName}>>(serviceProvider)
{
    private readonly NotificationType _notificationType = NotificationType.OrderUpdated;

    public override async Task<BaseResponse<{DtoName}>> HandleInternalAsync(
        {UseCaseName}Request request,
        CancellationToken cancellationToken
    )
    {
        var correlationId = request.CorrelationId;

        // TODO: repository/domain logic

        var response = new BaseResponse<{DtoName}>(true, new());

        CreateNotification(correlationId, NotificationStatus.Success, request.User, _notificationType, response);

        return response;
    }
}
```