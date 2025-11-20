# Hexagonal architecture solution template

[![Publish template in Nuget.org](https://github.com/gpreviatti/hexagonal-solution-template/actions/workflows/publish.yml/badge.svg?branch=main)](https://github.com/gpreviatti/hexagonal-solution-template/actions/workflows/publish.yml)
[![Validate pull request](https://github.com/gpreviatti/hexagonal-solution-template/actions/workflows/validate.yml/badge.svg)](https://github.com/gpreviatti/hexagonal-solution-template/actions/workflows/validate.yml)
[![Mutation testing template](https://img.shields.io/endpoint?style=flat&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2Fgpreviatti%2Fhexagonal-solution-template%2Fmain)](https://dashboard.stryker-mutator.io/reports/github.com/gpreviatti/hexagonal-solution-template/main)

This is a dotnet solution template from projects based on hexagonal architecture and best practices

## Used Technologies

- FluentValidation
- OpenTelemetry
- EntityFrameworkCore
- EntityFrameworkCore.SqlServer
- Xunit
- Moq
- AutoFixture
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
dotnet new install Hexagonal.Solution.Template
```

After that you can use it to create your project where -n is the name of your solution

```bash
dotnet new hexagonal-solution -n HexagonalSolution
```

If you had any doubts about the existing parameters you can also use -h to get more information

```bash
dotnet new hexagonal-solution -h
```

If you want to uninstall the template just execute the following command :(

```bash
dotnet new uninstall Hexagonal.Solution.Template
```

## Official solution template documentation

<https://github.com/dotnet/templating/wiki>
