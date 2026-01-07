namespace Infrastructure.Http;

public static class ServicesKeys
{
    public const string Orders = nameof(Orders);
    public const string Payments = nameof(Payments);
}

public sealed class ServiceConfigurations
{
    public string Name { get; set; }
    public string Url { get; set; }
    public object Authentication { get; set; }
}