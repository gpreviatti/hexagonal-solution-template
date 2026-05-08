# Hexagonal Solution Template – Contracts

AI agent reference for the Contracts template. Full context in [Readme.md](Readme.md).

## Build & Test

```powershell
dotnet restore          # Restore locked packages
dotnet build            # Build all projects
dotnet test             # Run all unit tests
dotnet build -c Release # Production build
```

> `TreatWarningsAsErrors` is enabled globally. Every build warning is a build failure.

## Architecture

This project is the **shared contracts layer** of a Hexagonal Architecture solution. It exposes:
- **Request/Response DTOs** — immutable C# records consumed by application ports
- **DTOs** — domain transfer objects for read models
- **gRPC Contracts** — proto3 definitions compiled at build time via `Grpc.AspNetCore`

```
src/Contracts/
  Common/           ← BaseRequest, BaseResponse (extend these, never duplicate)
  [Domain]/         ← Sealed records per domain (e.g. Orders/, Payments/)
  Protos/           ← *.proto files (auto-compiled via <Protobuf> item group)

tests/UnitTests/
  Common/           ← Tests for base contracts
  [Domain]/         ← Mirrors src/Contracts structure, one test file per contract file
```

## Conventions

### C# Contracts
- All contracts are `sealed record` — immutable, serialization-safe, value-equal by default
- All requests **must** extend `BaseRequest(Guid CorrelationId)` or `BasePaginatedRequest`
- All responses **must** extend `BaseResponse(bool IsSuccess, string Message)`
- Use file-scoped namespaces (`namespace Contracts.[Domain];`)
- Nullable reference types are enabled — annotate all nullable properties with `?`
- Package versions are managed centrally in [Directory.Packages.props](Directory.Packages.props) — never specify versions in `.csproj` files

### Unit Tests (xUnit)
- Test class naming: `[ClassName]Tests`
- Test method naming: `Given[Context]When[Action]Then[ExpectedBehavior]`
- Test display name = method name (set via `[Fact(DisplayName = nameof(...))]`)
- Test classes are `sealed` — no inheritance
- Use `Assert.*` directly (no assertion libraries)
- Every contract **must** have a corresponding test file validating: construction, property assignment, and defaults

### gRPC / Proto
- Proto files live in `src/Contracts/Protos/` and are compiled via the `<Protobuf>` item group
- Follow proto3 field numbering — never reuse or renumber existing fields
- Service and message names use PascalCase; field names use snake_case

## Skills (use these before writing code)

| Skill | Trigger |
|-------|---------|
| [contract-generator](.github/skills/contract-generator/SKILL.md) | Creating new request, response, DTO, or proto contracts |
| [code-review](.github/skills/code-review/SKILL.md) | Reviewing changes before merge — enforces quality gates |

## Examples

| Pattern | File |
|---------|------|
| Sealed request record | [src/Contracts/Orders/CreateOrderRequest.cs](src/Contracts/Orders/CreateOrderRequest.cs) |
| Base request | [src/Contracts/Common/BaseRequest.cs](src/Contracts/Common/BaseRequest.cs) |
| Base response | [src/Contracts/Common/BaseResponse.cs](src/Contracts/Common/BaseResponse.cs) |
| Proto definition | [src/Contracts/Protos/payment.proto](src/Contracts/Protos/payment.proto) |
| Request test | [tests/UnitTests/Orders/CreateOrderRequestTests.cs](tests/UnitTests/Orders/CreateOrderRequestTests.cs) |
| Base request test | [tests/UnitTests/Common/BaseRequestTests.cs](tests/UnitTests/Common/BaseRequestTests.cs) |

## Common Pitfalls

- Do **not** add business logic to contracts — they are pure data containers
- Do **not** add package versions in `.csproj`; use [Directory.Packages.props](Directory.Packages.props)
- Do **not** use `class` for contracts — use `record` or `sealed record`
- Do **not** skip tests — every new or changed contract needs a test
