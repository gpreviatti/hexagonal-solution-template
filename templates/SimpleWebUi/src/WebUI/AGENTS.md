# AGENTS.md — WebUI

Rules for AI coding agents working in `src/WebUI/`. Shared rules are in [AGENTS.md](../../AGENTS.md). Commands and step-by-step guides are in the [Readme](../../Readme.md#-development-guide).

Responsibility: Blazor Server pages and components, health checks, and middleware. References Infrastructure only.

## Rules

- Put every business invariant in Core, never in pages.
- Pages that call async use cases must pass `_cancellationTokenSource.Token`, never `CancellationToken.None`.
