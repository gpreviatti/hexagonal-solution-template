---
name: use-case-generator
description: 'Generate Application use case classes and xUnit unit tests following BaseInOutUseCase/BaseInUseCase/BaseOutUseCase patterns, FluentValidation, repository mocks, and fixture conventions for this hexagonal template.'
---

# Use Case Generator — Hexagonal Architecture Template

Generate new Application-layer use cases and their xUnit unit tests using the exact project conventions for request records, FluentValidation, repository usage, notifications, fixture patterns, and auto-registration.

## When to Use

Activate this skill when:
- User asks to create a new use case
- User asks to implement Create/Get/GetAll/Update/Delete orchestration in Application layer
- User asks to add request/response records and validators
- User mentions `BaseInOutUseCase`, `BaseInUseCase`, or `BaseOutUseCase`
- User asks how a use case should publish notifications
- User requests unit tests for a new or existing use case
- User asks to test `BaseInOutUseCase`, `BaseInUseCase`, or `BaseOutUseCase` implementations
- User mentions "use case tests", "application tests", or "HandleAsync tests"
- User asks to add test coverage for Create, Get, GetAll, Update, or Delete use cases
- User asks to test FluentValidation request validators

## Skill-local References

| File | Purpose |
|------|---------|
| `./references/base-inout-template.md` | Template for request + response use case |
| `./references/base-in-template.md` | Template for fire-and-forget use case |
| `./references/base-out-template.md` | Template for output-only use case |
| `./references/registration-checklist.md` | Auto-DI and naming checklist |
| `./references/fixture-cheatsheet.md` | Quick map of fixture helpers and verification methods |
| `./references/test-class-template.md` | Minimal scaffold template for a new use case test class |
| `./references/test-create-pattern.md` | Full Create use case test pattern (validator + fixture + tests) |
| `./references/test-get-pattern.md` | Full Get by Id use case test pattern |
| `./references/test-getall-pattern.md` | Full GetAll paginated use case test pattern |
| `./references/test-update-pattern.md` | Full Update use case test pattern |
| `./references/test-delete-pattern.md` | Full Delete / soft-delete use case test pattern |
| `./references/test-basein-pattern.md` | Full BaseInUseCase (fire-and-forget) test pattern |

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
7. Generate unit test file following the patterns below
8. Include validator tests, use case fixture, and use case test class in one file

---

## Output Expectations

When this skill is used, generated output should include:
- New use case file under `src/Application/{Context}/`
- Request record + validator in same file (matching existing style)
- Use case class with async implementation and cancellation token usage
- Proper response model (`BaseResponse<T>` or `BasePaginatedResponse<T>`)
- Notification creation (where business flow expects it)
- Unit test file under `tests/UnitTests/Application/{Context}/` with all required scenarios

---

## Test File Structure

Every use case test file follows this structure (all classes in one file):

```
1. [Optional] Request Validator Fixture class
2. [Optional] Request Validation Tests class
3. Use Case Fixture class  (extends BaseApplicationFixture<TRequest, TUseCase>)
4. Use Case Tests class    (implements IClassFixture<TUseCaseFixture>)
```

---

## Test Naming Conventions

- **Test methods**: `GivenContext_WhenCondition_ThenExpectedResult` — no underscores inside segments
  - `GivenAValidRequestThenPass`
  - `GivenAnInvalidRequestThenFails`
  - `GivenAValidRequestWhenOrderNotFoundThenFails`
  - `GivenAValidRequestWhenRepositoryReturnsZeroThenFails`
- **All `[Fact]` attributes must include `DisplayName = nameof(MethodName)`**
- **Fixture classes**: `{UseCaseName}Fixture` (e.g., `CreateOrderUseCaseFixture`)
- **Test classes**: `{UseCaseName}Test` (e.g., `CreateOrderUseCaseTest`)
- **Validation fixture**: `{RequestName}ValidationFixture`
- **Validation tests**: `{RequestName}ValidationTests`

---

