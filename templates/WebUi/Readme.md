# Hexagonal Architecture Solution Template (Web UI)

This repository provides a ready-to-use **Blazor Web UI** template built with **Hexagonal Architecture** (Ports and Adapters) on **.NET 10**.

The template keeps UI composition in the web app, contracts in a dedicated project, and technical adapters in infrastructure while adding component-level test coverage with **bUnit**.
It also includes a **Mock API** project consumed via HTTP adapter services, comprehensive **OpenTelemetry** instrumentation (logs, traces, metrics, profiling), and a full observability stack with **Grafana**, **Loki**, **Prometheus**, **Tempo**, and **Pyroscope**.

## Architecture at a glance

- **Inbound UI adapter:** `src/WebApp` Blazor Web App with interactive server-side components and routing.
- **Contracts (port models):** `src/Contracts` request/response payload models and DTOs.
- **Outbound adapters:** `src/Infrastructure` HTTP service adapters, dependency injection, Polly resilience policies, and OpenTelemetry instrumentation (metrics, traces, structured logs).
- **Mock external service:** `src/MockApi` local HTTP API used by `WebApp` through `Infrastructure` adapters.
- **Quality gates:** `tests/UnitTests` for component and service unit tests using xUnit + bUnit.
- **Observability:** Docker Compose stack with Grafana, Loki (logs), Prometheus (metrics), Tempo (traces), Alloy (collector), and Pyroscope (continuous profiling).

## Project structure (tree view)

```text
.
├── src
│   ├── Contracts
│   │   └── Orders
│   ├── Infrastructure
│   │   ├── Common
│   │   ├── Http
│   ├── MockApi
│   │   └── Endpoints
│   └── WebApp
│       ├── Components
│       │   ├── Layout
│       │   └── Pages
│       └── wwwroot
└── tests
    └── UnitTests
        └── WebApp
```
Quick start

### Prerequisites

- **.NET 10 SDK** or later
- **Docker** and **Docker Compose** (for local observability stack)

### Build solution

```bash
dotnet build Hexagonal.Solution.Template.WebUi.slnx
```

### Run locally (development)

Start the observability stack in a separate terminal:

```bash
docker-compose -f docker-compose-local.yml up --build
```

Then run the **Mock API** in one terminal:

```bash
dotnet run --project src/MockApi/MockApi.csproj
```

And the **Web App** in another terminal:

```bash
dotnet run --project src/WebApp/WebApp.csproj
```

TheKey features & practices

### Code quality
- **.NET 10** with nullable reference types enabled and implicit usings
- **Strict analysis** enabled: `TreatWarningsAsErrors=true` and `CodeAnalysisTreatWarningsAsErrors=true`
- **Latest analysis level** for static code analysis recommendations
- **Package lock files** for reproducible builds

### Resilience & HTTP communication
- **Polly** for resilience policies (retries, circuit breaker, timeout patterns)
- **Generic HTTP service abstraction** (`IBaseHttpService`) with structured error handling
- **HTTP/2 support** with version negotiation and proper protocol handling
- **Typed service configuration** via dependency injection

### Observability & monitoring
- **OpenTelemetry** instrumentation for traces, metrics, and logs
- **Structured logging** with correlation IDs and context
- **Continuous profiling** with Pyroscope (CPU, memory, allocations, exceptions)
- **Metrics collection** from ASP.NET Core, HttpClient, runtime, and process
- **Distributed tracing** with correlation across services
- **Grafana Alloy** as unified data collector
- **Loki** for scalable log aggregation
- **Prometheus** for metrics scraping and querying
- **Tempo** for trace storage and querying

### UI architecture
- **Blazor Web App** with interactive server-side rendering
- **Component-based** architecture with layout abstraction
- **Interactive server render mode** for real-time interactivity
- **Status code pages** with graceful error handling

### Testing
- **bUnit** for component-level integration tests
- **xUnit** as test framework
- **Unit tests** for service adapters and business logic

## Development workflow

1. **Local development:** Run Mock API and Web App separately with hot-reload capability
2. **Observability:** Use Docker Compose to spin up the full observability stack
3. **Testing:** Run unit tests frequently and use bUnit for component testing
4. **Containerization:** Use provided Dockerfiles for production deployment

## Future considerations

- Browser E2E tests (Playwright or Cypress)
- API versioning strategies
- Advanced caching patterns
- Additional resilience patterns (bulkhead, rate limiting)
- Custom health checks and diagnostics endpoints
docker-compose up --build
```

### Run unit tests

```bash
dotnet test tests/UnitTests/UnitTests.csproj
```

### View observability data

Once the observability stack is running:

- **Grafana:** http://localhost:3000 (dashboards for metrics, logs, traces)
- **Prometheus:** http://localhost:9090 (metrics explorer)
- **Tempo:** http://localhost:3200 (trace visualization)
- **Pyroscope:** http://localhost:4040 (continuous profiling)ker-compose up --build
```

## Notes

- This first version focuses on **Blazor + bUnit** only.
- Browser E2E tests can be added in a future iteration.
- `WebApp` calls `MockApi` at `/orders/summary` using HTTP adapter service(s) in `Infrastructure`.
