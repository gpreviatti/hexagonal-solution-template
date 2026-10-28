# AGENTS.md — IntegrationTests

Rules for AI coding agents working in `tests/IntegrationTests/`. Shared rules are in [AGENTS.md](../../AGENTS.md). Test strategy is in the [Development Guide](../../Readme.md#development-guide).

## Rules

- New endpoints, services, and behaviors need tests. Prefer integration tests for API behavior.
- Endpoint tests must assert the status code and payload on success, and the error behavior on edge cases.

## Skills

- [create-endpoint-with-integration-tests](../../.github/skills/create-endpoint-with-integration-tests/SKILL.md): building endpoints with integration tests.
- [create-load-tests](../../.github/skills/create-load-tests/SKILL.md): k6 scenarios and thresholds.
