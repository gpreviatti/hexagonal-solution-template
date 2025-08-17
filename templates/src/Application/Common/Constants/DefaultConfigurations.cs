using System.Diagnostics.Metrics;

namespace Application.Common.Constants;

public static class DefaultConfigurations
{
    public static string ApplicationName => "Hexagonal.Solution.Template";
    public static Meter Meter = new("Application");

}