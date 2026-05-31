# WEX Corporate Payments API

A technical assessment implementation for WEX — an enterprise-grade corporate payments service built with **.NET 8 Clean Architecture** and **Angular 19**.

---

## 📋 Requirements Implemented

### Requirement 1 — Store a Purchase Transaction
`POST /api/v1/transactions`

Stores a purchase transaction with:
- Description (max 50 characters)
- Transaction date
- Purchase amount in USD (positive, rounded to nearest cent)

### Requirement 2 — Retrieve Transaction in Foreign Currency
`GET /api/v1/transactions/{id}?currency={code}`

Retrieves a stored transaction and converts the USD amount to the target currency using the [US Treasury Reporting Rates of Exchange API](https://fiscaldata.treasury.gov/datasets/treasury-reporting-rates-exchange/treasury-reporting-rates-of-exchange). Returns the exchange rate active at the time of purchase (within 6 months), or `422 Unprocessable Entity` if no rate is available.

---

## 🏗️ Architecture

```
src/
├── WEX.Domain/          # Entities, Value Objects, Domain Exceptions, Interfaces
├── WEX.Application/     # CQRS Commands/Queries (MediatR), Validators (FluentValidation)
├── WEX.Infrastructure/  # EF Core (PostgreSQL), Treasury API client (Polly + IMemoryCache)
└── WEX.API/             # ASP.NET Core 8 REST API, Serilog, Swagger

src/WEX.UI/              # Angular 19 standalone app with Angular Material
```

## 🏗️ Architecture

```
src/
├── WEX.Domain/          # Entities, Value Objects, Domain Exceptions, Interfaces
├── WEX.Application/     # CQRS Commands/Queries (MediatR), Validators (FluentValidation)
├── WEX.Infrastructure/  # EF Core (PostgreSQL), Treasury API client (Polly + IMemoryCache)
└── WEX.API/             # ASP.NET Core 8 REST API, Serilog, Swagger

src/WEX.UI/              # Angular 19 standalone app with Angular Material
```

**Key design decisions:**
- **Clean Architecture** — strict dependency flow: Domain ← Application ← Infrastructure → API. Each layer has a single responsibility and can evolve independently. New developers onboard quickly because the structure is predictable.
- **CQRS with MediatR** — Commands (writes) and Queries (reads) are fully separated. This scales well as the team grows — developers work on features in parallel without stepping on each other.
- **FluentValidation pipeline** — validation runs as a MediatR behaviour before every handler. Validation is never forgotten and never duplicated.
- **Options pattern** — all config is strongly-typed via `IOptions<T>`, never raw strings. Eliminates runtime config errors and makes environment differences explicit.
- **IHttpClientFactory** — manages HTTP connection pooling for Treasury API calls (avoids socket exhaustion at scale).
- **Polly retry** — exponential backoff (3 retries) on transient Treasury API failures. Resilience is built in, not bolted on.
- **IMemoryCache** — 60-minute TTL caches exchange rates per currency+date pair. Reduces external API dependency and latency.
- **Quality guardrail skills** — VS Code Copilot skills enforce architecture patterns so the codebase stays consistent as the team scales. New developers follow the same patterns from day one without needing a lengthy onboarding session.
- `EnsureCreated()` is used for local/dev schema creation — no migration files required, so the interviewer can run `docker-compose up` and it just works. In production this would be replaced with EF Core migrations (`dotnet ef migrations add` / `dotnet ef database update`) for proper schema versioning, incremental changes, and rollback support. This was a conscious trade-off: optimise for reviewer simplicity while keeping the production path clear.

---

## 🚀 Quick Start

### One-command start (auto-detects Docker)

```bash
# Clone the repo first
git clone <repository-url>
cd wex_project

# Windows
.\start.ps1

# Mac / Linux
chmod +x start.sh && ./start.sh
```

The script detects whether Docker is available:
- **Docker found** → runs `docker-compose up --build` (zero config needed)
- **No Docker** → prints step-by-step local setup instructions

---

### Docker (recommended)

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) running

### Run everything
```bash
cd wex_project
docker-compose up
```

| Service | URL |
|---|---|
| Angular UI | http://localhost:4200 |
| .NET API | http://localhost:5000 |
| Swagger UI | http://localhost:5000/swagger |
| Health check | http://localhost:5000/health |
| PostgreSQL | localhost:5432 |

