# Hexagonal architecture solution template

[![Publish template in Nuget.org](https://github.com/gpreviatti/hexagonal-solution-template/actions/workflows/publish.yml/badge.svg?branch=main)](https://github.com/gpreviatti/hexagonal-solution-template/actions/workflows/publish.yml)

[![Mutation testing badge](https://img.shields.io/endpoint?style=flat&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2Fgpreviatti%2Fhexagonal-solution-template%2Fmain)](https://dashboard.stryker-mutator.io/reports/github.com/gpreviatti/hexagonal-solution-template/main)

This is a dotnet solution template from projects based on hexagonal architecture and best practices

## Available templates

- `hexagonal-solution-full`: Full hexagonal architecture template (Domain, Application, Infrastructure, WebApp, tests and load tests).
- `hexagonal-solution-bff`: Backend-for-Frontend focused template with HTTP/gRPC integration patterns and integration/load tests.
- `hexagonal-solution-contracts`: Contracts-only template for shared request/response models, DTOs, gRPC protobuf definitions and unit tests.

## Used Technologies

- FluentValidation
- OpenTelemetry
- EntityFrameworkCore
- EntityFrameworkCore.SqlServer
- Xunit
- Moq
- AutoFixture
- gRPC / Protobuf
- Docker and Docker Compose
- K6
- Stryker
- GitHub Actions
- Hybrid cache
- Rabbit Mq

## Advisors and recommendations

- When use the template i recommend you dot not remove Order example scenario until you have you own implementations

## How to use the template

To install the project template you have to use the following command

```bash
dotnet new install GPreviatti.Template.Hexagonal.Solution
```

After that you can use it to create your project where -n is the name of your solution to create the full solution template you can use the following command

```bash
dotnet new hexagonal-solution-full -n HexagonalSolution
```

To create only the BFF template you can use the following command

```bash
dotnet new hexagonal-solution-bff -n HexagonalSolution
```

To create only the Contracts template you can use the following command

```bash
dotnet new hexagonal-solution-contracts -n HexagonalSolution
```

The Contracts template is recommended when you want a lightweight shared library for API and messaging contracts that can be reused across services.

If you had any doubts about the existing parameters you can also use -h to get more information

```bash
dotnet new hexagonal-solution-full -h
```

or 

```bash
dotnet new hexagonal-solution-bff -h
```

or

```bash
dotnet new hexagonal-solution-contracts -h
```

If you want to update the template to the latest version just execute the following command

```bash
dotnet new update GPreviatti.Template.Hexagonal.Solution
```

If you want to uninstall the template just execute the following command :(

```bash
dotnet new uninstall GPreviatti.Template.Hexagonal.Solution
```

## Official solution template documentation

<https://github.com/dotnet/templating/wiki>


## Contribute

Have a feature request or found a bug? We'd love to hear from you!

- [Report a Bug](https://github.com/gpreviatti/hexagonal-solution-template/issues/new?template=bug_report.md)
- [Request a Feature](https://github.com/gpreviatti/hexagonal-solution-template/issues/new?template=feature_request.md)