## Test Infrastructure API

### BaseApplicationFixture<TRequest, TUseCase>

```csharp
// Available properties
Mock<IServiceProvider> MockServiceProvider
Mock<ILogger>          MockLogger
Mock<ILoggerFactory>   MockLoggerFactory
Mock<IProduceService>  MockProduceService
Mock<IBaseRepository>  MockRepository
Mock<IValidator<TRequest>> MockValidator
Mock<IHybridCacheService>  MockCache
TUseCase UseCase  // set in fixture constructor

// Available methods
void ClearInvocations()
void SetSuccessfulValidator(TRequest request)
void SetFailedValidator(TRequest request)
BasePaginatedRequest SetValidBasePaginatedRequest()  // for GetAll use cases
void SetValidGetOrCreateAsync<TResult>(TResult result)
void SetInvalidGetOrCreateAsync<TResult>()
void VerifyCache<TResult>(int times)
void VerifyProduce<TMessage>(int times = 1)
CancellationToken CancellationToken  // from BaseFixture
IFixture AutoFixture                  // from BaseFixture
```

### RepositoryMockExtensions

```csharp
// Add
mockRepository.SetSuccessfulAddAsync<TEntity>()    // returns 1
mockRepository.SetFailedAddAsync<TEntity>()         // returns 0
mockRepository.VerifyAddAsync<TEntity>(int times)

// Update
mockRepository.SetSuccessfulUpdate<TEntity>()       // returns 1
mockRepository.SetFailedUpdate<TEntity>()           // returns 0
mockRepository.VerifyUpdate<TEntity>(int times)

// Query (GetQueryable — used by Get and Delete use cases)
mockRepository.SetupQueryable<TEntity>(ICollection<TEntity> entities)
mockRepository.SetupQueryable<TEntity>(Guid correlationId, bool? newContext, ICollection<TEntity> entities)
mockRepository.VerifyQueryable<TEntity>(int times = 1)

// GetAll paginated
mockRepository.SetValidGetAllPaginatedAsyncNoIncludes<TEntity, TDto>(IEnumerable<TDto> data, int totalRecords)
mockRepository.SetInvalidGetAllPaginatedAsync<TEntity, TDto>()
mockRepository.VerifyGetAllPaginatedNoIncludes<TEntity, TDto>(int times = 1)
```

### LogMockExtensions

```csharp
mockLogger.VerifyStartOperation(int times = 1)     // "Starting operation"
mockLogger.VerifyFinishOperation(int times = 1)    // "Finished operation"
mockLogger.VerifyNotFound(int times = 1)           // "not found."
mockLogger.VerifyOperationFailed(int times = 1)    // "Failed operation"
mockLogger.VerifyInformation(string message, int times = 1)
mockLogger.VerifyWarning(string message, int times = 1)
mockLogger.VerifyError(string message, int times = 1)
mockLogger.VerifyDebug(string message, int times = 1)
```

---

## Pattern 1 — BaseInOutUseCase (Create)

Use this pattern for **create** use cases that take a request and return a typed response.

Includes: validator fixture, validator tests, use case fixture, and use case tests.

> Reference: [`./references/test-create-pattern.md`](./references/test-create-pattern.md)

---

## Pattern 2 — BaseInOutUseCase (Get by Id)

Use this pattern for **get single entity** use cases that query the repository and can return not-found.

Scenarios: valid + found, validator fails, entity not found.

> Reference: [`./references/test-get-pattern.md`](./references/test-get-pattern.md)

---

## Pattern 3 — BaseInOutUseCase (GetAll Paginated)

Use this pattern for **paginated list** use cases. The request type is `BasePaginatedRequest`.

Scenarios: valid + records found, valid + no records found, validator fails.

> Reference: [`./references/test-getall-pattern.md`](./references/test-getall-pattern.md)

---

## Pattern 4 — BaseInOutUseCase (Update)

Use this pattern for **update** use cases that query, mutate, then save the entity.

