using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Application.Common.Constants;

public static class DefaultConfigurations
{
    public static string ApplicationName => "Hexagonal.Solution.Template";
    public static string Version => typeof(DefaultConfigurations).Assembly.GetName().Version!.ToString();
    public static ActivitySource ActivitySource => new(ApplicationName, Version);
    public static readonly Meter Meter = new("Application");
}
