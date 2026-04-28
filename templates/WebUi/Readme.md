# Hexagonal Architecture Solution Template (Web UI)

This repository provides a ready-to-use **Blazor Web UI** template built with **Hexagonal Architecture** (Ports and Adapters).

The template keeps UI composition in the web app, contracts in a dedicated project, and technical adapters in infrastructure while adding component-level test coverage with **bUnit**.
It also includes a **Mock API** project consumed via HTTP adapter services and **OpenTelemetry** setup for logs, traces, and metrics.

## Architecture at a glance

- **Inbound UI adapter:** `src/WebApp` Blazor Web App components and routing.
- **Contracts (port models):** `src/Contracts` request/response payload models and DTOs.
- **Outbound adapters:** `src/Infrastructure` HTTP service adapters, dependency injection, resilience policies, and OpenTelemetry instrumentation.
- **Mock external service:** `src/MockApi` local HTTP API used by `WebApp` through `Infrastructure`.
- **Quality gates:** `tests/UnitTests` for component and service unit tests using xUnit + bUnit.

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

## Helper commands

### Build solution

```bash
dotnet build Hexagonal.Solution.Template.WebUi.slnx
```

### Run web app

```bash
dotnet run --project src/WebApp/WebApp.csproj
```

### Run mock api

```bash
dotnet run --project src/MockApi/MockApi.csproj
```

### Run unit tests

```bash
dotnet test tests/UnitTests/UnitTests.csproj
```

### Run with Docker Compose

```bash
docker-compose up --build
```

## Notes

- This first version focuses on **Blazor + bUnit** only.
- Browser E2E tests can be added in a future iteration.
- `WebApp` calls `MockApi` at `/orders/summary` using HTTP adapter service(s) in `Infrastructure`.