# AGENTS.md — Infrastructure

Rules for AI coding agents working in `src/Infrastructure/`. Shared rules are in [AGENTS.md](../../AGENTS.md). Overview and examples are in the [template Readme](../../Readme.md), and the [Development Guide](../../Readme.md#development-guide) has the details.

Responsibility: outbound adapters, cross-cutting concerns, and DI composition.

## Rules

- Extend `BaseHttpService` for outbound HTTP calls.
- Use `HybridCacheService` for caching.
- Keep resilience policies in DI.
