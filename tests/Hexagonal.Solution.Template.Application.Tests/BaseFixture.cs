using Serilog;

namespace Hexagonal.Solution.Template.Application.Tests;
public class BaseFixture
{
    public Fixture autoFixture = new();

    public CancellationToken cancellationToken = CancellationToken.None;

    public Mock<ILogger> mockLogger = new();

    public BaseFixture()
    {
        autoFixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }
}
