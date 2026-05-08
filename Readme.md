# Hexagonal architecture solution template

[![Publish template in Nuget.org](https://github.com/gpreviatti/hexagonal-solution-template/actions/workflows/publish.yml/badge.svg?branch=main)](https://github.com/gpreviatti/hexagonal-solution-template/actions/workflows/publish.yml)
[![Mutation testing badge](https://img.shields.io/endpoint?style=flat&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2Fgpreviatti%2Fhexagonal-solution-template%2Fmain)](https://dashboard.stryker-mutator.io/reports/github.com/gpreviatti/hexagonal-solution-template/main)

This repository provides a reusable .NET template package to bootstrap projects using **Hexagonal Architecture** (Ports and Adapters) and modern engineering practices.

It includes four templates for different scopes: a full application, a BFF-focused application, a Blazor Web UI application, and a contracts-only package.

## Table of contents

- [What this project provides](#what-this-project-provides)
- [Available templates](#available-templates)
- [Template documentation](#template-documentation)
- [Technologies included](#technologies-included)
- [Quick start](#quick-start)
- [Template options and help](#template-options-and-help)
- [Update or uninstall](#update-or-uninstall)
- [Recommendation](#recommendation)
- [Official template docs](#official-template-docs)
- [Contributing](#contributing)

## What this project provides

The package `GPreviatti.Template.Hexagonal.Solution` lets you quickly create solutions with:

- clear layering and separation of concerns;
- HTTP and gRPC integration patterns;
- observability and test-first tooling support;
- examples that help accelerate initial development.

## Available templates

- `hexagonal-solution-full`: complete hexagonal solution with `Domain`, `Application`, `Infrastructure`, `WebApp`, and test/load test projects.
- `hexagonal-solution-bff`: Backend-for-Frontend oriented solution with HTTP/gRPC adapters, integration tests, and load tests.
- `hexagonal-solution-webui`: Blazor Web App oriented solution with contracts/infrastructure separation and bUnit component unit tests.
- `hexagonal-solution-contracts`: lightweight contracts package for shared DTOs, request/response models, and protobuf definitions.

## Template documentation

Each template has its own README with architecture details, structure, and helper commands:

- Full template: [`templates/Full/Readme.md`](templates/Full/Readme.md)
- BFF template: [`templates/Bff/Readme.md`](templates/Bff/Readme.md)
- Web UI template: [`templates/WebUi/Readme.md`](templates/WebUi/Readme.md)
- Contracts template: [`templates/Contracts/Readme.md`](templates/Contracts/Readme.md)

## Technologies included

- FluentValidation
- OpenTelemetry
- Entity Framework Core
- gRPC / Protobuf
- xUnit, Moq, AutoFixture
- Docker and Docker Compose
- k6 (load testing)
- Stryker (mutation testing)
- GitHub Actions
- Hybrid cache
- RabbitMQ

## Quick start

Install the template package:

```bash
dotnet new install GPreviatti.Template.Hexagonal.Solution
```

Create a new solution from each template:

```bash
dotnet new hexagonal-solution-full -n HexagonalSolution
dotnet new hexagonal-solution-bff -n HexagonalSolution
dotnet new hexagonal-solution-webui -n HexagonalSolution
dotnet new hexagonal-solution-contracts -n HexagonalSolution
```

## Template options and help

Use `-h` to list available options for each template:

```bash
dotnet new hexagonal-solution-full -h
dotnet new hexagonal-solution-bff -h
dotnet new hexagonal-solution-webui -h
dotnet new hexagonal-solution-contracts -h
```

## Update or uninstall

Update to the latest published template version:

```bash
dotnet new update GPreviatti.Template.Hexagonal.Solution
```

Uninstall the template package:

```bash
dotnet new uninstall GPreviatti.Template.Hexagonal.Solution
```

## Recommendation

Keep the provided `Order` sample scenario in place until your own domain scenario is implemented. It serves as a reference for architecture, project organization, and tests.

## Official template docs

<https://github.com/dotnet/templating/wiki>

## Contributing

Have a feature request or found a bug? We'd love to hear from you!

- [Report a Bug](https://github.com/gpreviatti/hexagonal-solution-template/issues/new?template=bug_report.md)
- [Request a Feature](https://github.com/gpreviatti/hexagonal-solution-template/issues/new?template=feature_request.md)
