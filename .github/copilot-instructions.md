# WEX Corporate Payments — Copilot Instructions

## Architecture Rules (NON-NEGOTIABLE)
- **Clean Architecture**: WEX.Domain has ZERO external dependencies
- **Dependency flow**: Domain ← Application ← Infrastructure → API (never reverse)
- **All use cases** are MediatR `IRequest<T>` + `IRequestHandler<T>` in the Application layer
- **No business logic** in Controllers, Infrastructure, or Program.cs
- **No direct DbContext** usage outside of WEX.Infrastructure

## C# Standards
- C# 12, .NET 8, nullable reference types **always enabled**
- `record` types for Commands, Queries, and DTOs
- `sealed` on concrete domain entities unless explicitly designed for inheritance
- `async/await` throughout — every async method must accept `CancellationToken`
- No `async void` — use `async Task`
- Use `ArgumentNullException.ThrowIfNull()` over manual null checks

## Naming Conventions
- Interfaces: `IFoo` (enforced by .editorconfig)
- Commands: `VerbNounCommand` (e.g., `StorePurchaseTransactionCommand`)
- Queries: `GetNounQuery` or `GetNounByXQuery`
- Handlers: `VerbNounCommandHandler` / `GetNounQueryHandler`
- Validators: `VerbNounCommandValidator`
- Repository interfaces in Domain: `IFooRepository`
- Services interfaces in Application: `IFooService`

## Error Handling
- Domain errors → throw typed domain exceptions (e.g., `TransactionNotFoundException`, `CurrencyConversionUnavailableException`)
- Never expose stack traces in API responses
- All errors return RFC 7807 Problem Details (`application/problem+json`)
- Validation errors → 400 with field-level detail array
- Not found → 404, business rule violations → 422

## Logging (Serilog)
- Use **structured logging** always: `Log.Information("Stored transaction {TransactionId}", id)`
- Never log sensitive data (amounts are OK, but no PII)
- Always include `CorrelationId` context (set by middleware)
- Log level guidelines:
  - `Debug`: internal state for diagnosing issues
  - `Information`: business events (transaction stored, query executed)
  - `Warning`: recoverable issues (rate lookup fallback, retry)
  - `Error`: unhandled exceptions

## Testing Standards
- **Unit tests**: xUnit + NSubstitute + FluentAssertions
- **Integration tests**: WebApplicationFactory + Testcontainers (PostgreSQL) + WireMock.Net
- Test naming: `MethodName_Scenario_ExpectedBehaviour`
- Structure: Arrange / Act / Assert with blank line separation
- Every Command handler and Query handler **must** have unit tests
- Every API endpoint **must** have integration tests covering happy path + error cases
- Test doubles: prefer NSubstitute `Substitute.For<T>()` over manual mocks

## Security
- Never log raw request bodies (may contain amounts/sensitive context)
- Validate all inputs with FluentValidation **before** reaching handlers
- No SQL string concatenation — EF Core parameterised queries only
- HTTP headers: always include security headers middleware (X-Content-Type, X-Frame-Options)
- Secrets only via environment variables or Secret Manager — never in appsettings.json

## Observability
- OpenTelemetry traces on all incoming HTTP and outgoing HTTP calls
- Health check endpoint at `/health` (liveness + readiness)
- Structured log output (JSON in non-local environments)

## Configuration Environments
| File | Purpose | Committed? |
|---|---|---|
| `appsettings.json` | Shared defaults | ✅ Yes |
| `appsettings.Development.json` | Dev overrides | ✅ Yes |
| `appsettings.UAT.json` | UAT overrides | ✅ Yes |
| `appsettings.Local.json` | Local secrets/overrides | ❌ No (.gitignored) |
| `appsettings.Production.json` | Prod secrets | ❌ No (.gitignored) |

When running locally, set `ASPNETCORE_ENVIRONMENT=Local`.

## When Adding a New Feature
1. Start in **Domain**: entity, value object, or interface
2. Move to **Application**: Command or Query + Validator + Handler
3. Move to **Infrastructure**: concrete implementations
4. Finish in **API**: controller action, wired via DI
5. Write tests at every layer before moving to the next
