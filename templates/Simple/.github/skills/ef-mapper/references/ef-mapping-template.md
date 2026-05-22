# EF Mapping Template

```csharp
using Domain.{Context};
using Infrastructure.Data.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Mapping;

internal sealed class {EntityName}DbMapping : BaseDbMapping<{EntityName}>
{
    public override void ConfigureDomainEntity(EntityTypeBuilder<{EntityName}> builder)
    {
        builder.Property(p => p.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(p => p.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.HasMany(p => p.Items);
    }
}
```
