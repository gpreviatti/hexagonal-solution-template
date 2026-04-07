using System.Diagnostics;

namespace Domain.Common.Extensions;

public static class ActivityExtensions
{
    public static void SetDefaultTags(this Activity? activity)
    {
        activity?.SetTag("SpanId", activity.SpanId);
        activity?.SetTag("SpanId", activity.ParentSpanId);
        activity?.SetTag("TraceId", activity.TraceId);
        activity?.SetTag("ParentId", activity.ParentId);
    }
}