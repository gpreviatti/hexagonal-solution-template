namespace Contracts.Orders;

/// <summary>
/// Represents a summary of orders, including total number of orders, total revenue, and currency.
/// </summary>
/// <param name="TotalOrders"></param>
/// <param name="TotalRevenue"></param>
/// <param name="Currency"></param>
public sealed record OrderSummaryDto(int TotalOrders, decimal TotalRevenue, string Currency);
