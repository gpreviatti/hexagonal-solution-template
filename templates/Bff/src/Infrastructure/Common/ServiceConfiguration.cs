namespace Infrastructure.Common;

public sealed class ServiceConfiguration
{
    public string Name { get; set; }
    public string BaseAddress { get; set; }
    public object Authentication { get; set; }
    public object Headers { get; set; }
    public int LimitPerMinute { get; set; }
}