using AutoFixture;

namespace CommonTests.Fixtures;
public class BaseFixture
{
    public Fixture autoFixture = new();

    public CancellationToken cancellationToken = CancellationToken.None;

    public BaseFixture()
    {
        autoFixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }
}
