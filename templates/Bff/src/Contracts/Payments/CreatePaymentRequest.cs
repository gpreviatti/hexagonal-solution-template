using Contracts.Common;

namespace Contracts.Payments;

/// <summary>
/// Request to create a payment request
/// /// </summary>
/// <param name="CorrelationId">Correlation identifier for tracking the request</param>
/// <param name="OrderId">Unique identifier for the order</param>
/// <param name="Amount">Amount to be paid</param>
/// <param name="Currency">Currency of the payment</param>
/// <param name="PaymentMethod">Method of payment</param>
public sealed record CreatePaymentRequest(
    Guid CorrelationId,
    int OrderId,
    double Amount,
    string Currency,
    string PaymentMethod
) : BaseRequest(CorrelationId);