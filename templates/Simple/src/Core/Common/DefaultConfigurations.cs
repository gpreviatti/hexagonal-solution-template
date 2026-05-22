using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Core.Common;

public static class DefaultConfigurations
{
    public static string CoreName => "Hexagonal.Solution.Template";
    public static string Version => typeof(DefaultConfigurations).Assembly.GetName().Version!.ToString();
    public static readonly Meter Meter = new(CoreName, Version);
    public static readonly ActivitySource ActivitySource = new(CoreName, Version);
}
