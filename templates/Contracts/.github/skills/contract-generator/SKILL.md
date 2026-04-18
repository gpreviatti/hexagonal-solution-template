---
title: Contract Generator
description: Generate C# contract records, Proto files, and their corresponding unit tests following Hexagonal Architecture patterns
category: Code Generation
tags: [contracts, codegen, grpc, dto, records, hexagonal-architecture]
---

# Contract Generator Skill

This skill guides the generation of shared contracts (C# records and Proto files) and their corresponding unit tests for Hexagonal Architecture templates using modern C# features and best practices.

## Overview

When to use this skill:
- **Creating new C# request/response contracts** (records, DTOs, value objects)
- **Adding new Proto files for gRPC services**
- **Generating corresponding unit tests** for contract instantiation and validation
- **Extending existing contract domains** (Orders, Payments, Users, etc.)

## Contract Generation Patterns

### 1. Base Contracts

Base contracts provide foundation for all requests and responses.

#### BaseRequest Record

**Location**: `src/Contracts/Common/BaseRequest.cs`

```csharp
namespace Contracts.Common;

/// <summary>
/// Base request structure
/// </summary>
/// <param name="CorrelationId">The unique identifier for correlating requests</param>
public record BaseRequest(Guid CorrelationId);

/// <summary>
/// Base paginated request structure
/// </summary>
/// <param name="CorrelationId">The unique identifier for correlating requests</param>
/// <param name="Page">The page number to retrieve</param>
/// <param name="PageSize">The number of items per page</param>
/// <param name="SortBy">The field to sort by</param>
/// <param name="SortDescending">Indicates whether the sorting is in descending order</param>
/// <param name="SearchByValues">A dictionary of search criteria</param>
public record BasePaginatedRequest(
    Guid CorrelationId,
    int Page = 1,
    int PageSize = 10,
    string? SortBy = null,
    bool SortDescending = false,
    Dictionary<string, string>? SearchByValues = null
) : BaseRequest(CorrelationId);
```

#### BaseResponse Record

**Location**: `src/Contracts/Common/BaseResponse.cs`

```csharp
namespace Contracts.Common;

/// <summary>
/// Base response structure
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful</param>
/// <param name="Message">Additional message about the operation</param>
public record BaseResponse(bool IsSuccess, string Message = "");

/// <summary>
/// Base paginated response structure
/// </summary>
/// <typeparam name="T">The type of items in the response</typeparam>
/// <param name="IsSuccess">Indicates if the operation was successful</param>
/// <param name="Items">The collection of items</param>
/// <param name="TotalCount">Total number of items available</param>
/// <param name="Message">Additional message about the operation</param>
public record BasePaginatedResponse<T>(
    bool IsSuccess,
    List<T> Items,
    int TotalCount = 0,
    string Message = ""
) : BaseResponse(IsSuccess, Message);
```

### 2. Domain-Specific Contracts

Create contracts for each domain/feature area in dedicated folders.

#### File Structure
```
src/Contracts/
  ├── Common/
  │   ├── BaseRequest.cs
  │   └── BaseResponse.cs
  ├── [DomainName]/
  │   ├── Create[Entity]Request.cs
  │   ├── Update[Entity]Request.cs
  │   ├── Get[Entity]Request.cs
  │   ├── [Entity]Dto.cs
  │   ├── [Entity]Response.cs
  │   └── [Entity]ListResponse.cs
  └── Protos/
      └── [service].proto
```

#### Create Request Pattern

**Location**: `src/Contracts/[DomainName]/Create[Entity]Request.cs`

```csharp
using Contracts.Common;

namespace Contracts.[DomainName];

/// <summary>
/// Request to create a new [Entity]
/// </summary>
/// <param name="CorrelationId">The unique identifier for the request</param>
/// <param name="PropertyName">Description of the property</param>
public sealed record Create[Entity]Request(
    Guid CorrelationId,
    string PropertyName,
    // Additional properties
) : BaseRequest(CorrelationId);

/// <summary>
/// Represents a single item in the [Entity] request
/// </summary>
/// <param name="PropertyName">Description of the property</param>
public sealed record [Entity]ItemRequest(
    string PropertyName,
    // Additional properties
);
```

**Key Points**:
- Use `sealed record` to prevent inheritance
- Inherit from `BaseRequest` to include correlation ID
- Add comprehensive XML documentation with `<summary>` and `<param>` tags
- Use PascalCase for property names (C# naming conventions)
- Use `sealed` keyword for DTOs that shouldn't be inherited

#### DTO Pattern

**Location**: `src/Contracts/[DomainName]/[Entity]Dto.cs`

```csharp
namespace Contracts.[DomainName];

/// <summary>
/// Data transfer object representing a [Entity]
/// </summary>
/// <param name="Id">The unique identifier</param>
/// <param name="PropertyName">Description of the property</param>
public sealed record [Entity]Dto(
    Guid Id,
    string PropertyName,
    // Additional properties
);
```

#### Response Pattern

**Location**: `src/Contracts/[DomainName]/[Entity]Response.cs`

```csharp
using Contracts.Common;

namespace Contracts.[DomainName];

/// <summary>
/// Response containing a [Entity] data transfer object
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful</param>
/// <param name="Item">The [Entity] data transfer object</param>
/// <param name="Message">Additional message about the operation</param>
public sealed record [Entity]Response(
    bool IsSuccess,
    [Entity]Dto? Item = null,
    string Message = ""
) : BaseResponse(IsSuccess, Message);

/// <summary>
/// Response containing a paginated list of [Entity] objects
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful</param>
/// <param name="Items">The collection of [Entity] data transfer objects</param>
/// <param name="TotalCount">Total number of items available</param>
/// <param name="Message">Additional message about the operation</param>
public sealed record [Entity]ListResponse(
    bool IsSuccess,
    List<[Entity]Dto> Items,
    int TotalCount = 0,
    string Message = ""
) : BasePaginatedResponse<[Entity]Dto>(IsSuccess, Items, TotalCount, Message);
```

### 3. Proto File Generation

Create Proto files for gRPC service contracts.

#### File Structure
```
src/Contracts/Protos/
  └── [service].proto
```

#### Proto Template

**Location**: `src/Contracts/Protos/[service].proto`

```protobuf
syntax = "proto3";

option csharp_namespace = "Grpc[ServiceName]";

package Protos;

// Service for managing [entities]
service [Service]Service {
    // Creates a new [entity]
    rpc Create ([Create][Entity]Request) returns ([Entity]Reply);
    // Retrieves a [entity] by ID
    rpc Get (Get[Entity]Request) returns ([Entity]Reply);
    // Lists all [entities]
    rpc List (List[Entity]Request) returns (stream [Entity]Reply);
}

// Request message for creating a [entity]
message Create[Entity]Request {
    // Correlation ID for tracking requests
    string correlation_id = 1;
    // Property description
    string property_name = 2;
    // Additional properties
}

// Request message for retrieving a [entity]
message Get[Entity]Request {
    // Unique identifier
    int32 id = 1;
    // Correlation ID for tracking requests
    string correlation_id = 2;
}

// Request message for listing [entities]
message List[Entity]Request {
    // Page number (1-based)
    int32 page = 1;
    // Number of items per page
    int32 page_size = 2;
    // Correlation ID for tracking requests
    string correlation_id = 3;
}

// Reply message for [entity] operations
message [Entity]Reply {
    // Indicates if the operation was successful
    bool success = 1;
    // Unique identifier
    int32 id = 2;
    // Property description
    string property_name = 3;
    // Message providing additional information
    string message = 4;
}
```

**Key Points**:
- Use `proto3` syntax for modern Protocol Buffers
- Set `csharp_namespace` to match C# namespace conventions
- Use snake_case for Proto field names (Proto convention)
- Include correlation_id in all request messages for request tracing
- Add comment documentation above each message and RPC method
- Use appropriate field numbers (protocol buffers requirement)

## Unit Test Generation

Create corresponding unit tests to validate contract instantiation and property assignment.

### Unit Test File Structure
```
tests/UnitTests/
  ├── Common/
  │   ├── BaseRequestTests.cs
  │   └── BaseResponseTests.cs
  └── [DomainName]/
      └── Create[Entity]RequestTests.cs
```

### Unit Test Pattern

**Location**: `tests/UnitTests/[DomainName]/[Contract]Tests.cs`

```csharp
using Contracts.[DomainName];

namespace UnitTests.[DomainName];

public sealed class [Contract]Tests
{
    [Fact(DisplayName = nameof(GivenA[Entity]ItemRequestWhenInstantiatedThenShouldAssignProperties))]
    public void GivenA[Entity]ItemRequestWhenInstantiatedThenShouldAssignProperties()
    {
        // Arrange
        var itemName = "Item 1";
        var itemDescription = "Description 1";
        var itemValue = 120.5m;

        // Act
        var request = new [Entity]ItemRequest(itemName, itemDescription, itemValue);

        // Assert
        Assert.Equal(itemName, request.PropertyName);
        Assert.Equal(itemDescription, request.Description);
        Assert.Equal(itemValue, request.Value);
    }

    [Fact(DisplayName = nameof(GivenACreate[Entity]RequestWhenInstantiatedThenShouldAssignProperties))]
    public void GivenACreate[Entity]RequestWhenInstantiatedThenShouldAssignProperties()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var description = "Request description";
        [Entity]ItemRequest[] items =
        [
            new("Item 1", "Description 1", 100m),
            new("Item 2", "Description 2", 200m)
        ];

        // Act
        var request = new Create[Entity]Request(correlationId, description, items);

        // Assert
        Assert.Equal(correlationId, request.CorrelationId);
        Assert.Equal(description, request.Description);
        Assert.Equal(items, request.Items);
    }

    [Theory(DisplayName = nameof(GivenACreate[Entity]RequestWhenInstantiatedWithVariousValuesThenShouldAssignAllProperties))]
    [InlineData("Test Description", 100)]
    [InlineData("Another Description", 200)]
    public void GivenACreate[Entity]RequestWhenInstantiatedWithVariousValuesThenShouldAssignAllProperties(
        string description, decimal totalValue)
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        [Entity]ItemRequest[] items = [new("Item", "Desc", totalValue)];

        // Act
        var request = new Create[Entity]Request(correlationId, description, items);

        // Assert
        Assert.NotNull(request);
        Assert.Equal(correlationId, request.CorrelationId);
        Assert.Equal(description, request.Description);
    }
}
```

### Test Naming Convention

Follow the BDD-style naming pattern: `Given[Condition]When[Action]Then[ExpectedResult]`

**Examples**:
- `GivenACreateOrderRequestWhenInstantiatedThenShouldAssignProperties`
- `GivenABasePaginatedRequestWhenUsingDefaultValuesThenShouldAssignExpectedDefaults`
- `GivenABasePaginatedRequestWhenInstantiatedWithCustomValuesThenShouldAssignCustomValues`

### Test Best Practices

1. **Use `[Fact]` for single test cases** - One scenario, one assertion focus
2. **Use `[Theory]` with `[InlineData]` for multiple scenarios** - Test parameter variations
3. **Include `DisplayName`** - Makes test explorer more readable
4. **Use `sealed class`** - Prevent test inheritance
5. **Follow Arrange-Act-Assert pattern** - Clear test structure (comments optional for simple tests)
6. **Test default values** - Verify default parameters work correctly
7. **Test with various data types** - Use different values (empty strings, decimals, collections)
8. **Seal records** - Use `sealed record` to prevent inheritance

## Step-by-Step: Adding a New Contract

### Step 1: Define the Contract Domain Structure

Create a new folder under `src/Contracts/[DomainName]/` with:
- `Create[Entity]Request.cs` - Request to create
- `Update[Entity]Request.cs` - Request to update (optional)
- `[Entity]Dto.cs` - Data transfer object
- `[Entity]Response.cs` - Response wrapper

### Step 2: Implement Contract Records

1. **Create the request record** - Inherit from `BaseRequest`, use `sealed record`
2. **Add XML documentation** - `<summary>` and `<param>` tags for all properties
3. **Use proper C# types** - `decimal` for money, `Guid` for IDs, `DateTime` for timestamps
4. **Include collections with `[]` initializer** - Modern C# syntax

### Step 3: Create Unit Tests

1. **Create test file** - `tests/UnitTests/[DomainName]/Create[Entity]RequestTests.cs`
2. **Test instantiation** - Verify properties are assigned correctly
3. **Test default values** - For optional properties
4. **Test with collections** - If records contain arrays or lists
5. **Use `[Theory]` for multiple scenarios** - Validate edge cases

### Step 4: Add Proto File (If gRPC Required)

1. **Create proto file** - `src/Contracts/Protos/[service].proto`
2. **Define service with RPC methods** - Create, Get, List operations
3. **Define request/reply messages** - Match C# contracts where applicable
4. **Add correlation IDs** - For distributed tracing

### Step 5: Build and Validate

```bash
# Restore and build
dotnet restore
dotnet build src/Contracts

# Generate gRPC code
# (Automatically done during build if Protobuf is configured)

# Run tests
dotnet test tests/UnitTests
```

## Common Patterns

### Creating Request Records with Default Values

```csharp
public record BasePaginatedRequest(
    Guid CorrelationId,
    int Page = 1,                                   // Default value
    int PageSize = 10,                              // Default value
    string? SortBy = null,                          // Nullable with default
    bool SortDescending = false,                    // Boolean default
    Dictionary<string, string>? SearchByValues = null
) : BaseRequest(CorrelationId);
```

### Nested Request Objects

```csharp
public sealed record CreateOrderRequest(
    Guid CorrelationId,
    string Description,
    CreateOrderItemRequest[] Items  // Array of nested records
) : BaseRequest(CorrelationId);

public sealed record CreateOrderItemRequest(
    string Name,
    string Description,
    decimal Value
);
```

### Response with Generic Data

```csharp
public record BasePaginatedResponse<T>(
    bool IsSuccess,
    List<T> Items,
    int TotalCount = 0,
    string Message = ""
) : BaseResponse(IsSuccess, Message);
```

## C# Modern Features Used

- **File-scoped namespaces**: `namespace Contracts.Orders;` (compact, modern)
- **Records**: Immutable by default, perfect for contracts
- **Sealed records**: Prevent inheritance, optimize performance
- **Nullable reference types**: `string?` for optional properties
- **Collection expressions**: `[]` for empty arrays, collection initializers
- **XML documentation**: Triple-slash comments for API documentation

## Validation Tips

1. **Run the build** - Ensure no compilation errors
2. **Run unit tests** - All tests should pass
3. **Generate documentation** - Via `dotnet build /p:GenerateDocumentationFile=true`
4. **Check Proto compilation** - Verify gRPC code generation
5. **Review naming consistency** - Ensure ubiquitous language alignment

## Related Files

- `src/Contracts/Contracts.csproj` - Project configuration (enables gRPC Protobuf processing)
- `Directory.Packages.props` - Centralized dependency management
- `.editorconfig` - Code style enforcement
- `tests/UnitTests/GlobalUsings.cs` - Global using statements for tests

## References

### Official Documentation

- **[Protocol Buffers (proto3)](https://protobuf.dev/)** - Protocol Buffers language guide and syntax reference
- **[gRPC C# Documentation](https://grpc.io/docs/languages/csharp/)** - gRPC service implementation and best practices
- **[C# Records Documentation](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record)** - Records syntax, features, and patterns
- **[C# Nullable Reference Types](https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references)** - Null-safety features and usage
- **[XUnit Documentation](https://xunit.net/)** - xUnit testing framework and best practices

### Architecture & Patterns

- **[Hexagonal Architecture (Ports & Adapters)](https://en.wikipedia.org/wiki/Hexagonal_architecture)** - Architectural pattern overview
- **[Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)** - DDD concepts and practices
- **[Value Objects](https://martinfowler.com/bliki/ValueObject.html)** - Value object pattern in domain modeling
- **[Bounded Contexts](https://martinfowler.com/bliki/BoundedContext.html)** - Separating problem domains

### Related Skills

- **[Contract Generator Skill](.)** - This skill for generating contracts, DTOs, and tests
- **[Service Generator Skill](../../service-generator/)** - Generating application services
- **[Unit Test Generator Skill](../../unit-test-generator/)** - Comprehensive test generation

### .NET Best Practices

- **[Async/Await Best Practices](https://learn.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming)** - Asynchronous programming patterns
- **[Dependency Injection in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)** - Built-in DI container usage
- **[SOLID Principles](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/)** - Design principles for maintainable code

### Code Style & Conventions

- **[C# Naming Conventions](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/naming-guidelines)** - Official .NET naming guidelines
- **[EditorConfig Documentation](https://editorconfig.org/)** - Cross-editor code style enforcement
- **[Google C# Style Guide](https://google.github.io/styleguide/csharp-style.html)** - Comprehensive C# style conventions
