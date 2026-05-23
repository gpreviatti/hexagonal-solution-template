using AutoFixture;

namespace CommonTests.Fixtures;

public class BaseFixture
{
    public Fixture AutoFixture { get; }

    public CancellationToken CancellationToken { get; }

    public BaseFixture()
    {
        AutoFixture = new Fixture();
        AutoFixture.Behaviors.Add(new OmitOnRecursionBehavior());
        CancellationToken = CancellationToken.None;
    }
}
