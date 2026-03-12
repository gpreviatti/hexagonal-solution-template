using System.Diagnostics;
using Application.Common.Constants;

namespace Application.Common.Helpers;

public static class Activities
{
    public static ActivitySource ActivitySource => new(DefaultConfigurations.ApplicationName, DefaultConfigurations.Version);
    public static Activity StartActivity(string name) => ActivitySource.StartActivity(name) ?? new Activity(name);
}