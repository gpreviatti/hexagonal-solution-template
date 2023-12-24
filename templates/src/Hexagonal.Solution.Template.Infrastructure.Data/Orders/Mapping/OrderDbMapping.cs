using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Hexagonal.Solution.Template.Domain.Orders;
using Hexagonal.Solution.Template.Infrastructure.Data.Common;

namespace Hexagonal.Solution.Template.Infrastructure.Data.Orders.Mapping;
public class OrderDbMapping : BaseDbMapping<Order>
{
    public override void ConfigureDomainEntity(EntityTypeBuilder<Order> builder)
    {
        builder.Property(p => p.Description)
            .HasMaxLength(255)
            .IsRequired(true);

        builder.Property(p => p.Total)
            .IsRequired(true);

        builder.HasMany(p => p.Items);

        //builder.Navigation(p => p.Items).AutoInclude(false);
    }
}
