namespace Infrastructure.Http;

public sealed class ServiceConfigurations
{
    public string Name { get; set; }
    public string BaseAddress { get; set; }
    public object Authentication { get; set; }
    public object Headers { get; set; }
    public int LimitPerMinute { get; set; }
}