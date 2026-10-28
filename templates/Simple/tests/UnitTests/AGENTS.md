# AGENTS.md — UnitTests

Rules for AI coding agents working in `tests/UnitTests/`. Shared rules are in [AGENTS.md](../../AGENTS.md). Test fixtures are described in the [Readme](../../Readme.md#-development-guide).

## Rules

- Unit tests use `BaseCoreFixture`. Each test class has its own fixture subclass.
- Architecture tests in `Architecture/` run with the unit tests and must stay green.
