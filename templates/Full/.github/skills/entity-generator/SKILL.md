---
name: entity-generator
description: 'Generate entity classes, unit tests, and database migrations following hexagonal architecture DDD principles for this project'
---

# Entity Generator — Hexagonal Architecture Template

Generate domain entity classes, corresponding unit tests, and EF Core migrations following the project's hexagonal architecture patterns.

## When to Use

Activate this skill when:
- User requests a new entity or domain object
- User asks to add properties, methods, or business rules to an existing entity
- User needs a new database table/model following domain patterns
- User mentions "entity", "aggregate", "value object", or "domain model"
- User asks to implement soft delete, business validation, or domain operations

---

## Skill-local References

| File | Purpose |
|------|---------|
| `./references/entity-template.md` | Aggregate root template (`Create`, `Update`, `Delete`, `Result<T>`) |
| `./references/tests-template.md` | Domain unit test template with naming convention |

> Keep this skill self-contained: prefer these in-folder references when generating entities.

---

## Two Entity Patterns

### Pattern A — Aggregate Root (use `Result<T>` + static `Create`)

Use this for **aggregate roots** or entities with complex invariants and multiple business operations (create, update, delete).

**Example**: `Order`, any root entity of a bounded context.

```csharp
using Domain.Common;
using Domain.Common.Extensions;

namespace Domain.Products;

public sealed class Product : DomainEntity
{
    public Product() { }  // Required for EF Core

    private Product(
        string name,
        decimal price,
        string? createdBy = null,
        string? timezoneId = null
    ) : base(createdBy ?? "System", timezoneId)
    {
        Name = name;
        Price = price;
    }

    public string Name { get; private set; }
    public decimal Price { get; private set; }

    public static Result<Product> Create(
        string name,
        decimal price,
        string user = "System",
        string? timezoneId = null
    ) => Handle(activity =>
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Fail<Product>("Product name is required.");

        if (price <= 0)
            return Result.Fail<Product>("Product price must be greater than zero.");

        var product = new Product(name, price, user, timezoneId);

        activity?.SetTag(nameof(name), name);
        activity?.SetTag(nameof(price), price);

        return Result.Ok(product);
    });

    public Result Update(
        string name,
        decimal price,
        string user = "System",
        string? timezoneId = null
    ) => Handle(activity =>
    {
        if (IsDeleted)
            return Result.Fail("Cannot update a deleted product.");

        if (string.IsNullOrWhiteSpace(name))
            return Result.Fail("Product name is required.");

        if (price <= 0)
            return Result.Fail("Product price must be greater than zero.");

        Name = name;
        Price = price;

        var updateResult = Update(user, timezoneId);
        if (updateResult.IsFailure)
            return updateResult;

        activity?.SetTag(nameof(name), name);

        return Result.Ok();
    });
}
```

### Pattern B — Child Entity / Value Object (constructor validation + `DomainException`)

Use this for **child entities** or simpler domain objects that belong to an aggregate (e.g., line items, embedded sub-objects). Validation throws `DomainException` directly.

**Example**: `Item`, `Address`, `PhoneNumber`.

```csharp
using Domain.Common;
using Domain.Common.Exceptions;

namespace Domain.Products;

public sealed class ProductVariant : DomainEntity
{
    public ProductVariant() { }  // Required for EF Core

    public ProductVariant(
        string sku,
        decimal additionalCost,
        string? createdBy = null,
        string? timezoneId = null
    ) : base(createdBy ?? "System", timezoneId)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new DomainException("SKU is required.");

        if (additionalCost < 0)
            throw new DomainException("Additional cost cannot be negative.");

        Sku = sku;
        AdditionalCost = additionalCost;
    }

    public string Sku { get; private set; }
    public decimal AdditionalCost { get; private set; }
}
```

---

## Step-by-Step Guide

### Step 1 — Determine the Pattern

| Criteria | Pattern A (aggregate root) | Pattern B (child entity) |
|----------|---------------------------|--------------------------|
| Is it the root of a bounded context? | ✅ | |
| Does it have complex business workflows? | ✅ | |
| Does another entity own its lifecycle? | | ✅ |
| Is it a simple value container with basic validation? | | ✅ |

