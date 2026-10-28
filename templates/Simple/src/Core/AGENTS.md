# AGENTS.md — Core

Rules for AI coding agents working in `src/Core/`. Shared rules are in [AGENTS.md](../../AGENTS.md). Commands and step-by-step guides are in the [Readme](../../Readme.md#-development-guide).

Responsibility: domain, use cases, ports, request and response records, and messages. Must not reference Infrastructure or WebApp.

## Rules

- Put every business invariant in a domain entity or use case, not in endpoints or adapters.
- Use `Result<T>` for expected failures.
- Implement only `HandleInternalAsync` in use cases. Base classes handle validation, tracing, metrics, and logging.
- Do not register use cases manually. `AddCore()` discovers them.
- Core class names must not end in `Controller`, `Endpoint`, `DbContext`, `Mapping`, `Consumer`, or `Producer`. All Core types live in `Core.*`.
