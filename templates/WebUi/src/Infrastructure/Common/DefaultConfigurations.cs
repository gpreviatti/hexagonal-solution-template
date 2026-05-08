using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Infrastructure.Common;

public static class DefaultConfigurations
{
    public static string ApplicationName => "Hexagonal.Solution.Template.WebUI";
    public static string Version => typeof(DefaultConfigurations).Assembly.GetName().Version!.ToString();
    public static readonly Meter Meter = new(ApplicationName, Version);
    public static readonly ActivitySource ActivitySource = new(ApplicationName, Version);
}
