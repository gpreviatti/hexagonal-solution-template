# AGENTS.md

Rules for AI coding agents working in this hexagonal (ports and adapters) Blazor solution. Commands, configuration, and step-by-step guides are in [Readme.md](Readme.md#-development-guide).

## Quick reference

- Build: `dotnet build src/WebUI/WebUi.csproj`
- Unit tests: `dotnet test tests/UnitTests/UnitTests.csproj`
- E2E tests: `dotnet test tests/E2eTests/E2eTests.csproj` (the app must run on `http://localhost:5015`)

## Layers

- **`src/Core`:** domain, use cases, ports, request and response records, messages. Must not reference Infrastructure or WebUI.
- **`src/Infrastructure`:** adapters (EF Core, Redis HybridCache, in-process messaging, OpenTelemetry).
- **`src/WebUI`:** Blazor Server pages and components, health checks, and middleware. References Infrastructure only.

## Rules

- Keep Core free of infrastructure concerns. Infrastructure implements ports that Core defines.
- Put every business invariant in a domain entity or use case, never in pages or adapters.
- Use `Result<T>` for expected failures.
- Implement only `HandleInternalAsync` in use cases. Do not register use cases manually.
- Pages that call async use cases must pass `_cancellationTokenSource.Token`, never `CancellationToken.None`.
- Put every CSS selector in `tests/CommonTests/WebUI/Selectors.cs`. Never hardcode selectors in tests.
- Keep the architecture rules green. They run with the unit tests in `tests/UnitTests/Architecture/`.
- Test methods use `GivenWhenThen` names, with no underscores (CA1707).
- E2E tests must not use `WaitForLoadStateAsync(NetworkIdle)` to detect Blazor processing. Use the Blazor timing patterns in the Readme.
