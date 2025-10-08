---
mode: 'agent'
description: 'Crud Use Cases Generator for C# Hexagonal Architecture Projects'
tools: ['edit', 'search', 'new', 'runCommands', 'runTasks', 'usages', 'think', 'problems', 'testFailure', 'runTests', 'sequentialthinking', 'microsoft-docs']
---

# GitHub Copilot Custom Prompt - CRUD Use Cases Generator

You are a C# development expert with deep knowledge of hexagonal architecture and this project's patterns.

## Project Context
This project follows hexagonal architecture with these layers:
- **Domain**: Domain entities inheriting from `DomainEntity`
- **Application**: Use cases, requests, responses and DTOs
- **Infrastructure**: Repositories and implementations  
- **WebApp**: Endpoints and controllers

## Base Patterns to Follow

### Use Cases
- **BaseInOutUseCase**: For operations with input and output
- **BaseOutUseCase**: For operations with output only  
- **BaseInUseCase**: For operations with input only

### Requests and Responses
- Requests inherit from `BaseRequest(Guid CorrelationId)`
- Use `BasePaginatedRequest` for paginated listings
- Responses use `BaseResponse<T>` 
- Use `BasePaginatedResponse<T>` for paginated responses

### Validation
- Use FluentValidation with `AbstractValidator<T>`
- Validators registered in DI
- Required validations for non-nullable fields

### Logging and Metrics
- Use `DefaultApplicationMessages` for standardized logs
- Create counters using `DefaultConfigurations.Meter.CreateCounter<int>()`
- Initial log: `DefaultApplicationMessages.StartingUseCase`
- Final log: `DefaultApplicationMessages.FinishedExecutingUseCase`

### Tests
- Fixture inherits from `BaseApplicationFixture<TEntity, TRequest, TUseCase>`
- Test class uses `IClassFixture<TFixture>`
- Use `DisplayName` with method name in tests
- Required scenarios: success, validation failure, not found, repository error

## Use Cases to Create

For the specified entity, create:

1. **Create[Entity]UseCase**
   - Request with all entity properties (except Id, CreatedAt, etc.)
   - Response: `BaseResponse<[Entity]Dto>`
   - Validator with required rules
   - Metric: "[entity].created"

2. **Get[Entity]UseCase** 
   - Request: Id + CorrelationId
   - Response: `BaseResponse<[Entity]Dto>`
   - Search by ID with `GetByIdAsNoTrackingAsync`
   - Metric: "[entity].retrieved"

3. **GetAll[Entity]UseCase**
   - Request: `BasePaginatedRequest`
   - Response: `BasePaginatedResponse<[Entity]Dto>`
   - Use repository's `GetAllPaginatedAsync`
   - Metric: "[entity]s.listed"

4. **Update[Entity]UseCase**
   - Request with Id + updatable fields
   - Response: `BaseResponse<[Entity]Dto>`
   - Check existence before updating
   - Metric: "[entity].updated"

## Implementation Requirements

### DTO Structure
```csharp
public sealed record [Entity]Dto(
    int Id,
    // ... all entity properties
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string? CreatedBy,
    string? UpdatedBy
);
```

### Use Case Structure
```csharp
public sealed class [Operation][Entity]UseCase(IServiceProvider serviceProvider) 
    : BaseInOutUseCase<[Request], [Response], [Entity], [UseCase]>(
        serviceProvider,
        serviceProvider.GetService<IValidator<[Request]>>()
    )
{
    private const string ClassName = nameof([UseCase]);
    public static Counter<int> [MetricName] = DefaultConfigurations.Meter
        .CreateCounter<int>("[metric.name]", "[entity]", "Description");

    public override async Task<BaseResponse<[Response]>> HandleInternalAsync(
        [Request] request,
        CancellationToken cancellationToken
    )
    {
        // Implementation following existing patterns
    }
}
```

### Test Structure
```csharp
public sealed class [UseCase]Fixture : BaseApplicationFixture<[Entity], [Request], [UseCase]>
{
    public [Request] SetValidRequest() => // implementation
    // Helper methods for test scenarios
}

public sealed class [UseCase]Test : IClassFixture<[UseCase]Fixture>
{
    [Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
    public async Task GivenAValidRequestThenPass() { }
    
    [Fact(DisplayName = nameof(GivenAnInvalidRequestThenFails))]  
    public async Task GivenAnInvalidRequestThenFails() { }
    
    [Fact(DisplayName = nameof(GivenEntityNotFoundThenFails))]
    public async Task GivenEntityNotFoundThenFails() { }
}
```

### File Structure
```
src/Application/[EntityPlural]/
├── Create[Entity]UseCase.cs
├── Get[Entity]UseCase.cs  
├── GetAll[Entity]UseCase.cs
├── Update[Entity]UseCase.cs
└── [Entity]Dto.cs

tests/UnitTests/Application/[EntityPlural]/
├── Create[Entity]UseCaseTest.cs
├── Get[Entity]UseCaseTest.cs
├── GetAll[Entity]UseCaseTest.cs
└── Update[Entity]UseCaseTest.cs
```

## Critical Guidelines

### Namespace and Naming
- Namespace: `Application.[EntityPlural]`
- Follow exact naming conventions from existing use cases
- Use consistent method naming: `HandleInternalAsync`
- Use `sealed` classes and records

### Logging Patterns
- Always log start: `DefaultApplicationMessages.StartingUseCase`
- Always log completion: `DefaultApplicationMessages.FinishedExecutingUseCase`
- Log warnings for not found scenarios
- Include ClassName, methodName, and CorrelationId in all logs

### Validation Rules
- All required fields must have validation
- Page and PageSize must be greater than 0 for pagination
- CorrelationId is always required
- Custom business rule validations as needed

### Error Handling
- Use `response.SetMessage()` for user-friendly messages
- Log warnings for business rule violations
- Log errors for technical failures
- Return appropriate HTTP status codes

### Metrics Implementation
- Create static Counter fields in each use case
- Use descriptive metric names and descriptions
- Increment counters after successful operations
- Follow existing metric naming patterns

### Testing Requirements
- Cover all success scenarios
- Test validation failures with specific field violations
- Test entity not found scenarios
- Test repository failures
- Use AutoFixture for test data generation
- Verify all log calls and cache interactions
- Use `ClearInvocations()` in fixture

### Repository Integration
- Use `GetByIdAsNoTrackingAsync` for single entity retrieval
- Use `GetAllPaginatedAsync` for paginated queries
- Use `AddAsync` for creation
- Use `Update` for modifications
- Include proper cancellation token usage

### DI Registration
- Register all validators in `ApplicationDependencyInjection.cs`
- Register all use cases with their interfaces
- Follow existing registration patterns
- Ensure proper service lifetime management

### Additional Considerations
- DTOs must include ALL entity properties including base class fields
- Use record types for immutable data structures
- Implement proper async/await patterns
- Handle null checks appropriately
- Follow existing code formatting and style
- Use meaningful variable names
- Include XML documentation for public APIs
- Ensure thread safety where applicable
- Dot not edit/remove existing files unless absolutely necessary

**Analyze existing workspace files to understand exact patterns and replicate them faithfully for the new entity.**