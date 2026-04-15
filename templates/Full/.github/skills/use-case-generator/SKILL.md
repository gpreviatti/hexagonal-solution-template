---
name: use-case-generator
description: 'Generate Application use case classes (BaseInOutUseCase, BaseInUseCase, BaseOutUseCase) with request/validator patterns, notifications, and auto-DI conventions for this hexagonal template.'
---

# Use Case Generator — Hexagonal Architecture Template

Generate new Application-layer use cases using the exact project conventions for request records, FluentValidation, repository usage, notifications, and auto-registration.

## When to Use

Activate this skill when:
- User asks to create a new use case
- User asks to implement Create/Get/GetAll/Update/Delete orchestration in Application layer
- User asks to add request/response records and validators
- User mentions `BaseInOutUseCase`, `BaseInUseCase`, or `BaseOutUseCase`
- User asks how a use case should publish notifications

## Skill-local References

| File | Purpose |
|------|---------|
| `./references/base-inout-template.md` | Template for request + response use case |
| `./references/base-in-template.md` | Template for fire-and-forget use case |
| `./references/base-out-template.md` | Template for output-only use case |
| `./references/registration-checklist.md` | Auto-DI and naming checklist |

> This skill is self-contained by design: prefer local references from this folder.

---

## Architecture Guidance

### Layers and responsibilities

- **Domain layer**: business rules and invariants (`Result`, domain entities)
- **Application layer**: orchestration, validation, repository calls, notification publication
- **Infrastructure layer**: concrete persistence and transport implementations

Use cases should orchestrate domain calls; they should not embed domain invariants directly when those invariants belong to entities.

### Base class selection

- **`BaseInOutUseCase<TRequest, TResponse>`**: for CRUD operations returning data
- **`BaseInUseCase<TRequest>`**: for side-effect operations without response payload
- **`BaseOutUseCase<TResponse>`**: for operations with no input request

---

## Required Conventions

- Use **file-scoped namespaces**
- Request records inherit `BaseRequest` when input exists
- Add `AbstractValidator<TRequest>` for each input request type
- Use `Repository` from base class for persistence/querying
- Use `Logs.*` helper methods for operation failures/not-found
- Publish notifications via:
  - `CreateNotification(correlationId, NotificationStatus.Success|Failed, user, notificationType, response)`
- Keep class names ending with `UseCase` for auto DI registration

---

## Typical Generation Flow

1. Create request and validator
2. Implement use case class with the right base type
3. Add repository/domain orchestration in `HandleInternalAsync`
4. Return consistent `BaseResponse`/`BasePaginatedResponse`
5. Publish notification when workflow requires async messaging
6. Confirm naming/placement with `./references/registration-checklist.md`

---

## Output Expectations

When this skill is used, generated output should include:
- New use case file under `src/Application/{Context}/`
- Request record + validator in same file (matching existing style)
- Use case class with async implementation and cancellation token usage
- Proper response model (`BaseResponse<T>` or `BasePaginatedResponse<T>`)
- Notification creation (where business flow expects it)

---

## Quick Checklist

- [ ] Correct base use case selected
- [ ] Request + validator created (if input exists)
- [ ] Async repository calls use provided `CancellationToken`
- [ ] Errors return meaningful messages
- [ ] Notification status uses `NotificationStatus.Success`/`NotificationStatus.Failed`
- [ ] Use case class name ends with `UseCase`
- [ ] Placed under `src/Application/{Context}/`
