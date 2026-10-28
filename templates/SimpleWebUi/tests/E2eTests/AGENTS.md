# AGENTS.md — E2eTests

Rules for AI coding agents working in `tests/E2eTests/`. Shared rules are in [AGENTS.md](../../AGENTS.md). Blazor timing patterns are in the [Readme](../../Readme.md#-development-guide).

## Rules

- Put every CSS selector in `tests/CommonTests/WebUI/Selectors.cs`. Never hardcode selectors in tests.
- Do not use `WaitForLoadStateAsync(NetworkIdle)` to detect Blazor processing. Use the Blazor timing patterns in the Readme.
- The app must be running on `http://localhost:5015`.
