# AGENTS.md

Rules for AI coding agents working in this hexagonal (ports and adapters) solution. Commands, configuration, and step-by-step guides are in [Readme.md](Readme.md#-development-guide).

## Quick reference

- Build: `dotnet build src/WebApp/WebApp.csproj`
- Unit tests: `dotnet test tests/UnitTests/UnitTests.csproj`
- Integration tests: `dotnet test tests/IntegrationTests/IntegrationTests.csproj` (needs PostgreSQL and Redis)

## Layers

- **`src/Core`:** domain, use cases, ports, request and response records, messages. Must not reference Infrastructure or WebApp.
- **`src/Infrastructure`:** adapters (EF Core, Redis HybridCache, in-process messaging, OpenTelemetry).
- **`src/WebApp`:** Minimal API endpoints, health checks, and middleware. References Infrastructure only.

## Rules

- Keep Core free of infrastructure concerns. Infrastructure implements ports that Core defines.
- Put every business invariant in a domain entity or use case, not in endpoints or adapters.
- Use `Result<T>` for expected failures. Do not return `Result` from HTTP endpoints; wrap responses in `BaseResponse` types.
- Implement only `HandleInternalAsync` in use cases. Base classes handle validation, tracing, metrics, and logging.
- Do not register use cases manually. `AddCore()` discovers them.
- Core class names must not end in `Controller`, `Endpoint`, `DbContext`, `Mapping`, `Consumer`, or `Producer`. All Core types live in `Core.*`.
- Do not change the cross-project boundaries that the architecture tests enforce.

## Testing

- Unit tests use `BaseCoreFixture`. Each test class has its own fixture subclass.
- Integration tests use `CustomWebApplicationFactory<Program>` and the `WebApplicationFactoryCollectionDefinition` collection.
- Architecture tests in `tests/UnitTests/Architecture/` run with the unit tests and must stay green.
