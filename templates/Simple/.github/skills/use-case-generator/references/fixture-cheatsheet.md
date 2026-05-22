# Fixture Cheatsheet

- Extend `BaseApplicationFixture<TRequest, TUseCase>`
- In constructor: `UseCase = new(MockServiceProvider.Object);`
- Before each test: `_fixture.ClearInvocations();`
- Validation setup:
  - `_fixture.SetSuccessfulValidator(request);`
  - `_fixture.SetFailedValidator(request);`
- Repository setup:
  - `SetSuccessfulAddAsync<T>()`, `SetFailedAddAsync<T>()`
  - `SetSuccessfulUpdate<T>()`, `SetFailedUpdate<T>()`
  - `SetupQueryable<T>(...)`
- Verifications:
  - `_fixture.MockLogger.VerifyStartOperation();`
  - `_fixture.MockLogger.VerifyFinishOperation();`
  - `_fixture.VerifyProduce<CreateNotificationMessage>();`
