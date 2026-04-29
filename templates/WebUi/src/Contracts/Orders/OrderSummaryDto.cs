namespace Contracts.Orders;

public sealed record OrderSummaryDto(int TotalOrders, decimal TotalRevenue, string Currency);
