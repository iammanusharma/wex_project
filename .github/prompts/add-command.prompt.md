---
mode: agent
description: Scaffold a new MediatR CQRS Command with validator, handler, and unit tests.
---

# Add Command: {{CommandName}}

Create a new MediatR command for: **{{CommandDescription}}**

## Files to create

### 1. Command record
**Path**: `src/WEX.Application/Features/{{FeatureFolder}}/Commands/{{CommandName}}/{{CommandName}}Command.cs`
```csharp
// Record with all required input properties
// Return type: {{ReturnType}} (use Guid for new entity ID, or Unit for void)
public sealed record {{CommandName}}Command(...) : IRequest<{{ReturnType}}>;
```

### 2. Validator
**Path**: `src/WEX.Application/Features/{{FeatureFolder}}/Commands/{{CommandName}}/{{CommandName}}CommandValidator.cs`
```csharp
// FluentValidation AbstractValidator<{{CommandName}}Command>
// Validate every property with clear error messages
// No business logic here — only format/range/required rules
```

### 3. Handler
**Path**: `src/WEX.Application/Features/{{FeatureFolder}}/Commands/{{CommandName}}/{{CommandName}}CommandHandler.cs`
```csharp
// IRequestHandler<{{CommandName}}Command, {{ReturnType}}>
// Constructor-inject only interfaces (never concrete types)
// Include ILogger<{{CommandName}}CommandHandler>
// Accept CancellationToken, pass to all async calls
// Log: LogInformation at start and on success, LogWarning on expected failures
// Throw typed domain exceptions (never return null as an error signal)
```

### 4. Unit tests
**Path**: `tests/WEX.Application.Tests/Features/{{FeatureFolder}}/{{CommandName}}CommandHandlerTests.cs`

Create tests covering:
- `Handle_ValidCommand_Returns{{ReturnType}}` — happy path
- `Handle_InvalidInput_ThrowsValidationException` — if validation inline
- `Handle_EntityNotFound_ThrowsDomainException` — if applicable
- One test per distinct failure path

Use NSubstitute for mocks, FluentAssertions for assertions.
Arrange/Act/Assert with blank lines between sections.
