# AGENTS.md — WebApp

Rules for AI coding agents working in `src/WebApp/`. Shared rules are in [AGENTS.md](../../AGENTS.md). Commands and step-by-step guides are in the [Readme](../../Readme.md#-development-guide).

Responsibility: Minimal API endpoints, health checks, and middleware. References Infrastructure only.

## Rules

- Do not return `Result` from HTTP endpoints. Wrap responses in `BaseResponse` types.
- Put every business invariant in Core, not in endpoints.
