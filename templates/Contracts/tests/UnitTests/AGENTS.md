# AGENTS.md — UnitTests

Rules for AI coding agents working in `tests/UnitTests/`. Shared rules are in [AGENTS.md](../../AGENTS.md). Overview and test design are in the [Readme](../../Readme.md).

## Rules

- Every new or changed contract needs a test that covers construction, property assignment, and defaults.
- Use xUnit. Test classes are `sealed` and do not inherit. Use `Assert.*` directly.
- Test names follow `Given[Context]When[Action]Then[ExpectedBehavior]`, set with `[Fact(DisplayName = nameof(...))]`.