> The API waits for PostgreSQL to be healthy before starting, then auto-creates the schema on first boot.

---

## 💻 Local Development (without Docker)

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/)
- [PostgreSQL 16](https://www.postgresql.org/) running locally

### 1. Configure local database
Create `src/WEX.API/appsettings.Local.json` (git-ignored — never committed):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=wex_corporate_local;Username=<your_user>;Password=<your_password>"
  }
}
```

### 2. Run the API
```bash
cd src/WEX.API
ASPNETCORE_ENVIRONMENT=Local dotnet run
# Windows:
$env:ASPNETCORE_ENVIRONMENT="Local"; dotnet run
```

### 3. Run the Angular UI
```bash
cd src/WEX.UI
npm install
npm run start:local      # http://localhost:4200
```

---

## 🧪 Testing

### Unit Tests (no dependencies required)
```bash
dotnet test tests/WEX.Domain.Tests
dotnet test tests/WEX.Application.Tests
```

### Functional API Tests (requires running API + database)
```bash
# Start the API first, then:
dotnet test tests/WEX.API.FunctionalTests
```

### E2E Browser Tests — Playwright (requires running API + UI)
```bash
# Start both API and UI first, then:
cd tests/e2e
npm install
npx playwright install chromium
npm test                  # headless
npm run test:headed       # with visible browser
npm run test:ui           # interactive Playwright UI
```

### Run all unit tests at once
```bash
dotnet test WEX.CorporatePayments.slnx
```

---

## 📡 API Reference

### POST /api/v1/transactions
Store a new purchase transaction.

**Request:**
```json
{
  "description": "Office supplies",
  "transactionDate": "2024-06-15",
  "amountUsd": 49.99
}
```

**Response `201 Created`:**
```json
{ "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6" }
```

**Validation errors `400 Bad Request`:**
- Description: required, max 50 characters
- Amount: must be positive, max 2 decimal places

---

### GET /api/v1/transactions/{id}?currency={code}
Retrieve a transaction converted to a foreign currency.

**Example:** `GET /api/v1/transactions/3fa85f64...?currency=EUR`

**Response `200 OK`:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "description": "Office supplies",
  "transactionDate": "2024-06-15",
  "originalAmountUsd": 49.99,
  "targetCurrency": "EUR",
  "exchangeRate": 1.08,
  "convertedAmount": 53.99
}
```

**Error responses:**
| Code | Reason |
|---|---|
| `400` | Invalid transaction ID or currency code |
| `404` | Transaction not found |
| `422` | No exchange rate available within 6 months of purchase date |

---

## 🔧 Environment Configuration

| File | Purpose | Committed |
|---|---|---|
| `appsettings.json` | Shared defaults | ✅ |
| `appsettings.Development.json` | Docker / CI | ✅ |
| `appsettings.UAT.json` | UAT environment | ✅ |
| `appsettings.Local.json` | Developer local overrides | ❌ (git-ignored) |
| `appsettings.Production.json` | Production secrets | ❌ (git-ignored) |

---

## 🛡️ Quality Guardrails & Copilot Skills

This project ships with a set of **GitHub Copilot Skills** — reusable AI prompt templates that guide every developer to follow the same architecture, naming, and quality rules automatically. They remove the need to memorise conventions and make it safe to onboard new developers quickly.

---

### How Skills Work

Skills live in `.github/skills/` and are companion prompt files in `.github/prompts/`. When you open GitHub Copilot Chat in VS Code and type a skill command (e.g. `@workspace /implement-requirement`), Copilot loads the skill's instructions and guides you through the task step-by-step — always enforcing the project's quality rules.

**Prerequisites:**
- VS Code with GitHub Copilot extension installed
- `.vscode/settings.json` is committed (wires skills automatically for the whole team)
- Open the `wex_project/` folder as your workspace root

---

### Available Skills

#### `/implement-requirement`
**When to use:** Starting a new feature from a business requirement or user story.

**What it does:**
1. Asks you to paste the requirement text
2. Analyses it — identifies inputs, outputs, validation rules, edge cases
3. Produces a full implementation plan across all Clean Architecture layers
4. Asks for your approval before writing any code
5. Implements layer-by-layer (Domain → Application → Infrastructure → API → Tests)
6. Produces a compliance report confirming every quality gate is met

**Benefits:** Ensures no layer is skipped, all error scenarios are considered, and tests are written alongside the feature — not as an afterthought.

---

