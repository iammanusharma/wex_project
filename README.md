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

**Key design decisions:**
- **Clean Architecture** — strict dependency flow: Domain ← Application ← Infrastructure → API
- **CQRS with MediatR** — Commands (writes) and Queries (reads) are fully separated
- **FluentValidation pipeline** — validation runs as a MediatR behaviour before every handler
- **Options pattern** — all config is strongly-typed via `IOptions<T>`, never raw strings
- **IHttpClientFactory** — manages HTTP connection pooling for Treasury API calls (avoids socket exhaustion)
- **Polly retry** — exponential backoff (3 retries) on transient Treasury API failures
- **IMemoryCache** — 60-minute TTL caches exchange rates per currency+date pair
- `EnsureCreated()` used for local/dev schema creation. In production, replace with EF Core migrations for proper versioning and rollback support.

---

## 🚀 Quick Start (Docker — recommended)

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) running

### Run everything
```bash
git clone <repository-url>
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

## 🛡️ Quality Guardrails

This project includes VS Code Copilot skills to maintain quality as the team grows:

| Skill | Usage |
|---|---|
| `/implement-requirement` | Guided requirement implementation with explain → plan → approve → implement |
| `/add-feature` | Scaffold a new feature following Clean Architecture |
| `/add-command` | Add a new CQRS command with handler + validator + tests |
| `/add-query` | Add a new CQRS query with handler + validator + tests |
| `/code-quality-check` | Review code against project architecture rules |
| `/add-angular-feature` | Scaffold a new Angular feature following project patterns |

The project spec (`.github/copilot-instructions.md`) is auto-loaded by GitHub Copilot and enforces architecture rules, naming conventions, logging standards, and testing requirements.

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
