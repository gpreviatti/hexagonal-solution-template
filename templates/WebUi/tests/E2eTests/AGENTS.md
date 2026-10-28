# AGENTS.md — E2eTests

Rules for AI coding agents working in `tests/E2eTests/`. Shared rules are in [AGENTS.md](../../AGENTS.md). E2E patterns are in the [Development Guide](../../Readme.md#development-guide).

## Rules

- E2E tests need `WebApp` and `MockApi` running first.
- Select elements with `data-testid`. Keep those values stable, as described in [src/WebApp/AGENTS.md](../../src/WebApp/AGENTS.md).
- Do not hardcode waits or sleeps. Assert presence and shape, not non-deterministic MockApi totals.
