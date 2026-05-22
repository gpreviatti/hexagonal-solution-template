---
name: ef-mapper
description: 'Create and update Entity Framework Core mappings for domain entities in the hexagonal architecture template, including relationships, enum conversions, precision, and migrations.'
---

# EF Mapper Generator — Hexagonal Architecture Template

Create or update EF Core mapping classes under Infrastructure, keeping persistence rules explicit and consistent.

## When to Use

Activate this skill when:
- User asks to map a new entity to EF Core
- User adds or changes entity properties requiring DB config
- User introduces enums, decimal values, or relationships
- User requests migration generation after model changes

## Skill-local References

| File | Purpose |
|------|---------|
| `./references/ef-mapping-template.md` | Base mapping template and relationship samples |
| `./references/migration-commands.md` | Migration/update/script command quick reference |

## Mapping Rules

- Place mappings in `src/Infrastructure/Data/Mapping/`
- Inherit `BaseDbMapping<TEntity>`
- Configure inside `ConfigureDomainEntity(EntityTypeBuilder<TEntity> builder)`
- Use `HasMaxLength(...)` for strings
- Use `HasPrecision(18, 2)` for monetary decimals
- Use `.HasConversion<string>()` for enums stored as text
- Configure relationships (`HasMany`, `WithOne`, `HasForeignKey`) explicitly

## Enum Mapping Example

```csharp
builder.Property(p => p.NotificationStatus)
    .HasConversion<string>()
    .HasMaxLength(50)
    .IsRequired();
```

## Decimal Mapping Example

```csharp
builder.Property(p => p.Total)
    .HasPrecision(18, 2)
    .IsRequired();
```

## Final Checklist

- [ ] Mapping file created/updated under `Data/Mapping`
- [ ] Enum conversions configured where needed
- [ ] Decimal precision configured for money values
- [ ] Required/max length constraints set
- [ ] Navigation relationships configured
- [ ] Migration command planned/run using `./references/migration-commands.md`
