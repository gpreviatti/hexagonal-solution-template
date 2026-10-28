# AGENTS.md — Core

Rules for AI coding agents working in `src/Core/`. Shared rules are in [AGENTS.md](../../AGENTS.md). Commands and step-by-step guides are in the [Readme](../../Readme.md#-development-guide).

Responsibility: domain, use cases, ports, request and response records, and messages. Must not reference Infrastructure or WebUI.

## Rules

- Put every business invariant in a domain entity or use case, never in pages or adapters.
- Use `Result<T>` for expected failures.
- Implement only `HandleInternalAsync` in use cases. Do not register use cases manually.
