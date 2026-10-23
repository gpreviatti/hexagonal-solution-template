# AGENTS.md

Rules for AI coding agents working in this hexagonal (ports and adapters, DDD) solution. Project overview, commands, and examples are in [Readme.md](Readme.md). Detailed patterns are in [Development Patterns](Readme.md#-development-patterns).

## Quick reference

- Build: `dotnet build`
- Test: `dotnet test`
- Run: `dotnet run --project src/WebApp` (listens on `https://localhost:7000`)

## Architecture

Dependencies point inward only: Domain → Application → Infrastructure → WebApp.

| Layer | Path | Responsibility |
|---|---|---|
| Domain | `src/Domain/` | Entities, value objects, invariants. No dependencies on other layers or infrastructure packages. |
| Application | `src/Application/` | Use cases, validators, DTOs, and ports (interfaces such as `IBaseRepository<T>`). |
| Infrastructure | `src/Infrastructure/` | Adapters that implement the ports: EF Core, cache, messaging, telemetry. |
| WebApp | `src/WebApp/` | HTTP entry point: Minimal API endpoints, middleware, composition in `Program.cs`. |

## Rules

### Domain

- Enforce invariants inside entities, never in services.
- Seal entities and value objects. Use private setters and change state through business methods.
- Use `Result<T>` for expected errors. Use exceptions only for invariant violations that should never happen.
- Soft delete only: set `IsDeleted`, never remove rows.

### Application

- Define ports here. Infrastructure implements them.
- One operation per use case file. Seal use cases.
- Choose the base class by input and output (`BaseInOutUseCase`, `BaseInUseCase`, `BaseOutUseCase`).
- Validators implement `AbstractValidator<TRequest>` and are discovered automatically.
- Return DTO records from use cases. Never expose entities.

### Infrastructure

- Repositories inherit `BaseRepository<T>`. One EF Core mapping per entity in `Data/Mapping/`.
- Full-text search needs a GIN tsvector index on the searched columns, created through a migration.
- Consumers inherit `BaseConsumer<TMessage, TConsumer>`. Keep `CorrelationId` as the deduplication key.

### WebApp

- Endpoints are static `internal` classes in `Endpoints/`, registered in `EndpointExtensions.cs`.
- Read `correlationId` from the header and tag the current activity with it.
- Use the standard status codes for each route (see [Development Patterns](Readme.md#endpoints)).
- Add a sample for every new endpoint in `src/WebApp/WebApp.http`.

### Testing

- Test names follow `GivenContext_WhenCondition_ThenExpectedResult`.
- Domain entities, use cases, and validators need unit tests.
- HTTP and gRPC endpoints need integration tests.
- Use the shared fixtures: `BaseApplicationFixture<TRequest, TUseCase>` (unit), `BaseHttpFixture` (HTTP), and `BaseMessagingFixture` (messaging).
- Annotate HTTP integration tests with `[Collection("WebApplicationFactoryCollectionDefinition")]`.

### Coding standards

- Use file-scoped namespaces and seal classes by default. Use records for DTOs.
- Use `async`/`await` with a `CancellationToken` on every I/O path.
- Keep cyclomatic complexity at or below 30 per method. Extract methods or move logic into domain methods.
- Never hardcode credentials. Never log sensitive data or PII.
- Catch specific exceptions. Do not suppress them without a reason.
- Include correlation IDs in endpoint handlers and their logs.
- Mutation survival rate should be at least 80%.

### Migrations

- When adding full-text search to an existing entity, put the index in its own migration.

## Feature checklist

1. **Domain:** entity in `src/Domain/{Feature}/`.
2. **Application:** use case, request, validator, and DTO in `src/Application/{Feature}/`.
3. **Infrastructure:** mapping in `Data/Mapping/`, and a migration.
4. **WebApp:** endpoints, a registration line in `EndpointExtensions.cs`, and samples in `WebApp.http`.
5. **Tests:** domain, application, and HTTP integration tests.

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
