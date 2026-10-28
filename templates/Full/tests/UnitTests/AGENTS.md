# AGENTS.md — UnitTests

Rules for AI coding agents working in `tests/UnitTests/`. Shared rules (test naming, mutation survival rate) are in [AGENTS.md](../../AGENTS.md). Test strategy is in [Readme.md](../../Readme.md#-testing-strategy).

## Rules

- Domain entities, use cases, and validators need unit tests.
- Use `BaseApplicationFixture<TRequest, TUseCase>` for use case tests.
- Test folders mirror the source layers: `Domain/`, `Application/`, and `Architecture/`.
