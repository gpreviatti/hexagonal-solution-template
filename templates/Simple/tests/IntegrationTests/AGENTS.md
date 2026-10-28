# AGENTS.md — IntegrationTests

Rules for AI coding agents working in `tests/IntegrationTests/`. Shared rules are in [AGENTS.md](../../AGENTS.md). Commands and fixtures are in the [Readme](../../Readme.md#-development-guide).

## Rules

- Use `CustomWebApplicationFactory<Program>` and the `WebApplicationFactoryCollectionDefinition` collection.
- Tests need PostgreSQL and Redis running.
