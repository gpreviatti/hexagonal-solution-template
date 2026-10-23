# AGENTS.md

Rules for AI coding agents working in this Blazor Web App solution. Project overview, setup, and code examples are in [Readme.md](Readme.md). Component and E2E patterns are in the [Development Guide](Readme.md#development-guide).

## Repository shape

- `src/WebApp`: Blazor Web App (UI and inbound adapter).
- `src/Contracts`: shared request/response contracts and DTOs (port models).
- `src/Infrastructure`: outbound adapters (HTTP, DI wiring, resiliency, telemetry).
- `src/MockApi`: local simulator for the external API.
- `tests/UnitTests`: xUnit, bUnit, and Moq tests.
- `tests/E2eTests`: Playwright end-to-end tests.

Keep the boundaries: UI composition in `WebApp`, DTOs and contracts in `Contracts`, and external communication and technical concerns in `Infrastructure`.

## Build and run

Prefer the VS Code tasks when available: `build [Solution]`, `test [Solution]`, `watch [WebApp]`, and `watch [MockApi]`.

## Rules

- Target `net10.0`. `Nullable` is enabled. Do not introduce nullable warnings.
- Warnings are errors (`TreatWarningsAsErrors`, `CodeAnalysisTreatWarningsAsErrors`). Keep the build warning-free.
- Package versions are managed centrally in `Directory.Packages.props`.
- Keep page markup in `.razor` and behavior in `.razor.cs`.
- Use `IBaseHttpService` for outbound HTTP. Do not call `HttpClient` directly from components.
- On a failed call, log through `Logs.FailedOperation(...)` and keep the UI in a loading or fallback state instead of throwing.
- Add `data-testid` attributes to interactive and validated elements.
- Keep the existing Polly retry policies and OpenTelemetry wiring, unless the task changes them.
- Keep the `data-testid` values and the Not Found page contract used by E2E tests stable. If you change them, update the E2E tests in the same change.
- Do not hardcode waits or sleeps in E2E tests. Assert presence and shape, not non-deterministic MockApi totals.

## Testing

- Unit tests use xUnit, bUnit, and Moq, with fixture-based component tests and explicit setup per scenario.
- Select elements with `data-testid`.
- Verify mocked HTTP calls with `Times.Once` where the call count matters.
- E2E tests need `WebApp` and `MockApi` running first.

## Change hygiene

- Keep changes minimal and scoped to the request. Do not refactor unrelated files.
- Build and run the relevant tests after every change.
