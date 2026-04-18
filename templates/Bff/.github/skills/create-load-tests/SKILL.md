---
name: create-load-tests
description: "Create or extend k6 load tests for this BFF template. Use when user asks for HTTP/gRPC performance scenarios, thresholds, custom metrics, or environment-driven load profiles in tests/LoadTests."
---

# Create Load Tests (k6)

Create or update k6 scripts for HTTP and gRPC endpoints, with reusable environment-based settings and meaningful thresholds.

## When to use

Activate this skill when the user asks to:
- add load tests for a new endpoint
- tune VUs/duration/thresholds
- add custom metrics (counter, trend, rate)
- validate throughput/latency behavior under load

## Target files

- `tests/LoadTests/scriptHttp.js` — HTTP scenarios using `k6/http`; currently has `get_order` scenario hitting `GET /orders/1`
- `tests/LoadTests/scriptGrpc.js` — gRPC scenarios using `k6/net/grpc`; currently has `get_order` scenario invoking `order.OrderService/Get`
- `tests/LoadTests/protos/*.proto` — proto definitions loaded by the gRPC client (currently `order.proto`)

## k6 conventions in this template

- Keep `options.scenarios` and `options.thresholds` explicit.
- Support env-driven test profile: `VUS` (default 10), `DURATION` (default `'60s'`), `GRACEFUL_STOP` (default `'10s'`), `WEBAPP_URL` (default `https://localhost:7175`), `WEBAPP_GRPC_URL` (default `localhost:7175`).
- Per-scenario custom metrics follow the naming pattern `<scenario>_requests_total` (Counter), `<scenario>_response_time` (Trend), `<scenario>_success_rate` (Rate).
- HTTP headers always include `correlationId: crypto.randomUUID()`, `Accept: application/json`, `CacheEnabled: false`.
- gRPC client is instantiated at module level; `client.load([], './protos/<file>.proto')` before `export function`.
- HTTP thresholds: `http_req_duration p(95) < 500`, `http_req_failed rate < 0.1`.
- gRPC thresholds: `grpc_req_duration p(95) < 100`.
- Keep checks focused on status and minimal payload validity.

## Workflow

1. Identify target endpoint(s) and success criteria.
2. Add/update scenario function(s) in `scriptHttp.js` and/or `scriptGrpc.js`.
3. Add/update thresholds for latency, errors, and throughput.
4. Add/update proto contracts under `tests/LoadTests/protos` when needed.
5. Run the k6 script and tune thresholds.

## Done checklist

- [ ] Scenario names and metrics are descriptive
- [ ] Thresholds reflect realistic SLO expectations
- [ ] Env vars control VUs/duration and base URLs
- [ ] Checks validate status + key payload behavior
- [ ] Script runs successfully with full summary

## Practical examples

### Example 1 — New HTTP scenario

**User asks:**
- "Create a k6 scenario for POST /orders"

**Expected file changes:**
- `tests/LoadTests/scriptHttp.js` (updated)

**Real project reference:**
```js
// Pattern from tests/LoadTests/scriptHttp.js
const createOrderRequestsCounter = new Counter('create_order_requests_total');
const createOrderResponseTime = new Trend('create_order_response_time');
const createOrderSuccessRate = new Rate('create_order_success_rate');

export function createOrder() {
  createOrderRequestsCounter.add(1);

  const payload = JSON.stringify({
    correlationId: crypto.randomUUID(),
    description: 'Load test order',
    items: [{ name: 'Item 1', description: 'Desc', value: 9.99 }]
  });

  const res = http.post(`${webappUrl}/orders`, payload, {
    headers: {
      'Content-Type': 'application/json',
      'correlationId': crypto.randomUUID(),
      'CacheEnabled': 'false'
    }
  });

  createOrderResponseTime.add(res.timings.duration);
  const ok = check(res, {
    'status is 200': (r) => r.status === 200,
    'has data': (r) => JSON.parse(r.body)?.data != null
  });
  createOrderSuccessRate.add(ok);
  sleep(1);
}
```

**Acceptance checks:**
- Scenario has explicit executor, VUs, duration driven by env vars
- Adds `Counter`, `Trend`, and `Rate` metrics with `create_order_` prefix
- Adds checks for status 200 and key response fields

### Example 2 — Tune thresholds for existing scenario

**User asks:**
- "Make GET /orders load test stricter for p95 latency"

**Expected file changes:**
- `tests/LoadTests/scriptHttp.js` (updated)

**Real project reference:**
```js
// Current threshold in scriptHttp.js:
get_order_response_time: ['p(95) < 500'],
// After tightening:
get_order_response_time: ['p(95) < 200'],
```

**Acceptance checks:**
- Threshold keys map to existing metric names (`get_order_response_time`, etc.)
- Threshold values match intended SLO
- Script remains runnable with env overrides

### Example 3 — Add gRPC performance path

**User asks:**
- "Load test CreatePayment gRPC method"

**Expected file changes:**
- `tests/LoadTests/scriptGrpc.js` (updated)
- `tests/LoadTests/protos/payment.proto` (add if not present — mirrors `src/Contracts/Protos/payment.proto`)

**Real project reference:**
```js
// Pattern from tests/LoadTests/scriptGrpc.js
client.load([], './protos/payment.proto');

const createPaymentCounter = new Counter('create_payment_requests_total');
const createPaymentResponseTime = new Trend('create_payment_response_time');
const createPaymentSuccessRate = new Rate('create_payment_success_rate');

export function createPayment() {
  createPaymentCounter.add(1);
  client.connect(webappUrl, { plaintext: false });

  const start = Date.now();
  const response = client.invoke('Protos.PaymentService/Create', {
    order_id: 1,
    amount: 99.99,
    correlation_id: crypto.randomUUID(),
    currency: 'USD',
    payment_method: 'credit_card'
  });
  createPaymentResponseTime.add(Date.now() - start);

  const ok = check(response, {
    'status is OK': (r) => r && r.status === grpc.StatusOK,
    'success': (r) => r.message?.success === true
  });
  createPaymentSuccessRate.add(ok);
  client.close();
  sleep(1);
}
```

**Acceptance checks:**
- gRPC client loads correct proto (`payment.proto`) and invokes `Protos.PaymentService/Create`
- Checks validate `grpc.StatusOK` and `message.success === true`
- Scenario reports response-time and success-rate metrics
