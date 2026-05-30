---
description: Create a new MediatR CQRS Query with handler, response DTO, and full unit tests for WEX Corporate Payments.
---

Ask the user for:
1. Query name (e.g. "GetPurchaseInCurrency")
2. Feature folder (e.g. "Transactions")
3. Input properties and their types
4. Response shape (what fields to return)

Then create these files in `src/WEX.Application/Features/{Feature}/Queries/{QueryName}/`:

**1. `{QueryName}Query.cs`**
```csharp
public sealed record {QueryName}Query(...) : IRequest<{QueryName}Response>;
```

**2. `{QueryName}Response.cs`**
- Immutable record with all output properties
- No domain entities exposed directly — always map to DTO

**3. `{QueryName}QueryValidator.cs`**
- `AbstractValidator<{QueryName}Query>` for input validation

**4. `{QueryName}QueryHandler.cs`**
- `IRequestHandler<{QueryName}Query, {QueryName}Response>`
- Constructor-inject only interfaces
- Inject `ILogger<{QueryName}QueryHandler>`
- Throw `{Entity}NotFoundException` if entity not found (never return null)
- Map domain entity → response DTO in the handler
- Accept `CancellationToken`, pass to all async calls

**5. Unit tests** in `tests/WEX.Application.Tests/Features/{Feature}/`
- `Handle_Existing{Entity}_ReturnsMappedResponse` — happy path
- `Handle_{Entity}NotFound_Throws{Entity}NotFoundException`
- One test per additional business logic branch
- NSubstitute + FluentAssertions, Arrange/Act/Assert structure

Refer to `.github/copilot-instructions.md` before writing any code.
