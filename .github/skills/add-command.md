---
description: Create a new MediatR CQRS Command with FluentValidation validator, handler, and full unit tests for WEX Corporate Payments.
---

Ask the user for:
1. Command name (e.g. "StorePurchaseTransaction")
2. Feature folder (e.g. "Transactions")
3. Input properties and their types
4. Return type (Guid for new entity ID, or Unit for void)

Then create these files in `src/WEX.Application/Features/{Feature}/Commands/{CommandName}/`:

**1. `{CommandName}Command.cs`**
```csharp
public sealed record {CommandName}Command(...) : IRequest<{ReturnType}>;
```

**2. `{CommandName}CommandValidator.cs`**
- `AbstractValidator<{CommandName}Command>`
- Validate every property with clear `.WithMessage()` on each rule
- Format/range/required rules only — no business logic here

**3. `{CommandName}CommandHandler.cs`**
- `IRequestHandler<{CommandName}Command, {ReturnType}>`
- Constructor-inject only interfaces (never concrete types)
- Inject `ILogger<{CommandName}CommandHandler>`
- `LogInformation` at entry and on success
- Accept `CancellationToken`, pass to all async calls
- Throw typed domain exceptions — never return null as an error signal

**4. Unit tests** in `tests/WEX.Application.Tests/Features/{Feature}/`
- `Handle_Valid{CommandName}_Returns{ReturnType}` — happy path
- One test per validation failure path
- One test per domain exception path
- NSubstitute for mocks, FluentAssertions for assertions
- Arrange / Act / Assert with blank line separators

Refer to `.github/copilot-instructions.md` before writing any code.
