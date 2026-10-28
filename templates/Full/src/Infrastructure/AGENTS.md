# AGENTS.md — Infrastructure

Rules for AI coding agents working in `src/Infrastructure/`. Shared rules are in [AGENTS.md](../../AGENTS.md). Patterns are in [Development Patterns](../../Readme.md#-development-patterns).

Responsibility: adapters that implement the ports from Application: EF Core, cache, messaging, telemetry.

## Rules

- Repositories inherit `BaseRepository<T>`. One EF Core mapping per entity in `Data/Mapping/`.
- Full-text search needs a GIN tsvector index on the searched columns, created through a migration.
- Consumers inherit `BaseConsumer<TMessage, TConsumer>`. Keep `CorrelationId` as the deduplication key.

## Migrations

- When adding full-text search to an existing entity, put the index in its own migration.

## Feature checklist

- Mapping in `Data/Mapping/`, and a migration.
- Migration commands are in [Readme.md](../../Readme.md#database-migrations).
