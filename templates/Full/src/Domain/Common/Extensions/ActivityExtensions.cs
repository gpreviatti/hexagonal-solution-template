using System.Diagnostics;

namespace Domain.Common.Extensions;

public static class ActivityExtensions
{
    public static void SetDefaultTags(this Activity? activity)
    {
        activity?.SetTag(nameof(activity.SpanId), activity.SpanId);
        activity?.SetTag(nameof(activity.ParentSpanId), activity.ParentSpanId);
        activity?.SetTag(nameof(activity.TraceId), activity.TraceId);
        activity?.SetTag(nameof(activity.ParentId), activity.ParentId);
        activity?.SetTag(nameof(activity.TraceStateString), activity.TraceStateString);
    }
}
