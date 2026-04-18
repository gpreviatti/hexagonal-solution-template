# Hexagonal Architecture Contracts Template

This repository provides a focused contracts template for applications using Hexagonal Architecture. The template includes shared request/response contracts, DTOs, and gRPC contract definitions with unit tests to validate contract instantiation and behavior.

## Table of Contents

- [Project Overview](#project-overview)
  - [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Core Concepts](#core-concepts)
  - [Base Contracts Layer](#base-contracts-layer-srccontractscommon)
  - [Domain-Specific Contracts](#domain-specific-contracts)
  - [gRPC Contracts](#grpc-contracts-srccontractsprotos)
- [Unit Tests](#unit-tests-testsunittests)
  - [Test Structure](#test-structure)
  - [Test Categories](#test-categories)
- [Building and Testing](#building-and-testing)
  - [Prerequisites](#prerequisites)
  - [Restore Dependencies](#restore-dependencies)
  - [Build Contracts Project](#build-contracts-project)
  - [Run Unit Tests](#run-unit-tests)
  - [Build Release Configuration](#build-release-configuration)
- [Adding New Contracts](#adding-new-contracts)
- [Best Practices](#best-practices)
  - [C# Contract Design](#c-contract-design)
  - [Test Design](#test-design)
  - [gRPC Contract Design](#grpc-contract-design)
- [Troubleshooting](#troubleshooting)
- [Quick Reference](#quick-reference)
- [Guidelines](#guidelines)
- [Contributing](#-contributing)

## Project Overview

The Contracts project serves as the communication layer between your Hexagonal Architecture domains. It defines:
- **Request/Response DTOs** - Immutable records for service contracts
- **Domain Models** - Transfer objects representing domain entities
- **gRPC Contracts** - Protocol Buffer definitions for inter-service communication
- **Comprehensive Tests** - Unit tests validating contract instantiation and property assignment

### Technology Stack

- **Language**: C# 13 with modern features (records, sealed types, nullable reference types)
- **Testing Framework**: XUnit with BDD-style naming conventions
- **Serialization**: gRPC Protocol Buffers (proto3)
- **Target Framework**: .NET 10.0

## Project Structure

```
.
├── Directory.Build.props              # Centralized build properties
├── Directory.Packages.props            # Centralized package versions
├── Hexagonal.Solution.Template.Contracts.slnx  # Solution file
├── Readme.md                           # This file
│
├── src/
│   └── Contracts/
│       ├── Contracts.csproj            # Main contracts project
│       ├── Common/
│       │   ├── BaseRequest.cs          # Base request record
│       │   └── BaseResponse.cs         # Base response record
│       ├── Orders/
│       │   ├── CreateOrderRequest.cs   # Order creation request
│       │   └── OrderDto.cs             # Order data transfer object
│       ├── Protos/
│       │   └── payment.proto           # gRPC service definition
│       └── bin/
│           ├── Debug/                  # Debug build output
│           └── Release/                # Release build output
│
└── tests/
    └── UnitTests/
        ├── UnitTests.csproj            # Test project
        ├── GlobalUsings.cs             # Global using statements
        ├── Common/
        │   ├── BaseRequestTests.cs     # Base request tests
        │   └── BaseResponseTests.cs    # Base response tests
        ├── Orders/
        │   ├── CreateOrderRequestTests.cs
        │   └── OrderDtoTests.cs
        └── bin/                        # Test build output
```

## Core Concepts

### Base Contracts Layer (`src/Contracts/Common/`)

All domain contracts inherit from base records providing common functionality:

- **`BaseRequest`** - Common request properties (CorrelationId for tracing)
- **`BasePaginatedRequest`** - Pagination support (Page, PageSize, SortBy, SearchByValues)
- **`BaseResponse`** - Common response properties (IsSuccess, Message)
- **`BasePaginatedResponse<T>`** - Generic paginated responses (Items, TotalCount)

**Example:**
```csharp
// Inherits CorrelationId from BaseRequest
public sealed record CreateOrderRequest(
    Guid CorrelationId,
    string Description,
    CreateOrderItemRequest[] Items
) : BaseRequest(CorrelationId);
```

### Domain-Specific Contracts

Each domain (Orders, Payments, Users, etc.) has its own namespace with standardized contracts:

#### Request Types
- **`Create[Entity]Request`** - Request to create a new entity
- **`Update[Entity]Request`** - Request to update an existing entity
- **`Get[Entity]Request`** - Request to retrieve a specific entity

#### Data Transfer Objects (DTOs)
- **`[Entity]Dto`** - Represents domain entity for external consumption
- Sealed records for immutability and security
- Minimal properties (no internal domain logic exposed)

#### Response Types
- **`[Entity]Response`** - Single item response with optional DTO
- **`[Entity]ListResponse`** - Paginated list response with multiple DTOs
- Inherit from base response types for consistency

**Example:**
```csharp
public sealed record OrderDto(
    Guid Id,
    string Description,
    decimal TotalValue
);

public sealed record OrderResponse(
    bool IsSuccess,
    OrderDto? Item = null,
    string Message = ""
) : BaseResponse(IsSuccess, Message);
```

### gRPC Contracts (`src/Contracts/Protos/`)

Protocol Buffer (protobuf) files define gRPC service contracts for inter-service communication.

**Key Features:**
- **Service Definition** - RPC methods (Create, Get, List, Delete, etc.)
- **Message Types** - Request and reply structures with typed fields
- **Language Support** - Compiled to C# during build
- **Type Safety** - Strong typing with proto3 syntax

**Example:**
```protobuf
service PaymentService {
    rpc Create (CreatePaymentRequest) returns (PaymentReply);
}

message CreatePaymentRequest {
    int32 order_id = 1;
    double amount = 2;
    string correlation_id = 3;
}

message PaymentReply {
    bool success = 1;
    string message = 2;
}
```

## Unit Tests (`tests/UnitTests/`)

Comprehensive test coverage validates contract correctness using XUnit and BDD-style naming.

### Test Structure

Tests follow the pattern: `Given[Condition]When[Action]Then[ExpectedResult]`

```csharp
[Fact(DisplayName = nameof(GivenACreateOrderRequestWhenInstantiatedThenShouldAssignProperties))]
public void GivenACreateOrderRequestWhenInstantiatedThenShouldAssignProperties()
{
    var correlationId = Guid.NewGuid();
    CreateOrderItemRequest[] items = [new("Item 1", "Desc 1", 100m)];

    var request = new CreateOrderRequest(correlationId, "Order desc", items);

    Assert.Equal(correlationId, request.CorrelationId);
    Assert.Equal("Order desc", request.Description);
    Assert.NotEmpty(request.Items);
}
```

### Test Categories

1. **Instantiation Tests** - Verify record properties are assigned correctly
2. **Default Value Tests** - Validate optional parameters use correct defaults
3. **Collection Tests** - Test nested records and array handling
4. **Theory Tests** - Multiple scenarios using `[Theory]` and `[InlineData]`

## Building and Testing

### Prerequisites

- .NET 10.0 SDK or later
- C# 13 support

### Restore Dependencies

Install all NuGet packages defined in `Directory.Packages.props`:

```bash
dotnet restore
```

### Build Contracts Project

Compile the contracts library (includes gRPC code generation):

```bash
dotnet build src/Contracts
```

**Build Process:**
1. Restores dependencies (if needed)
2. Compiles C# records
3. Generates gRPC service classes from `.proto` files
4. Generates XML documentation (if enabled)

### Run Unit Tests

Execute all unit tests with detailed output:

```bash
dotnet test tests/UnitTests
```

**Test Options:**
```bash
# Run with verbose output
dotnet test tests/UnitTests -v normal

# Run specific test class
dotnet test tests/UnitTests --filter "FullyQualifiedName~OrderDtoTests"

# Run with code coverage
dotnet test tests/UnitTests /p:CollectCoverage=true
```

### Build Release Configuration

Create optimized release build:

```bash
dotnet build src/Contracts -c Release
```

## Adding New Contracts

### Step 1: Create Contract Folder Structure

```bash
# Create domain folder
mkdir src/Contracts/[YourDomain]

# Create test folder
mkdir tests/UnitTests/[YourDomain]
```

### Step 2: Implement Request Record

Create `src/Contracts/[YourDomain]/Create[Entity]Request.cs`:

```csharp
using Contracts.Common;

namespace Contracts.[YourDomain];

/// <summary>
/// Request to create a new [Entity]
/// </summary>
/// <param name="CorrelationId">Unique request identifier for tracing</param>
/// <param name="PropertyName">Description of property</param>
public sealed record Create[Entity]Request(
    Guid CorrelationId,
    string PropertyName
) : BaseRequest(CorrelationId);
```

### Step 3: Implement DTO Record

Create `src/Contracts/[YourDomain]/[Entity]Dto.cs`:

```csharp
namespace Contracts.[YourDomain];

/// <summary>
/// Data transfer object representing [Entity]
/// </summary>
/// <param name="Id">Unique identifier</param>
/// <param name="PropertyName">Description of property</param>
public sealed record [Entity]Dto(
    Guid Id,
    string PropertyName
);
```

### Step 4: Implement Response Record

Create `src/Contracts/[YourDomain]/[Entity]Response.cs`:

```csharp
using Contracts.Common;

namespace Contracts.[YourDomain];

/// <summary>
/// Response containing [Entity] details
/// </summary>
public sealed record [Entity]Response(
    bool IsSuccess,
    [Entity]Dto? Item = null,
    string Message = ""
) : BaseResponse(IsSuccess, Message);
```

### Step 5: Create Unit Tests

Create `tests/UnitTests/[YourDomain]/Create[Entity]RequestTests.cs`:

```csharp
using Contracts.[YourDomain];

namespace UnitTests.[YourDomain];

public sealed class Create[Entity]RequestTests
{
    [Fact(DisplayName = nameof(GivenACreate[Entity]RequestWhenInstantiatedThenShouldAssignProperties))]
    public void GivenACreate[Entity]RequestWhenInstantiatedThenShouldAssignProperties()
    {
        var correlationId = Guid.NewGuid();

        var request = new Create[Entity]Request(correlationId, "Test");

        Assert.Equal(correlationId, request.CorrelationId);
        Assert.Equal("Test", request.PropertyName);
    }
}
```

### Step 6: (Optional) Create Proto File

Create `src/Contracts/Protos/[service].proto`:

```protobuf
syntax = "proto3";

option csharp_namespace = "Grpc[Service]";

package Protos;

service [Service]Service {
    rpc Create ([Entity]Request) returns ([Entity]Reply);
}

message [Entity]Request {
    string correlation_id = 1;
    string property_name = 2;
}

message [Entity]Reply {
    bool success = 1;
    string message = 2;
}
```

### Step 7: Build and Test

```bash
dotnet build src/Contracts
dotnet test tests/UnitTests
```

## Best Practices

### C# Contract Design

✅ **DO:**
- Use `sealed record` for immutability and performance
- Inherit from `BaseRequest`/`BaseResponse` for consistency
- Add comprehensive XML documentation (`/// <summary>`)
- Use nullable reference types (`string?`) for optional properties
- Use `Guid` for unique identifiers
- Use `decimal` for monetary values
- Use collection expressions: `[]` for empty arrays

❌ **DON'T:**
- Use mutable classes for contracts (use records)
- Expose internal domain logic in DTOs
- Include timestamps without timezone information
- Use `string` for strongly-typed values (prefer enums where appropriate)

### Test Design

✅ **DO:**
- Use BDD-style test names (`Given...When...Then...`)
- Test property assignments explicitly
- Test default values for optional parameters
- Use `[Theory]` with `[InlineData]` for multiple scenarios
- Use `[Fact]` for single scenarios

❌ **DON'T:**
- Create overly complex test cases
- Test framework behavior (only test your contracts)
- Use mutable test data

### gRPC Contract Design

✅ **DO:**
- Use `proto3` syntax
- Include `correlation_id` for distributed tracing
- Document RPC methods and messages with comments
- Use snake_case for proto field names
- Use appropriate field numbers (1, 2, 3, ...)

❌ **DON'T:**
- Skip correlation IDs in requests
- Reuse field numbers
- Mix naming conventions (use snake_case in proto, PascalCase in C#)

## Troubleshooting

### gRPC Code Not Generated

**Issue**: Proto files compile but corresponding C# files not generated

**Solution**:
1. Verify `Contracts.csproj` contains Protobuf configuration:
   ```xml
   <ItemGroup>
       <Protobuf Include="Protos\*" />
   </ItemGroup>
   ```
2. Rebuild project: `dotnet clean && dotnet build`
3. Check `obj/Debug/net10.0/Protos/` for generated files

### Test Failures After Adding Records

**Issue**: New tests fail with "type not found"

**Solution**:
1. Ensure namespace matches: `namespace Contracts.[DomainName];`
2. Verify test project imports: `using Contracts.[DomainName];`
3. Rebuild solution: `dotnet build`
4. Run: `dotnet test`

### Documentation Not Generated

**Issue**: XML documentation file (.xml) not created

**Solution**:
1. Verify `Contracts.csproj` has: `<GenerateDocumentationFile>true</GenerateDocumentationFile>`
2. Build with documentation generation:
   ```bash
   dotnet build /p:GenerateDocumentationFile=true
   ```
3. Check `bin/Debug/net10.0/Contracts.xml`

## Quick Reference

| Command | Purpose |
|---------|---------|
| `dotnet restore` | Restore NuGet dependencies |
| `dotnet build src/Contracts` | Build contracts library |
| `dotnet test tests/UnitTests` | Run all unit tests |
| `dotnet build -c Release` | Build release configuration |
| `dotnet test --filter "TestName"` | Run specific test |
| `dotnet test /p:CollectCoverage=true` | Run with coverage |

## Guidelines

When adding new contracts or tests:
1. Follow the patterns in this template
2. Use consistent namespacing and naming conventions
3. Add comprehensive XML documentation
4. Write unit tests for each new contract
5. Ensure all tests pass: `dotnet test`
6. Verify build without warnings: `dotnet build`

## 🤝 Contributing

Have a feature request or found a bug? We'd love to hear from you!

- [Report a Bug](https://github.com/gpreviatti/hexagonal-solution-template/issues/new?template=bug_report.md)
- [Request a Feature](https://github.com/gpreviatti/hexagonal-solution-template/issues/new?template=feature_request.md)
