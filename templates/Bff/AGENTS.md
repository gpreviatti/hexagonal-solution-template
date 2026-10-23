# AGENTS.md

Rules for AI coding agents working in the Hexagonal Architecture Backend for Frontend (BFF) template. Architecture, commands, Docker setup, and code patterns are in the [template Readme](Readme.md), and the [Development Guide](Readme.md#development-guide) has the details.

## Layer boundaries

- **Contracts** (`src/Contracts/`): business request and response records and gRPC `.proto` files. No infrastructure concerns.
- **Infrastructure** (`src/Infrastructure/`): outbound adapters, cross-cutting concerns, and DI composition.
- **WebApp** (`src/WebApp/`): inbound adapters. Endpoints, middleware, and request orchestration.
- **MockApi** (`src/MockApi/`): fake downstream services for local development and tests.

Keep contracts in `src/Contracts`, adapters in `src/Infrastructure`, and inbound composition in `src/WebApp`.

## Rules

- Target `.NET 10.0` with C# 13. `Nullable` is enabled.
- Zero compiler warnings. `TreatWarningsAsErrors` is enabled.
- Versions live in `Directory.Packages.props`. Never hardcode NuGet versions in project files.
- Every HTTP request record carries a `Guid CorrelationId`. Use `decimal` for money.
- Keep gRPC field numbers stable.
- Wrap every endpoint in `ActivitySource.StartActivity(...)` and use correlation IDs for tracing.
- Extend `BaseHttpService` for outbound HTTP calls and use `HybridCacheService` for caching. Keep resilience policies in DI.
- New endpoints, services, and behaviors need tests. Prefer integration tests for API behavior.
- Keep cyclomatic complexity at 30 or below per method.
- No hardcoded secrets or SQL injection vectors.
- Run `dotnet build` and `dotnet test` after every change.

## Skills

Skills in `.github/skills/` contain templates and checklists. Read the matching `SKILL.md` before starting the task.

| Skill | Use when |
|---|---|
| [create-contracts](.github/skills/create-contracts/SKILL.md) | Adding or changing DTOs, proto definitions, or base contract wrappers |
| [create-endpoint-with-integration-tests](.github/skills/create-endpoint-with-integration-tests/SKILL.md) | Building GET, POST, PUT, PATCH, or DELETE endpoints with integration tests |
| [create-load-tests](.github/skills/create-load-tests/SKILL.md) | Adding k6 scenarios and thresholds |
| [code-review](.github/skills/code-review/SKILL.md) | Reviewing changes for architecture, tests, complexity, security, and modern C# |

Every endpoint needs `Produces`, `WithDescription`, and `WithName`. Its tests must assert the status code and payload on success and the error behavior on edge cases.
