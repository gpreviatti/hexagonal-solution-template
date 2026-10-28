# AGENTS.md

Shared rules for AI coding agents working in this hexagonal (ports and adapters) Blazor solution. Commands, configuration, and step-by-step guides are in [Readme.md](Readme.md#-development-guide). Each project has its own `AGENTS.md` with the rules specific to it, listed below.

## Quick reference

- Build: `dotnet build src/WebUI/WebUi.csproj`
- Unit tests: `dotnet test tests/UnitTests/UnitTests.csproj`
- E2E tests: `dotnet test tests/E2eTests/E2eTests.csproj` (the app must run on `http://localhost:5015`)

## Layers

| Layer | Path | Responsibility | Rules |
|---|---|---|---|
| Core | `src/Core` | Domain, use cases, ports, request and response records, messages. Must not reference Infrastructure or WebUI. | [AGENTS.md](src/Core/AGENTS.md) |
| Infrastructure | `src/Infrastructure` | Adapters: EF Core, Redis HybridCache, in-process messaging, OpenTelemetry. | [AGENTS.md](src/Infrastructure/AGENTS.md) |
| WebUI | `src/WebUI` | Blazor Server pages and components, health checks, and middleware. References Infrastructure only. | [AGENTS.md](src/WebUI/AGENTS.md) |
| Unit tests | `tests/UnitTests/` | Core tests, component tests, and architecture tests. | [AGENTS.md](tests/UnitTests/AGENTS.md) |
| E2E tests | `tests/E2eTests/` | Browser tests against the running app. | [AGENTS.md](tests/E2eTests/AGENTS.md) |

## Rules

- Keep Core free of infrastructure concerns. Infrastructure implements ports that Core defines.
- Keep the architecture rules green. They run with the unit tests in `tests/UnitTests/Architecture/`.
