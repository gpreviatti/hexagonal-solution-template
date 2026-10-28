# AGENTS.md

Shared rules for AI coding agents working in this hexagonal (ports and adapters, DDD) solution. Project overview is in [Readme.md](Readme.md). Each project has its own `AGENTS.md` with the rules specific to it, listed below. Read the one for the project you are changing.

## Quick reference

- Build: `dotnet build`
- Test: `dotnet test`

## Architecture

Dependencies point inward only: Domain → Application → Infrastructure → WebApp.

| Layer | Path | Responsibility | Rules |
|---|---|---|---|
| Domain | `src/Domain/` | Entities, value objects, invariants. | [AGENTS.md](src/Domain/AGENTS.md) |
| Application | `src/Application/` | Use cases, validators, DTOs, and ports. | [AGENTS.md](src/Application/AGENTS.md) |
| Infrastructure | `src/Infrastructure/` | Adapters that implement the ports: EF Core, cache, messaging, telemetry. | [AGENTS.md](src/Infrastructure/AGENTS.md) |
| WebApp | `src/WebApp/` | HTTP entry point: Minimal API endpoints, middleware, composition in `Program.cs`. | [AGENTS.md](src/WebApp/AGENTS.md) |
| Unit tests | `tests/UnitTests/` | Domain, application, and validator tests. | [AGENTS.md](tests/UnitTests/AGENTS.md) |
| Integration tests | `tests/IntegrationTests/` | HTTP, gRPC, and messaging tests. | [AGENTS.md](tests/IntegrationTests/AGENTS.md) |

## Testing

- Test names follow `GivenContext_WhenCondition_ThenExpectedResult`.
- Mutation survival rate should be at least 80%.

## Coding standards

- Use file-scoped namespaces and seal classes by default. Use records for DTOs.
- Use `async`/`await` with a `CancellationToken` on every I/O path.
- Keep cyclomatic complexity at or below 30 per method. Extract methods or move logic into domain methods.
- Never hardcode credentials. Never log sensitive data or PII.
- Catch specific exceptions. Do not suppress them without a reason.

## Feature checklist

1. **Domain:** entity. See [src/Domain/AGENTS.md](src/Domain/AGENTS.md).
2. **Application:** use case, request, validator, and DTO. See [src/Application/AGENTS.md](src/Application/AGENTS.md).
3. **Infrastructure:** mapping and migration. See [src/Infrastructure/AGENTS.md](src/Infrastructure/AGENTS.md).
4. **WebApp:** endpoints, registration, and samples. See [src/WebApp/AGENTS.md](src/WebApp/AGENTS.md).
5. **Tests:** domain, application, and HTTP integration tests. See [tests/UnitTests/AGENTS.md](tests/UnitTests/AGENTS.md) and [tests/IntegrationTests/AGENTS.md](tests/IntegrationTests/AGENTS.md).

## Code review

Run the `code-review` skill before merging. It checks architecture, tests, complexity, security, and modern C#.

## AI agent skills

Skills in `.github/skills/` contain templates and checklists. Read the matching `SKILL.md` before generating code.

| Skill | Use when |
|---|---|
| [entity-generator](.github/skills/entity-generator/SKILL.md) | Creating entities, aggregates, or value objects |
| [use-case-generator](.github/skills/use-case-generator/SKILL.md) | Creating use cases, validators, and tests |
| [endpoints-generator](.github/skills/endpoints-generator/SKILL.md) | Exposing use cases over HTTP |
| [consumers-generator](.github/skills/consumers-generator/SKILL.md) | Creating message consumers and contracts |
| [ef-mapper](.github/skills/ef-mapper/SKILL.md) | Creating EF Core mappings |
| [integration-tests-generator](.github/skills/integration-tests-generator/SKILL.md) | Writing HTTP, gRPC, and messaging integration tests |
| [load-tests-generator](.github/skills/load-tests-generator/SKILL.md) | Writing k6 scripts |
| [code-review](.github/skills/code-review/SKILL.md) | Reviewing changes before merge |

Prompts in `.github/prompts/` include `create-migrations.prompt.md` and `crud-generator.prompt.md`.
