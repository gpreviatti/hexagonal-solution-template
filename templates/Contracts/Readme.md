# Hexagonal Architecture Contracts Template

This repository provides a focused contracts template for applications using Hexagonal Architecture. The template includes shared request/response contracts, DTOs, and gRPC contract definitions with unit tests to validate contract instantiation.

## Project Structure

- `src/`: Contains shared contracts.
  - `Contracts/`: Common base contracts, order contracts, and proto files.

- `tests/`: Contains tests for contract correctness.
  - `UnitTests/`: Instantiation tests for all contract records and DTOs.

## Helper Commands

### Restore dependencies

```bash
dotnet restore
```

### Build contracts project

```bash
dotnet build src/Contracts
```

### Run unit tests

```bash
dotnet test tests/UnitTests
```
