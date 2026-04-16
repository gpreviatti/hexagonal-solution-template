---
name: create-contracts
description: "Create or update BFF contracts in src/Contracts (records, DTOs, and .proto). Use when user asks to add/change request/response contracts, order DTOs, base contract wrappers, or gRPC proto contracts."
---

# Create Contracts (BFF)

Create or update contracts used by inbound APIs, outbound adapters, and tests.

## When to use

Activate this skill when the user asks to:
- add a new contract record/DTO
- change request/response payload shapes
- add or update a gRPC `.proto` contract
- align API models between `WebApp`, `Infrastructure`, and tests

## Target files

- `src/Contracts/Common/BaseRequest.cs` — `record BaseRequest(Guid CorrelationId)` and `BasePaginatedRequest`
- `src/Contracts/Common/BaseResponse.cs` — `record BaseResponse { bool Success; string? Message; }` and `BaseResponse<T>`
- `src/Contracts/Orders/CreateOrderRequest.cs` — `sealed record CreateOrderRequest(Guid CorrelationId, string Description, CreateOrderItemRequest[] Items) : BaseRequest`
- `src/Contracts/Orders/OrderDto.cs` — `sealed record OrderDto { int Id; string Description; decimal Total; IReadOnlyCollection<ItemDto>? Items; }`
- `src/Contracts/<Feature>/*.cs` — new feature contracts follow the same pattern
- `src/Contracts/Protos/payment.proto` — `PaymentService.Create(CreatePaymentRequest) returns (PaymentReply)` with fields: `order_id`, `amount`, `correlation_id`, `currency`, `payment_method`

## Contract conventions

- All HTTP request records inherit from `BaseRequest(Guid CorrelationId)`.
- Paginated requests inherit from `BasePaginatedRequest` (adds `Page`, `PageSize`, `SortBy`, `SortDescending`, `SearchByValues`).
- Prefer immutable request models (`record` / `sealed record`).
- Keep XML documentation for public contract types and fields.
- Keep `CorrelationId` consistency in request contracts — it is always a `Guid` positional parameter.
- Use `decimal` for monetary values in C# contracts (e.g., `CreateOrderItemRequest.Value`, `OrderDto.Total`); proto uses `double` for amounts.
- For gRPC contracts, keep field numbers stable; add new fields with new numbers. Never reuse or remove field tags.

## Workflow

1. Confirm domain vocabulary and required payload shape.
2. Update/create C# contracts under `src/Contracts`.
3. Update/create `.proto` contracts when gRPC is involved.
4. Ensure impacted endpoint/service code compiles with the new contracts.
5. Add or update integration tests if payload behavior changes.

## Practical examples

### Example 1 — New HTTP contract pair

**User asks:**
- "Add contracts for canceling an order by id with reason"

**Recommended delegation:**
- `@runSubagent Woven Engineer Agent Create CancelOrderRequest and CancelOrderResponse under src/Contracts/Orders with XML docs and CorrelationId consistency`

**Expected file changes:**
- `src/Contracts/Orders/CancelOrderRequest.cs` (new) — `sealed record CancelOrderRequest(Guid CorrelationId, int Id, string Reason) : BaseRequest(CorrelationId)`
- `src/Contracts/Orders/CancelOrderResponse.cs` (new) — inherits or wraps `BaseResponse`

**Real project reference:**
```csharp
// Pattern from src/Contracts/Orders/CreateOrderRequest.cs
public sealed record CancelOrderRequest(
    Guid CorrelationId,
    int Id,
    string Reason
) : BaseRequest(CorrelationId);
```

**Acceptance checks:**
- Request inherits from `BaseRequest(Guid CorrelationId)`
- Response contract is documented and serializable
- Monetary fields (if any) use `decimal`

### Example 2 — Extend existing DTO

**User asks:**
- "Include status and cancelledAt on OrderDto"

**Recommended delegation:**
- `@runSubagent Woven Engineer Agent Update src/Contracts/Orders/OrderDto.cs adding Status and CancelledAt with XML comments and non-breaking defaults`

**Expected file changes:**
- `src/Contracts/Orders/OrderDto.cs` (updated)

**Real project reference:**
```csharp
// Pattern from src/Contracts/Orders/OrderDto.cs
public sealed record OrderDto
{
    public int Id { get; set; }
    public string Description { get; set; }
    public decimal Total { get; set; }
    public IReadOnlyCollection<ItemDto>? Items { get; set; }
    // New fields:
    public string? Status { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
}
```

**Acceptance checks:**
- New members are documented with XML `<summary>`
- Existing serialization remains backward-compatible

### Example 3 — gRPC contract evolution

**User asks:**
- "Add payment_reference and card_brand to payment proto"

**Recommended delegation:**
- `@runSubagent Woven Engineer Agent Update src/Contracts/Protos/payment.proto with payment_reference and card_brand preserving existing field numbers`

**Expected file changes:**
- `src/Contracts/Protos/payment.proto` (updated)

**Real project reference:**
```proto
// Existing fields in payment.proto (must not change):
// order_id = 1, amount = 2, correlation_id = 3, currency = 4, payment_method = 5
// Add only:
string payment_reference = 6;
string card_brand = 7;
```

**Acceptance checks:**
- Existing field numbers 1–5 are unchanged
- New fields use tags 6+
- No message/field removals that break compatibility

## Done checklist

- [ ] New/updated contracts compile
- [ ] XML docs are present on public contract members
- [ ] Correlation and money fields follow conventions
- [ ] Proto changes are backward-compatible (no field-number reuse)
- [ ] Affected tests are updated
