---
mode: agent
description: >
  Scaffold a complete new feature end-to-end following Clean Architecture.
  Provide the feature name and a one-line description.
---

# Add New Feature: {{featureName}}

**Description**: {{featureDescription}}

You are adding a new feature to the WEX Corporate Payments application.
Follow the mandatory layer order below. Do NOT skip layers or combine steps.

---

## Step 1 — Domain Layer (`src/WEX.Domain`)

If the feature involves a new entity or value object, create it here:
- Entity: `Entities/{{EntityName}}.cs` — sealed class, private setters, factory method `Create(...)`
- Value Object: `ValueObjects/{{ValueObjectName}}.cs` — record, immutable, include validation
- Repository interface: `Interfaces/I{{EntityName}}Repository.cs` with async methods + CancellationToken
- Domain exception(s): `Exceptions/{{EntityName}}NotFoundException.cs` if applicable
- Zero external dependencies — no EF Core, no HTTP, no NuGet beyond the framework

## Step 2 — Application Layer (`src/WEX.Application/Features/{{FeatureName}}`)

Create the use case:
- Command OR Query record: `{{VerbNoun}}Command.cs` or `{{VerbNoun}}Query.cs`
- FluentValidation validator: `{{VerbNoun}}CommandValidator.cs`
- Handler: `{{VerbNoun}}CommandHandler.cs` — inject only interfaces, never concrete types
- Response DTO if needed: `{{VerbNoun}}Response.cs`
- Register in `DependencyInjection.cs` if new service interface added

Handler rules:
- Accept CancellationToken and pass it to all async calls
- Log with structured properties: `_logger.LogInformation("...", {{correlatedProperty}})`
- Throw typed domain exceptions (never return nulls as error signals)

## Step 3 — Infrastructure Layer (`src/WEX.Infrastructure`)

Implement any interfaces declared in Domain or Application:
- Repository: `Repositories/{{EntityName}}Repository.cs`
- External service: `ExternalServices/{{ServiceName}}/{{ServiceName}}Client.cs`
- Add EF Core DbSet and configuration in `Persistence/Configurations/`
- Add Polly resilience if calling external HTTP APIs
- Register in `DependencyInjection.cs`

## Step 4 — API Layer (`src/WEX.API/Controllers/v1`)

Add the endpoint:
- Add action to existing controller OR create `{{EntityName}}sController.cs`
- Return correct HTTP status codes (201+Location for create, 200 for get, 422 for business errors)
- Map domain exceptions to Problem Details in `GlobalExceptionHandlerMiddleware`
- Document with XML comments for Swagger

## Step 5 — Tests (write BEFORE moving to next layer)

**Unit tests** (`tests/WEX.Application.Tests/Features/{{FeatureName}}/`):
- `{{VerbNoun}}CommandHandlerTests.cs` — test all outcomes with NSubstitute + FluentAssertions
- Cover: happy path, each validation failure, each domain exception path

**Integration tests** (`tests/WEX.API.IntegrationTests/Controllers/`):
- `{{EntityName}}sControllerTests.cs` — use WebApplicationFactory + Testcontainers
- Cover: valid request → correct response, invalid input → 400, not found → 404

---

Remind yourself of the rules in `.github/copilot-instructions.md` before writing any code.
