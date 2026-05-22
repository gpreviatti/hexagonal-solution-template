# BaseOutUseCase Template

```csharp
using Application.Common.Helpers;
using Application.Common.Requests;
using Application.Common.UseCases;

namespace Application.{Context};

public sealed class {UseCaseName}UseCase(IServiceProvider serviceProvider)
    : BaseOutUseCase<BaseResponse<IEnumerable<{DtoName}>>>(serviceProvider)
{
    public override async Task<BaseResponse<IEnumerable<{DtoName}>>> HandleInternalAsync(
        CancellationToken cancellationToken
    )
    {
        var correlationId = Guid.NewGuid();

        var items = await Repository.GetQueryable<{EntityName}>(correlationId)
            .Select(x => new {DtoName}
            {
                Id = x.Id
            })
            .ToListAsync(cancellationToken);

        if (!items.Any())
        {
            Logs.NotFound(Logger, correlationId, nameof(items));
            return new(false, [], "No records found.");
        }

        return new(true, items);
    }
}
```