### Step 2 — Create the Entity File

File path: `src/Domain/{PluralContext}/{EntityName}.cs`

Example: `src/Domain/Products/Product.cs`

Checklist:
- [ ] `public sealed class` extending `DomainEntity`
- [ ] `public EntityName() { }` — parameterless constructor for EF Core
- [ ] Private constructor (Pattern A) OR public constructor (Pattern B)
- [ ] All properties with `private set` (use `init` only for base-class-assigned properties)
- [ ] Static `Result<TEntity> Create(...)` method using `Handle(activity => ...)` (Pattern A)
- [ ] Instance operation methods returning `Result` using `Handle(activity => ...)` (Pattern A)
- [ ] `DomainException` thrown for violations in constructors (Pattern B)
- [ ] `activity?.SetTag(nameof(property), value)` for meaningful telemetry tags
- [ ] Call `Update(user, timezoneId)` from `DomainEntity` base when modifying audit fields

### Step 3 — Add Enums (if needed)

File path: `src/Domain/Common/Enums/{EnumName}.cs`

```csharp
namespace Domain.Common.Enums;

public enum ProductCategory
{
    Electronics,
    Clothing,
    Food
}
```

### Step 4 — Register EF Core Configuration (Infrastructure layer)

If the entity needs persistence, add its EF Core configuration in the Infrastructure layer. Check existing patterns in `src/Infrastructure/Data/`.

### Step 5 — Write Unit Tests

File path: `tests/UnitTests/Domain/{EntityName}Tests.cs`

---

## Unit Test Pattern

### Rules
- Framework: **xUnit**
- Naming: `GivenContext_WhenCondition_ThenExpectedResult`
  Examples: `GivenANewProductWhenValidPriceThenShouldCreateWithSuccess`
- Always use `[Fact(DisplayName = nameof(MethodName))]`
- Sections: `// Arrange`, `// Act`, `// Assert` (combine Arrange+Act when trivial)
- Group tests in nested `sealed` classes by method under test

### Test Structure

```csharp
using Domain.Common.Exceptions;
using Domain.Products;

namespace UnitTests.Domain;

public sealed class ProductTests
{
    // ── Create ────────────────────────────────────────────────────────────────

    [Fact(DisplayName = nameof(GivenANewProductWhenValidInputThenShouldCreateWithSuccess))]
    public void GivenANewProductWhenValidInputThenShouldCreateWithSuccess()
    {
        // Arrange, Act
        var result = Product.Create("Laptop", 999.99m, "John Doe", "America/New_York");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Value);
        Assert.Equal("Laptop", result.Value.Name);
        Assert.Equal(999.99m, result.Value.Price);
        Assert.Equal("John Doe", result.Value.CreatedBy);
        Assert.Equal("America/New_York", result.Value.CreatedByTimezoneId);
    }

    [Fact(DisplayName = nameof(GivenANewProductWhenNoUserProvidedThenShouldDefaultToSystem))]
    public void GivenANewProductWhenNoUserProvidedThenShouldDefaultToSystem()
    {
        // Arrange, Act
        var result = Product.Create("Laptop", 999.99m);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("System", result.Value.CreatedBy);
        Assert.Equal("UTC", result.Value.CreatedByTimezoneId);
    }

    [Fact(DisplayName = nameof(GivenANewProductWhenNameIsEmptyThenShouldReturnFailure))]
    public void GivenANewProductWhenNameIsEmptyThenShouldReturnFailure()
    {
        // Arrange, Act
        var result = Product.Create("", 999.99m);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Product name is required.", result.Message);
    }

    [Fact(DisplayName = nameof(GivenANewProductWhenPriceIsZeroThenShouldReturnFailure))]
    public void GivenANewProductWhenPriceIsZeroThenShouldReturnFailure()
    {
        // Arrange, Act
        var result = Product.Create("Laptop", 0m);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Product price must be greater than zero.", result.Message);
    }

    // ── Update ─────────────────────────────────────────────────────────

    [Fact(DisplayName = nameof(GivenAnExistingProductWhenValidUpdateThenShouldUpdateWithSuccess))]
    public void GivenAnExistingProductWhenValidUpdateThenShouldUpdateWithSuccess()
    {
        // Arrange
        var product = Product.Create("Laptop", 999.99m).Value;

        // Act
        var result = product.Update("Gaming Laptop", 1499.99m, "Jane Doe");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Gaming Laptop", product.Name);
        Assert.Equal(1499.99m, product.Price);
    }

    [Fact(DisplayName = nameof(GivenADeletedProductWhenUpdateIsCalledThenShouldReturnFailure))]
    public void GivenADeletedProductWhenUpdateIsCalledThenShouldReturnFailure()
    {
        // Arrange
        var product = Product.Create("Laptop", 999.99m).Value;
        product.Delete();

        // Act
        var result = product.Update("Gaming Laptop", 1499.99m);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Cannot update a deleted product.", result.Message);
    }
}
```

