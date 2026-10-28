# AGENTS.md — WebApp

Rules for AI coding agents working in `src/WebApp/`. Shared rules are in [AGENTS.md](../../AGENTS.md). Overview and examples are in the [template Readme](../../Readme.md), and the [Development Guide](../../Readme.md#development-guide) has the details.

Responsibility: inbound adapters. Endpoints, middleware, and request orchestration.

## Rules

- Wrap every endpoint in `ActivitySource.StartActivity(...)` and use correlation IDs for tracing.
- Every endpoint needs `Produces`, `WithDescription`, and `WithName`.

## Skills

- [create-endpoint-with-integration-tests](../../.github/skills/create-endpoint-with-integration-tests/SKILL.md): building endpoints with integration tests.
