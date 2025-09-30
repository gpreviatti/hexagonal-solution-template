# Hexagonal Architecture Solution Template

This repository provides a template for building applications using the Hexagonal Architecture (also known as Ports and Adapters Architecture). The template includes a well-structured project layout, sample implementations, and best practices to help you get started quickly.

## Project Structure

- `src/`: Contains the source code for the application.
    - `Domain/`: Contains the core business logic and domain entities.
    - `Application/`: Contains use cases and application services.
    - `Infrastructure/`: Contains implementations for external systems (e.g., databases, messaging).
    - `WebApp/`: Contains the web application layer (e.g., REST API, gRPC services).

- `tests/`: Contains unit and integration tests for the application.
    - `CommonTests/`: Contains shared test utilities and base classes.
    - `UnitTests/`: Contains unit tests for individual components.
    - `IntegrationTests/`: Contains integration tests for the application.
    - `LoadTests/`: Contains load tests for performance evaluation.

## Helper Commands

### Run migrations

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/WebApp --output-dir Data/Migrations
```

### Create a new migration

```bash
dotnet ef migrations add <MigrationName> --project src/Infrastructure --startup-project src/WebApp --output-dir Data/Migrations
```
### Run the application

```bash
dotnet run --project src/WebApp
```

### Run load tests with full summary

```bash
k6 run tests/LoadTests/script.js --summary-mode=full
```

### Run unit tests

```bash
dotnet test tests/UnitTests
```

### Run integration tests

```bash
dotnet test tests/IntegrationTests
```

### Start docker compose

```bash
docker-compose up -d
```