# Hexagonal Architecture Solution Template

This repository provides a template for building applications using the Hexagonal Architecture (also known as Ports and Adapters Architecture). The template includes a well-structured project layout, sample implementations, and best practices to help you get started quickly.

## Project Structure

- `src/`: Contains the source code for the application.
  - `Infrastructure/`: Contains implementations for external systems (e.g., databases, messaging).
  - `WebApp/`: Contains the web application layer (e.g., REST API, gRPC services).

- `tests/`: Contains unit and integration tests for the application.
  - `CommonTests/`: Contains shared test utilities and base classes.
  - `IntegrationTests/`: Contains integration tests for the application.
  - `LoadTests/`: Contains load tests for performance evaluation.

## Helper Commands

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

### Run unit tests

```bash
dotnet test tests/UnitTests
```

### Run integration tests

```bash
dotnet test tests/IntegrationTests
```

## Contribute

Have a feature request or found a bug? We'd love to hear from you!

- [Report a Bug](https://github.com/gpreviatti/hexagonal-solution-template/issues/new?template=bug_report.md)
- [Request a Feature](https://github.com/gpreviatti/hexagonal-solution-template/issues/new?template=feature_request.md)
