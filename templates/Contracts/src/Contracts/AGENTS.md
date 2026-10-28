# AGENTS.md — Contracts

Rules for AI coding agents working in `src/Contracts/`. Shared rules are in [AGENTS.md](../../AGENTS.md). Overview and layout are in the [Readme](../../Readme.md).

## Rules

- Every contract is a `sealed record`. Do not use `class`.
- Contracts are pure data. Do not add business logic.
- Requests extend `BaseRequest(Guid CorrelationId)`, `BasePaginatedRequest`, or `BaseFullTextSearchPaginatedRequest`. Never duplicate base contracts.
- Responses extend `BaseResponse(bool IsSuccess, string Message)`.
- Use file-scoped namespaces: `namespace Contracts.[Domain];`.
- Annotate nullable properties with `?`. Nullable reference types are enabled.
- Proto3: never reuse or renumber existing field numbers. Use PascalCase for services and messages and snake_case for fields.
- Every new or changed contract needs a test. See [tests/UnitTests/AGENTS.md](../../tests/UnitTests/AGENTS.md).

## Skills

- [contract-generator](../../.github/skills/contract-generator/SKILL.md): creating request, response, DTO, or proto contracts.
