# Use Case Test Class Template

```csharp
public sealed class {UseCaseName}Fixture : BaseApplicationFixture<{RequestType}, {UseCaseName}>
{
    public {UseCaseName}Fixture() => UseCase = new(MockServiceProvider.Object);
}

public sealed class {UseCaseName}Tests : IClassFixture<{UseCaseName}Fixture>
{
    private readonly {UseCaseName}Fixture _fixture;

    public {UseCaseName}Tests({UseCaseName}Fixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearInvocations();
    }

    [Fact(DisplayName = nameof(GivenAValidRequest_WhenHandleAsync_ThenShouldPass))]
    public async Task GivenAValidRequest_WhenHandleAsync_ThenShouldPass()
    {
        var request = new {RequestType}(Guid.NewGuid());
        _fixture.SetSuccessfulValidator(request);

        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        Assert.True(result.Success);
    }
}
```
