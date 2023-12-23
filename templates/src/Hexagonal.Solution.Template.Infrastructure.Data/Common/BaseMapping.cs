using Hexagonal.Solution.Template.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hexagonal.Solution.Template.Infrastructure.Data.Common;
public abstract class BaseDbMapping<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : DomainEntity
{
    public void Configure(EntityTypeBuilder<TEntity> builder)
    {

        builder.HasKey(p => p.Id);

        ConfigureDomainEntity(builder);

        builder.Property(p => p.CreatedAt)
            .IsRequired(true);

        builder.Property(p => p.UpdatedAt)
            .IsRequired(false);
    }

    public abstract void ConfigureDomainEntity(EntityTypeBuilder<TEntity> builder);
}
