# Entity Template (Aggregate Root)

```csharp
using Domain.Common;
using Domain.Common.Extensions;

namespace Domain.{Context};

public sealed class {EntityName} : DomainEntity
{
    public {EntityName}() { }

    private {EntityName}(string name, string? createdBy = null, string? timezoneId = null)
        : base(createdBy ?? "System", timezoneId)
    {
        Name = name;
    }

    public string Name { get; private set; }
    public bool IsDeleted { get; private set; }

    public static Result<{EntityName}> Create(string name, string user = "System", string? timezoneId = null) => Handle(activity =>
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Fail<{EntityName}>("Name is required.");

        var entity = new {EntityName}(name, user, timezoneId);
        activity?.SetTag(nameof(name), name);
        return Result.Ok(entity);
    });

    public Result Update(string name, string user = "System", string? timezoneId = null) => Handle(activity =>
    {
        if (IsDeleted)
            return Result.Fail("Cannot update deleted entity.");

        if (string.IsNullOrWhiteSpace(name))
            return Result.Fail("Name is required.");

        Name = name;
        var updateResult = Update(user, timezoneId);
        if (updateResult.IsFailure)
            return updateResult;

        activity?.SetTag(nameof(name), name);
        return Result.Ok();
    });
}
```
