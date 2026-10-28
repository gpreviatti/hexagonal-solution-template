# AGENTS.md — Infrastructure

Rules for AI coding agents working in `src/Infrastructure/`. Shared rules are in [AGENTS.md](../../AGENTS.md). Overview is in the [Readme](../../Readme.md).

Responsibility: outbound adapters (HTTP, DI wiring, resiliency, telemetry).

## Rules

- Keep the existing Polly retry policies and OpenTelemetry wiring, unless the task changes them.
