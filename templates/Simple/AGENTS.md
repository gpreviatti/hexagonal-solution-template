# AGENTS.md

Shared rules for AI coding agents working in this hexagonal (ports and adapters) solution. Commands, configuration, and step-by-step guides are in [Readme.md](Readme.md#-development-guide). Each project has its own `AGENTS.md` with the rules specific to it, listed below.

## Quick reference

- Build: `dotnet build src/WebApp/WebApp.csproj`
- Unit tests: `dotnet test tests/UnitTests/UnitTests.csproj`
- Integration tests: `dotnet test tests/IntegrationTests/IntegrationTests.csproj` (needs PostgreSQL and Redis)

## Layers

| Layer | Path | Responsibility | Rules |
|---|---|---|---|
| Core | `src/Core` | Domain, use cases, ports, request and response records, messages. Must not reference Infrastructure or WebApp. | [AGENTS.md](src/Core/AGENTS.md) |
| Infrastructure | `src/Infrastructure` | Adapters: EF Core, Redis HybridCache, in-process messaging, OpenTelemetry. | [AGENTS.md](src/Infrastructure/AGENTS.md) |
| WebApp | `src/WebApp` | Minimal API endpoints, health checks, and middleware. References Infrastructure only. | [AGENTS.md](src/WebApp/AGENTS.md) |
| Unit tests | `tests/UnitTests/` | Core tests and architecture tests. | [AGENTS.md](tests/UnitTests/AGENTS.md) |
| Integration tests | `tests/IntegrationTests/` | HTTP endpoint tests. | [AGENTS.md](tests/IntegrationTests/AGENTS.md) |

## Rules

- Keep Core free of infrastructure concerns. Infrastructure implements ports that Core defines.
- Do not change the cross-project boundaries that the architecture tests enforce.
- The architecture tests in `tests/UnitTests/Architecture/` run with the unit tests and must stay green.
