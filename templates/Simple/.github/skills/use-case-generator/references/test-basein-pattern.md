# Test Pattern — BaseInUseCase (Fire and Forget)

`BaseInUseCase<TRequest>` returns `Task` (void). There is no return value — assert behavior through repository and logger mocks.

```csharp
using Application.Products;
using Domain.Products;
using UnitTests.Application.Common;

namespace UnitTests.Application.Products;

// ── 3. Use Case Fixture ─────────────────────────────────────────────────────

public sealed class SyncProductUseCaseFixture : BaseApplicationFixture<SyncProductRequest, SyncProductUseCase>
{
    public SyncProductUseCaseFixture() => UseCase = new(MockServiceProvider.Object);
    public static SyncProductRequest SetValidRequest() => new(Guid.NewGuid(), 42);
}

// ── 4. Use Case Tests ───────────────────────────────────────────────────────

public sealed class SyncProductUseCaseTest : IClassFixture<SyncProductUseCaseFixture>
{
    private readonly SyncProductUseCaseFixture _fixture;

    public SyncProductUseCaseTest(SyncProductUseCaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearInvocations();
    }

    [Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
    public async Task GivenAValidRequestThenPass()
    {
        var request = SyncProductUseCaseFixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetSuccessfulAddAsync<Product>();

        // BaseInUseCase returns Task (void) — no result to assert
        await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation();
        _fixture.MockRepository.VerifyAddAsync<Product>(1);
    }

    [Fact(DisplayName = nameof(GivenAnInvalidRequestThenDoesNothing))]
    public async Task GivenAnInvalidRequestThenDoesNothing()
    {
        var request = SyncProductUseCaseFixture.SetValidRequest();
        _fixture.SetFailedValidator(request);

        await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation(0);
        _fixture.MockRepository.VerifyAddAsync<Product>(0);
    }
}
```
