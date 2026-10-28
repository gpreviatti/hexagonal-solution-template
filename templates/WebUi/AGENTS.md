# AGENTS.md

Shared rules for AI coding agents working in this Blazor Web App solution. Project overview, setup, and code examples are in [Readme.md](Readme.md). Component and E2E patterns are in the [Development Guide](Readme.md#development-guide). Each project has its own `AGENTS.md` with the rules specific to it, listed below.

## Repository shape

| Path | Responsibility | Rules |
|---|---|---|
| `src/WebApp` | Blazor Web App (UI and inbound adapter). | [AGENTS.md](src/WebApp/AGENTS.md) |
| `src/Contracts` | Shared request/response contracts and DTOs (port models). | None |
| `src/Infrastructure` | Outbound adapters (HTTP, DI wiring, resiliency, telemetry). | [AGENTS.md](src/Infrastructure/AGENTS.md) |
| `src/MockApi` | Local simulator for the external API. | None |
| `tests/UnitTests` | xUnit, bUnit, and Moq tests. | [AGENTS.md](tests/UnitTests/AGENTS.md) |
| `tests/E2eTests` | Playwright end-to-end tests. | [AGENTS.md](tests/E2eTests/AGENTS.md) |

Keep the boundaries: UI composition in `WebApp`, DTOs and contracts in `Contracts`, and external communication and technical concerns in `Infrastructure`.

## Build and run

Prefer the VS Code tasks when available: `build [Solution]`, `test [Solution]`, `watch [WebApp]`, and `watch [MockApi]`.

## Rules

- Target `net10.0`. `Nullable` is enabled. Do not introduce nullable warnings.
- Warnings are errors (`TreatWarningsAsErrors`, `CodeAnalysisTreatWarningsAsErrors`). Keep the build warning-free.
- Package versions are managed centrally in `Directory.Packages.props`.

## Change hygiene

- Keep changes minimal and scoped to the request. Do not refactor unrelated files.
- Build and run the relevant tests after every change.
