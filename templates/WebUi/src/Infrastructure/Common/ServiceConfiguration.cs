namespace Infrastructure.Common;

public sealed class ServiceConfiguration
{
    public string Name { get; set; } = string.Empty;

    public string BaseAddress { get; set; } = string.Empty;

    public object? Authentication { get; set; }

    public object? Headers { get; set; }

    public int LimitPerMinute { get; set; }

    public int ProtocolVersion { get; set; } = 2;
}