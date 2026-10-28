# AGENTS.md — WebApp

Rules for AI coding agents working in `src/WebApp/`. Shared rules are in [AGENTS.md](../../AGENTS.md). Patterns are in [Development Patterns](../../Readme.md#-development-patterns).

Responsibility: HTTP entry point. Minimal API endpoints, middleware, and composition in `Program.cs`.

Run: `dotnet run --project src/WebApp` (listens on `https://localhost:7000`).

## Rules

- Endpoints are static `internal` classes in `Endpoints/`, registered in `EndpointExtensions.cs`.
- Read `correlationId` from the header and tag the current activity with it. Include correlation IDs in endpoint handlers and their logs.
- Use the standard status codes for each route (see [Development Patterns](../../Readme.md#endpoints)).
- Add a sample for every new endpoint in `WebApp.http`.

## Feature checklist

- Endpoints, a registration line in `EndpointExtensions.cs`, and samples in `WebApp.http`.
- HTTP integration tests in `tests/IntegrationTests/WebApp/`.
