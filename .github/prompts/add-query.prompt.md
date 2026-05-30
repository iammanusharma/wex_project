---
mode: agent
description: Scaffold a new MediatR CQRS Query with handler and unit tests.
---

# Add Query: {{QueryName}}

Create a new MediatR query for: **{{QueryDescription}}**

## Files to create

### 1. Query record
**Path**: `src/WEX.Application/Features/{{FeatureFolder}}/Queries/{{QueryName}}/{{QueryName}}Query.cs`
```csharp
public sealed record {{QueryName}}Query(...) : IRequest<{{ResponseType}}>;
```

### 2. Response DTO
**Path**: `src/WEX.Application/Features/{{FeatureFolder}}/Queries/{{QueryName}}/{{QueryName}}Response.cs`
```csharp
// Immutable record with all output properties
// No domain entities exposed directly
public sealed record {{QueryName}}Response(...);
```

### 3. Handler
**Path**: `src/WEX.Application/Features/{{FeatureFolder}}/Queries/{{QueryName}}/{{QueryName}}QueryHandler.cs`
```csharp
// IRequestHandler<{{QueryName}}Query, {{ResponseType}}>
// Constructor-inject only interfaces
// Include ILogger<{{QueryName}}QueryHandler>
// Accept CancellationToken, pass to all async calls
// Throw EntityNotFoundException if entity not found
// Map domain entity to response DTO (do not return raw entities)
```

### 4. Unit tests
**Path**: `tests/WEX.Application.Tests/Features/{{FeatureFolder}}/{{QueryName}}QueryHandlerTests.cs`

Tests required:
- `Handle_ExistingEntity_ReturnsMappedResponse`
- `Handle_EntityNotFound_ThrowsNotFoundException`
- Any additional business logic paths (e.g., currency not available)

Use NSubstitute for mocks, FluentAssertions for assertions.
