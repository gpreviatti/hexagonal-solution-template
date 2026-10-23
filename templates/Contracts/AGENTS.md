# AGENTS.md

Rules for AI coding agents working in the Contracts template, the shared contracts layer of a hexagonal solution. Project overview, layout, build commands, and reference examples are in [Readme.md](Readme.md).

## Rules

- Every contract is a `sealed record`. Do not use `class`.
- Contracts are pure data. Do not add business logic.
- Requests extend `BaseRequest(Guid CorrelationId)`, `BasePaginatedRequest`, or `BaseFullTextSearchPaginatedRequest`. Never duplicate base contracts.
- Responses extend `BaseResponse(bool IsSuccess, string Message)`.
- Use file-scoped namespaces: `namespace Contracts.[Domain];`.
- Annotate nullable properties with `?`. Nullable reference types are enabled.
- Put package versions in `Directory.Packages.props`, never in `.csproj` files.
- Proto3: never reuse or renumber existing field numbers. Use PascalCase for services and messages and snake_case for fields.
- Every new or changed contract needs a test that covers construction, property assignment, and defaults.
- Unit tests use xUnit. Test classes are `sealed` and do not inherit. Use `Assert.*` directly.
- Test names follow `Given[Context]When[Action]Then[ExpectedBehavior]`, set with `[Fact(DisplayName = nameof(...))]`.
- `TreatWarningsAsErrors` is enabled. Any warning fails the build.

## Skills

Use these before writing code:

| Skill | Use when |
|---|---|
| [contract-generator](.github/skills/contract-generator/SKILL.md) | Creating request, response, DTO, or proto contracts |
| [code-review](.github/skills/code-review/SKILL.md) | Reviewing changes before merge |
