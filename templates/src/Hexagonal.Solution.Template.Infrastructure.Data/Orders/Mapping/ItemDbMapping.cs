using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Hexagonal.Solution.Template.Domain.Orders;
using Hexagonal.Solution.Template.Infrastructure.Data.Common;

namespace Hexagonal.Solution.Template.Infrastructure.Data.Orders.Mapping;
public class ItemDbMapping : BaseDbMapping<Item>
{
    public override void ConfigureDomainEntity(EntityTypeBuilder<Item> builder)
    {
        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired(true);

        builder.Property(p => p.Value)
            .HasMaxLength(255)
            .IsRequired(true);

        builder.Property(p => p.Description)
            .HasMaxLength(255)
            .IsRequired(true);
    }
}
