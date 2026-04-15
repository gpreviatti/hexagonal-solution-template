---
name: load-tests-generator
description: 'Generate k6 load tests for HTTP and gRPC endpoints in this template. Use when creating performance scenarios, thresholds, metrics, and environment-driven load profiles under tests/LoadTests.'
---

# Load Tests Generator — Hexagonal Architecture Template

Generate k6 load-testing scripts that follow the repository patterns for scenarios, thresholds, custom metrics, and env-based tuning.

## When to Use

Activate this skill when:
- User asks to create or update load tests
- User asks to benchmark HTTP or gRPC endpoint performance
- User needs custom thresholds/SLAs in k6
- User asks for performance smoke/stress test scripts
- User asks for env-configurable VUs/duration/graceful stop

## Skill-local References

| File | Purpose |
|------|---------|
| `./references/http-k6-template.js.md` | HTTP k6 script template |
| `./references/grpc-k6-template.js.md` | gRPC k6 script template |
| `./references/run-commands.md` | k6 run command matrix |

> Keep this skill resilient by using these local references only.

---

## Project Conventions

- Place scripts in `tests/LoadTests/`
- Reuse env vars where possible:
  - `VUS`
  - `DURATION`
  - `GRACEFUL_STOP`
  - `WEBAPP_URL` (HTTP)
  - `WEBAPP_GRPC_URL` (gRPC)
- Add custom metrics (`Counter`, `Trend`, `Rate`) for scenario-level observability
- Define strict thresholds for latency and success rate
- Include `correlationId` when endpoint contracts require it

---

## Threshold Guidance

Start with realistic defaults and tighten over time:
- HTTP:
  - `http_req_duration` p95 < 500ms
  - `http_req_failed` rate < 10%
- gRPC:
  - `grpc_req_duration` p95 < 100ms
  - success rate > 95%

---

## Output Expectations

When used, this skill should generate:
- New or updated `scriptHttp.js`/`scriptGrpc.js` style k6 script
- Scenario block with env-driven runtime config
- Custom counters/trends/rates and explicit checks
- Thresholds aligned with scenario objectives

---

## Checklist

- [ ] Script saved under `tests/LoadTests/`
- [ ] Uses env variables for load profile
- [ ] Defines scenario + thresholds + metrics
- [ ] Uses endpoint/proto paths that exist in repo
- [ ] Run command verified via `./references/run-commands.md`
