# AGENTS.md — Contracts

Rules for AI coding agents working in `src/Contracts/`. Shared rules are in [AGENTS.md](../../AGENTS.md). Overview and examples are in the [template Readme](../../Readme.md).

Responsibility: business request and response records and gRPC `.proto` files. No infrastructure concerns.

## Rules

- Every HTTP request record carries a `Guid CorrelationId`.
- Use `decimal` for money.
- Keep gRPC field numbers stable. Never reuse or renumber a field.

## Skills

- [create-contracts](../../.github/skills/create-contracts/SKILL.md): adding or changing DTOs, proto definitions, or base contract wrappers.