#### `/add-feature`
**When to use:** Scaffolding a brand new feature module (new entity, new domain concept).

**What it does:** Creates the full vertical slice — entity, repository interface, domain exceptions, command/query, validator, handler, infrastructure implementation, controller action, and unit tests — all in one guided flow.

**Benefits:** Eliminates the "where do I put this?" question for new developers. Every scaffold follows the same structure.

---

#### `/add-command`
**When to use:** Adding a write operation (create, update, delete) to an existing feature.

**What it does:** Scaffolds:
- `{Name}Command.cs` — MediatR `IRequest<T>` record
- `{Name}CommandValidator.cs` — FluentValidation with `.WithMessage()` on every rule
- `{Name}CommandHandler.cs` — handler with structured logging, `CancellationToken`, typed exceptions
- Unit tests — happy path + all failure paths, NSubstitute + FluentAssertions

**Benefits:** Consistent CQRS structure across every command. No forgotten validators, no missing tests.

---

#### `/add-query`
**When to use:** Adding a read operation (get by ID, search, list) to an existing feature.

**What it does:** Scaffolds:
- `{Name}Query.cs` + `{Name}Response.cs` — query record and immutable DTO
- `{Name}QueryValidator.cs` — input validation
- `{Name}QueryHandler.cs` — handler mapping domain entity → response DTO, never exposing internals
- Unit tests — found/not-found/business-rule scenarios

**Benefits:** Enforces the rule that domain entities are never leaked to API callers — always mapped to a response DTO.

---

#### `/code-quality-check`
**When to use:** Before every pull request, or after adding/changing any feature.

**What it does:** Reviews changed files against 8 quality gates:
| Gate | Checks |
|---|---|
| Architecture | Dependency direction, no DbContext outside Infrastructure |
| SOLID | Single responsibility, open/closed, dependency inversion |
| Null safety | Nullable types enabled, no `!` operators without comment |
| Async | CancellationToken everywhere, no `.Result`/`.Wait()` |
| Logging | Structured logs, no sensitive data, correct log levels |
| Security | No raw SQL, no secrets in code, FluentValidation on all inputs |
| Error handling | Typed exceptions, RFC 7807 Problem Details, no silent catches |
| Test coverage | Every handler tested, naming convention, no logic in tests |

Produces: **PASS / FAIL** verdict with file:line violations and suggested fixes.

**Benefits:** Acts as a senior code reviewer available to every developer at any time. Catches issues before they reach human review.

---

#### `/add-angular-feature`
**When to use:** Adding a new page or component to the Angular UI.

**What it does:** Scaffolds a lazy-loaded Angular feature module with component, service, routing, and typed API integration — following the project's Angular structure.

**Benefits:** Consistent Angular patterns across the UI codebase.

---

### Project Spec (Auto-loaded)

`.github/copilot-instructions.md` is automatically loaded by GitHub Copilot for every conversation in this repository. It enforces:
- Clean Architecture dependency rules
- C# 12 / .NET 8 coding standards
- Naming conventions (Commands, Queries, Handlers, Validators)
- Logging standards (structured, no PII, correct levels)
- Testing standards (xUnit + NSubstitute + FluentAssertions)
- Security rules (no raw SQL, secrets via env vars only)
- Environment configuration table

> **For Engineering Managers:** These skills are the primary mechanism for maintaining consistent quality as the team scales. New developers are productive from day one because the skills encode the architecture decisions. The spec file ensures Copilot never suggests patterns that violate your standards.

---

### Recommended Workflow for New Developers

```
1. Clone repo → open wex_project/ in VS Code
2. Install GitHub Copilot extension
3. Start a new feature? → use /implement-requirement or /add-feature
4. Adding a command? → /add-command
5. Adding a query?   → /add-query
6. Before raising PR → /code-quality-check
```

---

## 📦 Tech Stack

| Layer | Technology |
|---|---|
| API | ASP.NET Core 8, MediatR 12, FluentValidation, Serilog |
| Data | Entity Framework Core 8, PostgreSQL 16 (Npgsql) |
| Resilience | Polly (retry + circuit breaker), IMemoryCache |
| Observability | Serilog structured logging, OpenTelemetry, Health Checks |
| UI | Angular 19, Angular Material, TypeScript |
| Testing | xUnit, NSubstitute, FluentAssertions, Playwright |
| Infrastructure | Docker, GitHub Actions CI |
