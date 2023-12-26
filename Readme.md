# Hexagonal architecture solution template

[![Publish template in Nuget.org](https://github.com/gpreviatti/hexagonal-solution-template/actions/workflows/publish.yml/badge.svg?branch=main)](https://github.com/gpreviatti/hexagonal-solution-template/actions/workflows/publish.yml)
[![Validate pull request](https://github.com/gpreviatti/hexagonal-solution-template/actions/workflows/validate.yml/badge.svg)](https://github.com/gpreviatti/hexagonal-solution-template/actions/workflows/validate.yml)

This is a dotnet solution template from projects based on hexagonal architecture and best practices

## Used Technologies

- MediatR
- FluenValidation
- Serilog
- EntityFrameworkCore
- EntityFrameworkCore.SqlServer
- Xunit
- Moq
- Testcontainers
- Autofixture
- FluentAssertions

## Advisors and recommendations

- When use the template i recommend you dot not remove Order example scenario until you have you own implementations

## How to use the template

To install the project template you have to use the following command

```
dotnet new install Hexagonal.Solution.Template
```

After that you can use it to create your project where -n is the name of your solution

```
dotnet new Hexagonal.Solution.Template -n HexagonalSolution
```

If you had any doubts about the existing parameters you can also use -h to get more information

```
dotnet new Hexagonal.Solution.Template -h
```
