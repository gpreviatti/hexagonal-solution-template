# AGENTS.md

Shared rules for AI coding agents working in the Hexagonal Architecture Backend for Frontend (BFF) template. Architecture, commands, Docker setup, and code patterns are in the [template Readme](Readme.md). Each project has its own `AGENTS.md` with the rules specific to it, listed below.

## Layer boundaries

| Layer | Path | Responsibility | Rules |
|---|---|---|---|
| Contracts | `src/Contracts/` | Business request and response records and gRPC `.proto` files. No infrastructure concerns. | [AGENTS.md](src/Contracts/AGENTS.md) |
| Infrastructure | `src/Infrastructure/` | Outbound adapters, cross-cutting concerns, and DI composition. | [AGENTS.md](src/Infrastructure/AGENTS.md) |
| WebApp | `src/WebApp/` | Inbound adapters. Endpoints, middleware, and request orchestration. | [AGENTS.md](src/WebApp/AGENTS.md) |
| MockApi | `src/MockApi/` | Fake downstream services for local development and tests. | None |
| Integration tests | `tests/IntegrationTests/` | API behavior tests. | [AGENTS.md](tests/IntegrationTests/AGENTS.md) |

Keep contracts in `src/Contracts`, adapters in `src/Infrastructure`, and inbound composition in `src/WebApp`.

## Rules

- Target `.NET 10.0` with C# 13. `Nullable` is enabled.
- Zero compiler warnings. `TreatWarningsAsErrors` is enabled.
- Versions live in `Directory.Packages.props`. Never hardcode NuGet versions in project files.
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