### Tests for Pattern B (constructor + `DomainException`)

```csharp
[Fact(DisplayName = nameof(GivenANewVariantWhenSkuIsEmptyThenShouldThrowDomainException))]
public void GivenANewVariantWhenSkuIsEmptyThenShouldThrowDomainException()
{
    // Arrange, Act
    var exception = Assert.Throws<DomainException>(() => new ProductVariant("", 10m));

    // Assert
    Assert.Equal("SKU is required.", exception.Message);
}

[Fact(DisplayName = nameof(GivenANewVariantWhenNegativeCostThenShouldThrowDomainException))]
public void GivenANewVariantWhenNegativeCostThenShouldThrowDomainException()
{
    // Arrange, Act
    var exception = Assert.Throws<DomainException>(() => new ProductVariant("SKU-001", -5m));

    // Assert
    Assert.Equal("Additional cost cannot be negative.", exception.Message);
}
```

---

## Tips

### Soft Delete
Always check and set `IsDeleted` guard in update methods before performing changes:
```csharp
if (IsDeleted)
    return Result.Fail("Cannot update a deleted entity.");
```

### Decimal for Monetary Values
Use `decimal` — never `double` or `float` — for prices, amounts, and totals.

### Enums
- Declare enums in `src/Domain/Common/Enums/`
- Namespace: `Domain.Common.Enums`
- Reference in entity: `using Domain.Common.Enums;`

### Telemetry Tags
Set meaningful tags on `activity` to aid distributed tracing. Prefer business identifiers:
```csharp
activity?.SetTag(nameof(name), name);
activity?.SetTag(nameof(price), price);
```
Avoid tagging sensitive personal or financial data.

### `Update()` from Base Class
Call `DomainEntity.Update(user, timezoneId)` at the end of every mutating method to keep audit fields (`UpdatedAt`, `UpdatedBy`, `UpdatedByTimezoneId`) in sync:
```csharp
var updateResult = Update(user, timezoneId);
if (updateResult.IsFailure)
    return updateResult;
```

### ActivitySource in Constructors (when not using `Handle()`)
When a constructor needs telemetry but cannot use `Handle()` (e.g., Pattern B constructors), start an activity manually as seen in `Notification.cs`:
```csharp
using var activity = ActivitySource.StartActivity($"{GetType().Name}.Constructor");
activity.SetDefaultTags();
```

### Domain DI Registration
The `Domain` layer typically has no DI registrations beyond what is added by EF Core configurations in `Infrastructure`. If you add a domain service, register it in `src/Domain/DomainDependencyInjection.cs`.

---

## Verification Checklist

Before delivering generated entity code, confirm:

- [ ] Entity is `public sealed class` extending `DomainEntity`
- [ ] Parameterless constructor `public EntityName() { }` present for EF Core
- [ ] All public properties have `private set`
- [ ] Validation returns `Result.Fail<T>(message)` (Pattern A) or throws `DomainException` (Pattern B)
- [ ] `Update(user, timezoneId)` called from all mutating methods (Pattern A)
- [ ] `ActivitySource` telemetry tags added for meaningful fields
- [ ] `decimal` used for all monetary values
- [ ] Enums placed in `src/Domain/Common/Enums/`
- [ ] Unit tests follow `GivenContext_WhenCondition_ThenExpectedResult` naming
- [ ] All tests use `[Fact(DisplayName = nameof(MethodName))]`
- [ ] Tests cover: success path, failure path, edge cases, already-deleted guard
