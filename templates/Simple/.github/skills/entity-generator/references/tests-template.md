# Domain Tests Template

```csharp
namespace UnitTests.Domain;

public sealed class {EntityName}Tests
{
    [Fact(DisplayName = nameof(GivenAValidInput_WhenCreate_ThenShouldSucceed))]
    public void GivenAValidInput_WhenCreate_ThenShouldSucceed()
    {
        var result = {EntityName}.Create("valid");

        Assert.True(result.Success);
        Assert.NotNull(result.Value);
    }

    [Fact(DisplayName = nameof(GivenAnInvalidInput_WhenCreate_ThenShouldFail))]
    public void GivenAnInvalidInput_WhenCreate_ThenShouldFail()
    {
        var result = {EntityName}.Create(string.Empty);

        Assert.False(result.Success);
        Assert.NotEmpty(result.Message);
    }
}
```