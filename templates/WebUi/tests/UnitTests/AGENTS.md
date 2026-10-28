# AGENTS.md — UnitTests

Rules for AI coding agents working in `tests/UnitTests/`. Shared rules are in [AGENTS.md](../../AGENTS.md). Component test patterns are in the [Development Guide](../../Readme.md#development-guide).

## Rules

- Use xUnit, bUnit, and Moq, with fixture-based component tests and explicit setup per scenario.
- Select elements with `data-testid`.
- Verify mocked HTTP calls with `Times.Once` where the call count matters.
