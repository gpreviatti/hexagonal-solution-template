# AGENTS.md — Domain

Rules for AI coding agents working in `src/Domain/`. Shared rules are in [AGENTS.md](../../AGENTS.md). Patterns are in [Development Patterns](../../Readme.md#-development-patterns).

Responsibility: entities, value objects, and invariants. No dependencies on other layers or infrastructure packages.

## Rules

- Enforce invariants inside entities, never in services.
- Seal entities and value objects. Use private setters and change state through business methods.
- Use `Result<T>` for expected errors. Use exceptions only for invariant violations that should never happen.
- Soft delete only: set `IsDeleted`, never remove rows.

## Feature checklist

- Entity in `src/Domain/{Feature}/`.
- Unit tests for the entity in `tests/UnitTests/Domain/`.
