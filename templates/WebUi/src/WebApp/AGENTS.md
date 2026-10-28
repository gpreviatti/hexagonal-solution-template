# AGENTS.md — WebApp

Rules for AI coding agents working in `src/WebApp/`. Shared rules are in [AGENTS.md](../../AGENTS.md). Component patterns are in the [Development Guide](../../Readme.md#development-guide).

Responsibility: Blazor Web App UI and inbound adapter.

## Rules

- Keep page markup in `.razor` and behavior in `.razor.cs`.
- Use `IBaseHttpService` for outbound HTTP. Do not call `HttpClient` directly from components.
- On a failed call, log through `Logs.FailedOperation(...)` and keep the UI in a loading or fallback state instead of throwing.
- Add `data-testid` attributes to interactive and validated elements.
- Keep the `data-testid` values and the Not Found page contract used by E2E tests stable. If you change them, update the E2E tests in the same change.