Scenarios: valid + updated, validator fails, entity not found, repository returns 0.

> Reference: [`./references/test-update-pattern.md`](./references/test-update-pattern.md)

---

## Pattern 5 — BaseInOutUseCase (Delete / Soft Delete)

Delete use cases fetch the entity, apply domain logic (e.g., `DeleteOrder()`), then call `UpdateAsync`. Test the state-check guard.

Scenarios: valid + deleted, validator fails, entity not found, already deleted guard, repository returns 0.

> Reference: [`./references/test-delete-pattern.md`](./references/test-delete-pattern.md)

---

## Pattern 6 — BaseInUseCase (Fire and Forget)

`BaseInUseCase<TRequest>` returns `Task` (void). Assert via repository and logger mocks — no `result.Success`.

Scenarios: valid request (verify side effects), invalid request (verify nothing happens).

> Reference: [`./references/test-basein-pattern.md`](./references/test-basein-pattern.md)

---

## Required Test Scenarios by Use Case Type

| Use Case Type | Required Scenarios |
|---|---|
| **Create** | Valid request passes, Invalid request fails (validator), Repository returns 0 (fails), Domain guard violation (if any) |
| **Get by Id** | Valid + entity found, Invalid request (validator fails), Entity not found |
| **GetAll** | Valid + records found, Valid + no records found, Invalid request (validator fails) |
| **Update** | Valid request passes (data returned), Invalid request (validator fails), Entity not found, Repository returns 0 (fails), Optional: domain state guard |
| **Delete** | Valid request passes, Invalid request (validator fails), Entity not found, Entity already deleted (if soft-delete), Repository returns 0 (fails) |
| **BaseInUseCase** | Valid request (verify side effects), Invalid request (verify nothing happens) |

---

## Implementation Rules

### Always

- Call `_fixture.ClearInvocations()` in the test class constructor
- Add `[Fact(DisplayName = nameof(MethodName))]` on every test
- Assert both `result.Success` **and** `result.Message` for failure cases
- Call `VerifyStartOperation()` on every test (it always logs this)
- Call `VerifyFinishOperation(0)` when validation fails (it skips the internal execution)
- Call `VerifyFinishOperation()` even when business logic fails after validation (it still runs)
- Use `SetupQueryable(request.CorrelationId, null, [entities])` — always pass `correlationId` and `null` for newContext

### Never

- Do not use `Times.Once()` — use `Times.Exactly(n)` through the extension methods (`1` or `0`)
- Do not call `_fixture.MockRepository.VerifyQueryable<T>()` unless the use case actually queries
- Do not assert `result.Data` on failure cases
- Do not set up repository mocks when the test expects validation to fail

### Fixture Static vs Instance Methods

- Use `static` factory methods when the request does not require `AutoFixture` (e.g., `SetValidRequest(int id)` with fixed values)
- Use instance methods when `AutoFixture.CreateMany<T>()` is needed (e.g., generating item collections)

---

## File Placement

```
src/Application/{Context}/{OperationName}UseCase.cs
tests/UnitTests/Application/{Context}/{OperationName}UseCaseTests.cs
```

All classes for one use case (validation fixture, validation tests, use case fixture, use case tests) go in **one test file**.

---

## Quick Checklist

### Use Case
- [ ] Correct base use case selected
- [ ] Request + validator created (if input exists)
- [ ] Async repository calls use provided `CancellationToken`
- [ ] Errors return meaningful messages
- [ ] Notification status uses `NotificationStatus.Success`/`NotificationStatus.Failed`
- [ ] Use case class name ends with `UseCase`
- [ ] Placed under `src/Application/{Context}/`

### Unit Tests
- [ ] All required scenarios covered (see table above)
- [ ] Validator tests present when a validator exists
- [ ] `[Fact(DisplayName = nameof(...))]` on every test method
- [ ] `ClearInvocations()` called in test class constructor
- [ ] Logger and repository mocks verified per test
- [ ] Placed under `tests/UnitTests/Application/{Context}/`
