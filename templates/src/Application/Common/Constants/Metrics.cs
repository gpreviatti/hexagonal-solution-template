using System.Diagnostics.Metrics;

namespace Application.Common.Constants;

public static class Metrics
{
    public static Meter Meter = new("Application");
    public static Counter<int> OrderCreated = Meter.CreateCounter<int>("order.created", "orders", "Number of orders created");
    public static Counter<int> OrderRetrieved = Meter.CreateCounter<int>("order.retrieved", "orders", "Number of orders retrieved");
}