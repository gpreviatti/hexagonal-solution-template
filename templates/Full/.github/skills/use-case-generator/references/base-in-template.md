# BaseInUseCase Template (Fire-and-forget)

```csharp
using Application.Common.Helpers;
using Application.Common.Requests;
using Application.Common.UseCases;
using FluentValidation;

namespace Application.{Context};

public sealed record {UseCaseName}Request(
    Guid CorrelationId,
    int EntityId,
    string? CreatedBy = null,
    object? Message = null
) : BaseRequest(CorrelationId);

public sealed class {UseCaseName}RequestValidator : AbstractValidator<{UseCaseName}Request>
{
    public {UseCaseName}RequestValidator()
    {
        RuleFor(r => r.CorrelationId).NotEmpty();
        RuleFor(r => r.EntityId).GreaterThan(0);
    }
}

public sealed class {UseCaseName}UseCase(IServiceProvider serviceProvider)
    : BaseInUseCase<{UseCaseName}Request>(serviceProvider)
{
    public override async Task HandleInternalAsync(
        {UseCaseName}Request request,
        CancellationToken cancellationToken
    )
    {
        // TODO: perform side-effect operation
        var rows = await Repository.AddAsync(new object(), request.CorrelationId, cancellationToken);

        if (rows == 0)
            Logs.FailedOperation(Logger, request.CorrelationId, "Operation failed. No rows affected.");
    }
}
```