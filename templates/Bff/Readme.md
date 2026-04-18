# Hexagonal Architecture Solution Template (BFF)

This repository provides a ready-to-use **Backend For Frontend (BFF)** template built with **Hexagonal Architecture** (Ports and Adapters).

The goal is to keep business contracts and API surfaces clean while making adapters (HTTP, gRPC, cache, observability, resilience) easy to evolve independently.

## Table of Contents

- [Architecture at a glance](#architecture-at-a-glance)
- [Project structure (tree view)](#project-structure-tree-view)
- [Detailed project structure](#detailed-project-structure)
  - [Solution root](#solution-root)
  - [`src/Contracts`](#srccontracts)
  - [`src/Infrastructure`](#srcinfrastructure)
  - [`src/WebApp`](#srcwebapp)
  - [`src/MockApi`](#srcmockapi)
  - [`tests/CommonTests`](#testscommontests)
  - [`tests/IntegrationTests`](#testsintegrationtests)
  - [`tests/LoadTests`](#testsloadtests)
- [Copilot skills included in this template](#copilot-skills-included-in-this-template)
  - [Using skills with `@runSubagent`](#using-skills-with-runsubagent)
- [Helper commands](#helper-commands)
- [Contribute](#contribute)

## Architecture at a glance

- **Inbound adapters:** `src/WebApp` minimal APIs and endpoint composition.
- **Contracts (port models):** `src/Contracts` request/response DTOs and gRPC `.proto` files.
- **Outbound adapters:** `src/Infrastructure` HTTP and gRPC service adapters, cache, retry policies, telemetry/logging setup.
- **Mock external systems:** `src/MockApi` local fake downstream HTTP + gRPC APIs.
- **Quality gates:** `tests/CommonTests`, `tests/IntegrationTests`, and `tests/LoadTests`.

## Project structure (tree view)

```text
.
├── src
│   ├── Contracts
│   │   ├── Common
│   │   ├── Orders
│   │   └── Protos
│   ├── Infrastructure
│   │   ├── Cache
│   │   ├── Common
│   │   ├── Grpc
│   │   └── Http
│   ├── MockApi
│   │   ├── Endpoints
│   │   └── GrpcServices
│   └── WebApp
│       ├── Endpoints
│       ├── Extensions
│       └── Middlewares
├── tests
│   ├── CommonTests
│   ├── IntegrationTests
│   │   ├── Common
│   │   ├── Fixtures
│   │   └── WebApp
│   │       ├── Grpc
│   │       └── Http
│   └── LoadTests
│       └── protos
└── scripts
  └── grafana
```

## Detailed project structure

### Solution root

- `Hexagonal.Solution.Template.Bff.slnx`: solution entry point.
- `Directory.Build.props` / `Directory.Packages.props`: centralized build and package management.
- `docker-compose.yml` / `docker-compose-local.yml` / `docker-compose-load-tests.yml`: local and load-test environments.
- `Dockerfile` / `Dockerfile.MockApi`: app and mock API container definitions.
- `scripts/grafana/`: local observability stack configuration (Grafana/Tempo/Loki/Prometheus).

### `src/Contracts`

Shared message contracts used by the BFF and tests.

- `Common/`
  - `BaseRequest.cs`, `BaseResponse.cs`: standard wrappers (correlation-aware base contracts).
- `Orders/`
  - `CreateOrderRequest.cs`, `OrderDto.cs`: order-specific request and response contracts.
- `Protos/`
  - `payment.proto`: gRPC contract for payment flows.

### `src/Infrastructure`

Adapters and cross-cutting technical concerns.

- `InfrastructureDependencyInjection.cs`: DI composition for cache, HTTP clients, gRPC clients, OpenTelemetry, retry policies.
- `Cache/HybridCacheService.cs`: distributed + local cache access strategy.
- `Http/BaseHttpService.cs`: reusable outbound HTTP adapter.
- `Grpc/PaymentsService.cs`: outbound gRPC adapter.
- `Common/`: constants, service keys, logging and default configuration helpers.

### `src/WebApp`

Inbound API layer (Minimal APIs).

- `Program.cs`: app bootstrap, OpenAPI, health checks, rate limiting, middleware pipeline.
- `Endpoints/`
  - `EndpointExtensions.cs`: endpoint registration root.
  - `OrderEndpoints.cs`: order endpoints (GET/POST).
  - `PaymentEndpoints.cs`: payment endpoint(s).
- `Extensions/`, `Middlewares/`: host and pipeline concerns.

### `src/MockApi`

Local fake dependencies used for development and tests.

- `Program.cs`: mock host startup.
- `Endpoints/OrderEndpoints.cs`: fake HTTP order endpoints.
- `GrpcServices/PaymentService.cs`: fake gRPC service.

### `tests/CommonTests`

Common unit-test support package and shared test utilities.

### `tests/IntegrationTests`

End-to-end API integration tests against `WebApp`.

- `Common/CustomWebApplicationFactory.cs`: host factory for test server.
- `WebApp/Http/`: HTTP integration fixtures/helpers/tests.
- `WebApp/Grpc/`: gRPC integration helpers/tests.

### `tests/LoadTests`

k6 scripts for performance validation.

- `scriptHttp.js`: HTTP load scenarios and thresholds.
- `scriptGrpc.js`: gRPC load scenarios and thresholds.
- `protos/`: protobuf contracts used by load tests.

## Copilot skills included in this template

This template now includes dedicated skills under `.github/skills/`:

- `create-contracts`: generate/update contract records and proto definitions.
- `create-endpoint-with-integration-tests`: create a new endpoint and its integration tests.
- `create-load-tests`: create or extend k6 load-test scripts and thresholds.

## Helper commands

### Start docker compose

```bash
docker-compose up -d
```

### Run load tests with full summary for HTTP script

```bash
k6 run tests/LoadTests/scriptHttp.js --summary-mode=full
```

### Run load tests with full summary for gRPC script

```bash
k6 run tests/LoadTests/scriptGrpc.js --summary-mode=full
```

### Run tests (except load tests)

```bash
dotnet test
```

### Run common/unit tests

```bash
dotnet test tests/CommonTests
```

### Run integration tests

```bash
dotnet test tests/IntegrationTests
```

## Contribute

Have a feature request or found a bug? We'd love to hear from you!

- [Report a Bug](https://github.com/gpreviatti/hexagonal-solution-template/issues/new?template=bug_report.md)
- [Request a Feature](https://github.com/gpreviatti/hexagonal-solution-template/issues/new?template=feature_request.md)
