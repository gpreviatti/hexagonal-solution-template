using System.Diagnostics;
using Application.Common.Constants;

namespace Application.Common.Helpers;

public static class Activities
{
    private static ActivitySource _activitySource => new(DefaultConfigurations.ApplicationName, DefaultConfigurations.Version);
    public static Activity StartActivity(string name) => _activitySource.StartActivity(name) ?? new Activity(name);
}