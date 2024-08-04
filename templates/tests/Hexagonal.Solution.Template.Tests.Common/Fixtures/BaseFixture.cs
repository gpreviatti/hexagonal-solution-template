using AutoFixture;

namespace Hexagonal.Solution.Template.Application.Tests.Common;
public class BaseFixture
{
    public Fixture autoFixture = new();

    public CancellationToken cancellationToken = CancellationToken.None;

    public BaseFixture()
    {
        autoFixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }
}
