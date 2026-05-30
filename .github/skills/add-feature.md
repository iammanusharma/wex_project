---
description: Scaffold a complete new feature end-to-end in WEX Corporate Payments following Clean Architecture. Provide feature name and description.
---

Add a new feature to WEX.CorporatePayments following Clean Architecture layer order.

Ask the user for:
1. Feature name (e.g. "Transactions", "Reports")
2. A one-line description of what the feature does

Then scaffold in this exact order — ask me before each step:

**Step 1 — Domain Layer** (`src/WEX.Domain`)
- Entity with private setters and a `Create(...)` factory method
- Value objects if needed (immutable records)
- Repository interface (`I{Entity}Repository`) with async methods + CancellationToken
- Domain exception(s): `{Entity}NotFoundException`, business rule exceptions
- Zero external dependencies

**Step 2 — Application Layer** (`src/WEX.Application/Features/{Feature}`)
- Command or Query record implementing `IRequest<T>`
- FluentValidation `AbstractValidator<TCommand>` — format/range/required only
- Handler implementing `IRequestHandler<TCommand, T>` — inject interfaces only, never concrete types
- Response DTO as immutable record
- ILogger injection, structured logging with named properties
- CancellationToken accepted and passed to all async calls
- Unit tests: NSubstitute + FluentAssertions, one test per outcome

**Step 3 — Infrastructure Layer** (`src/WEX.Infrastructure`)
- Repository implementing domain interface
- External service client if needed (with Polly retry + IMemoryCache)
- EF Core DbSet + configuration
- Register in DependencyInjection.cs
- Integration tests with Testcontainers

**Step 4 — API Layer** (`src/WEX.API/Controllers/v1`)
- Controller action with correct HTTP verbs and status codes
- Map domain exceptions to Problem Details in GlobalExceptionHandlerMiddleware
- XML doc comments for Swagger
- Integration tests with WebApplicationFactory

Refer to `.github/copilot-instructions.md` for all coding standards before writing any code.
