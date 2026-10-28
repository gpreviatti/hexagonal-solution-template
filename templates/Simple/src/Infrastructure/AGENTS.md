# AGENTS.md — Infrastructure

Rules for AI coding agents working in `src/Infrastructure/`. Shared rules are in [AGENTS.md](../../AGENTS.md). Commands and step-by-step guides are in the [Readme](../../Readme.md#-development-guide).

Responsibility: adapters (EF Core, Redis HybridCache, in-process messaging, OpenTelemetry).

## Rules

- Implement the ports that Core defines. Do not add business logic here.
