# AGENTS.md — Application

Rules for AI coding agents working in `src/Application/`. Shared rules are in [AGENTS.md](../../AGENTS.md). Patterns are in [Development Patterns](../../Readme.md#-development-patterns).

Responsibility: use cases, validators, DTOs, and ports (interfaces such as `IBaseRepository<T>`).

## Rules

- Define ports here. Infrastructure implements them.
- One operation per use case file. Seal use cases.
- Choose the base class by input and output (`BaseInOutUseCase`, `BaseInUseCase`, `BaseOutUseCase`).
- Validators implement `AbstractValidator<TRequest>` and are discovered automatically.
- Return DTO records from use cases. Never expose entities.

## Feature checklist

- Use case, request, validator, and DTO in `src/Application/{Feature}/`.
- Unit tests for use cases and validators in `tests/UnitTests/Application/`.
