https://github.com/HackToHire-BNA01/Manu_Sharma_UC12.1_repo

# WEX Corporate Payments API

A technical assessment implementation for WEX — an enterprise-grade corporate payments service built with **.NET 8 Clean Architecture** and **Angular 19**.

---

## 📑 Table of Contents

1. [🚀 Quick Start](#-quick-start)
2. [💻 Local Development](#-local-development-without-docker)
3. [🔐 Authentication](#-authentication)
4. [📋 Requirements Implemented](#-requirements-implemented)
5. [🏗️ Architecture](#️-architecture)
6. [📡 API Reference](#-api-reference)
7. [🖥️ UI Pages](#️-ui-pages)
8. [🧪 Testing](#-testing)
9. [🔧 Environment Configuration](#-environment-configuration)
10. [🛡️ Quality Guardrails & Copilot Skills](#️-quality-guardrails--copilot-skills)
11. [📦 Tech Stack](#-tech-stack)

---
## 🚀 Quick Start

### Option A — One-command start (auto-detects Docker)

```powershell
# Windows
.\start.ps1

# Mac / Linux
chmod +x start.sh && ./start.sh
```

The script checks whether Docker is available:
- **Docker found** → runs `docker-compose up --build` (zero additional config)
- **No Docker** → prints manual setup instructions

### Option B — Docker Compose (recommended)

**Prerequisites:** [Docker Desktop](https://www.docker.com/products/docker-desktop/) running

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

> The API waits for PostgreSQL to become healthy, then auto-creates the schema on first boot. No manual DB setup required.

---

---

## 💻 Local Development (without Docker)

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/)
- [PostgreSQL 16](https://www.postgresql.org/) running locally

### 1. Create local config file
Create `src/WEX.API/appsettings.Local.json` (git-ignored — never committed):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=wex_corporate_local;Username=<your_user>;Password=<your_password>"
  },
  "Cors": {
    "AllowedOrigins": [ "http://localhost:4200" ]
  },
  "Jwt": {
    "Secret": "WexLocal-SuperSecret-Key-32chars!!",
    "Issuer": "wex-api",
    "Audience": "wex-client",
    "ExpiryMinutes": 60
  },
  "Auth": {
    "Users": {
      "<email-from-email>": "<password-from-email>"
    }
  }
}
```

### 2. Start the API
```powershell
# From wex_project root
dotnet run --project src/WEX.API --launch-profile Local
# Listening on http://localhost:5000
```

### 3. Start the Angular UI
```bash
cd src/WEX.UI
npm install
npx ng serve --configuration local
# Listening on http://localhost:4200
```

---

---

## 🔐 Authentication

All API endpoints (except `/health`) are protected by **JWT Bearer authentication**.

### Test Credentials

Test credentials have been shared separately via email.
Contact the author if you have not received them.

### How to Authenticate

**Option 1 — Angular UI (simplest)**
1. Open `http://localhost:4200`
2. You are redirected to the Login page automatically
3. Enter the credentials above and click **Sign In**
4. The UI handles all token management transparently

**Option 2 — Swagger UI**
1. Open `http://localhost:5000/swagger`
2. Expand `POST /api/v1/auth/login` → click **Try it out**
3. Enter the credentials and click **Execute**
4. Copy the `accessToken` value from the response body
5. Click the 🔒 **Authorize** button at the top of the page
6. Paste the token and click **Authorize**
7. All subsequent Swagger requests will include the Bearer token

**Option 3 — cURL / Postman**
```bash
# Step 1 — obtain a token
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"<email>","password":"<password>"}'

# Response: { "accessToken": "eyJ...", "expiresIn": 3600 }

# Step 2 — use the token
curl -X GET "http://localhost:5000/api/v1/transactions/{id}?currency=EUR" \
  -H "Authorization: Bearer eyJ..."
```

**Token details:** Tokens are valid for **60 minutes** (HS256 signed). The login endpoint returns the same `401` message for both wrong username and wrong password — this prevents user enumeration.

> **Production note:** The `InMemoryUserStore` and plaintext config passwords are intentional simplifications for this assessment. Production would use ASP.NET Core Identity with bcrypt hashing and optionally an OAuth 2.0 provider (Azure AD, Keycloak, etc.). The interfaces (`ITokenService`, `IUserStore`) are already in place so the swap is an Infrastructure-only change.

---

---

## 📋 Requirements Implemented

### Requirement 1 — Store a Purchase Transaction
`POST /api/v1/transactions` *(requires Bearer token)*

Stores a purchase transaction with:
- Description (max 50 characters)
- Transaction date
- Purchase amount in USD (positive, rounded to nearest cent)

### Requirement 2 — Retrieve Transaction in Foreign Currency
`GET /api/v1/transactions/{id}?currency={code}` *(requires Bearer token)*

Retrieves a stored transaction and converts the USD amount to the target currency using the [US Treasury Reporting Rates of Exchange API](https://fiscaldata.treasury.gov/datasets/treasury-reporting-rates-exchange/treasury-reporting-rates-of-exchange). Returns the exchange rate active within 6 months of the purchase date, or `422 Unprocessable Entity` if no rate is available.

---

---

## 🏗️ Architecture

```
src/
├── WEX.Domain/          # Entities, domain exceptions, repository interfaces
├── WEX.Application/     # CQRS handlers (MediatR), validators (FluentValidation), service interfaces
├── WEX.Infrastructure/  # EF Core + PostgreSQL, Treasury API client, JWT token service
└── WEX.API/             # ASP.NET Core 8, Swagger, Serilog, global exception handler

src/WEX.UI/              # Angular 19 standalone SPA (Angular Material)

tests/
├── WEX.Domain.Tests/           # Domain entity + value object unit tests
├── WEX.Application.Tests/      # Command/query handler + validator unit tests
├── WEX.Infrastructure.Tests/   # Repository + external service unit tests
└── WEX.API.IntegrationTests/   # Full HTTP integration tests (in-memory DB)
```

**Key design decisions:**

| Decision | Rationale |
|---|---|
| **Clean Architecture** | Strict dependency inversion: Domain ← Application ← Infrastructure → API. Each layer evolves independently; new developers know exactly where code belongs. |
| **CQRS with MediatR** | Commands (writes) and Queries (reads) are fully separated. Teams can work on them in parallel without conflicts. |
| **FluentValidation pipeline** | Validation runs as a MediatR behaviour before every handler — never forgotten, never duplicated. |
| **JWT Bearer auth** | `ITokenService` + `IUserStore` interfaces live in Application; HS256 implementation is in Infrastructure. Swapping to OAuth2/OIDC is an Infrastructure-only change. |
| **Options pattern** | All configuration is strongly-typed via `IOptions<T>`. No raw string lookups, environment differences are explicit. |
| **IHttpClientFactory + Polly** | Manages HTTP connection pooling for Treasury API; exponential-backoff retry (3 attempts) on transient failures. |
| **IMemoryCache** | 60-minute TTL per currency+date pair. Reduces external API dependency and latency. |
| **EnsureCreated()** | Zero-friction local/Docker startup — schema auto-created, no migration step. Production would use `dotnet ef database update` for versioned, rollback-safe migrations. Conscious trade-off for reviewer experience. |

---

---

## �� API Reference

> ⚠️ All endpoints except `POST /api/v1/auth/login` require `Authorization: Bearer <token>`.

### POST /api/v1/auth/login
Authenticate and receive a JWT access token.

**Request body:**
```json
{ "username": "<email>", "password": "<password>" }
```

**Response `200 OK`:**
```json
{ "accessToken": "eyJhbGci...", "expiresIn": 3600 }
```

| Code | Reason |
|---|---|
| `400` | Missing username or password |
| `401` | Invalid credentials |

---

### POST /api/v1/transactions
Store a new purchase transaction.

**Request body:**
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

| Code | Reason |
|---|---|
| `400` | Validation failed — description > 50 chars, amount ≤ 0, or > 2 decimal places |
| `401` | Missing or expired Bearer token |

---

### GET /api/v1/transactions/{id}?currency={code}
Retrieve a transaction with USD amount converted to the target currency.

**Example:** `GET /api/v1/transactions/3fa85f64-5717-4562-b3fc-2c963f66afa6?currency=EUR`

**Supported currencies (sample):** `EUR`, `GBP`, `JPY`, `CAD`, `AUD`, `CHF`, `CNY`, `INR`, `MXN`, `NZD` — any country/currency supported by the US Treasury Rates of Exchange dataset.

**Response `200 OK`:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "description": "Office supplies",
  "transactionDate": "2024-06-15",
  "originalAmountUsd": 49.99,
  "targetCurrency": "EUR",
  "exchangeRate": 0.93,
  "convertedAmount": 46.49
}
```

| Code | Reason |
|---|---|
| `400` | Invalid UUID or currency code format |
| `401` | Missing or expired Bearer token |
| `404` | Transaction ID not found |
| `422` | No Treasury exchange rate available within 6 months of the purchase date |
| `503` | US Treasury API temporarily unavailable (retried 3× before failing) |

---

---

## 🖥️ UI Pages

| Page | Route | Description |
|---|---|---|
| Login | `/login` | Sign in — redirected here automatically when not authenticated |
| New Transaction | `/transactions` | Create a new purchase transaction |
| Transaction Detail (lookup) | `/transactions/lookup` | Enter any transaction UUID + currency to retrieve and convert |
| Transaction Detail (linked) | `/transactions/:id` | Auto-navigated to after creating a transaction |

The **Transaction Detail** page works in two modes:
- **Linked mode** (from New Transaction) — ID pre-filled from the URL, only the currency field is editable
- **Lookup mode** (from nav menu) — both Transaction ID and Target Currency are editable inputs

A clear error message is shown when the ID does not exist.

---

---

## 🧪 Testing

### Run all tests (recommended)
```bash
dotnet test WEX.CorporatePayments.slnx
```

### Run individual test projects
```bash
dotnet test tests/WEX.Domain.Tests              # 12 tests  — domain entities & value objects
dotnet test tests/WEX.Application.Tests         # 38 tests  — handlers, validators, incl. auth
dotnet test tests/WEX.API.IntegrationTests      #  6 tests  — full HTTP: auth + protected endpoints
```

**Test summary: 56 tests, 0 failures**

Integration tests use an in-memory database and inject test JWT config via `appsettings.Testing.json` — no running database or external services required.

---

---

## 🔧 Environment Configuration

| File | Environment | Committed |
|---|---|---|
| `appsettings.json` | Shared base defaults | ✅ |
| `appsettings.Development.json` | Docker Compose / CI | ✅ |
| `appsettings.UAT.json` | UAT — uses `#{token}#` placeholders for secrets | ✅ |
| `appsettings.Testing.json` | Integration test in-memory config | ✅ |
| `appsettings.Local.json` | Developer machine overrides | ❌ git-ignored |
| `appsettings.Production.json` | Production secrets | ❌ git-ignored |

JWT secrets and database passwords are **never committed**. They are injected via environment-specific files locally, and via CI/CD secrets in deployment pipelines.

---

---

## 🛡️ Quality Guardrails & Copilot Skills

This project ships with **GitHub Copilot Skills** — reusable AI prompt templates that encode architecture rules, naming conventions, and quality gates so every developer follows the same patterns automatically.

### How Skills Work

Skills live in `.github/skills/` with companion prompt files in `.github/prompts/`. In VS Code Copilot Chat, type the skill command (e.g. `/implement-requirement`) and Copilot guides you step-by-step, enforcing all project standards.

**Prerequisites:**
- VS Code + GitHub Copilot extension
- Open `wex_project/` as your workspace root (`.vscode/settings.json` wires skills automatically)

---

### Available Skills

#### `/implement-requirement`
**Use when:** Starting a feature from a business requirement or user story.

Guides you through: requirement analysis → implementation plan (with approval gate) → layer-by-layer implementation (Domain → Application → Infrastructure → API → Tests) → compliance report.

---

#### `/add-feature`
**Use when:** Adding a brand new domain concept (new entity, new bounded context).

Creates the full vertical slice: entity, repository interface, domain exceptions, command/query/handler/validator, infrastructure, controller action, and tests.

---

#### `/add-command`
**Use when:** Adding a write operation (create, update, delete) to an existing feature.

Scaffolds: `Command`, `CommandValidator`, `CommandHandler`, and matching unit tests (happy path + all failure paths).

---

#### `/add-query`
**Use when:** Adding a read operation (get, search, list) to an existing feature.

Scaffolds: `Query`, `Response` DTO, `QueryValidator`, `QueryHandler` (with entity→DTO mapping), and unit tests.

---

#### `/code-quality-check`
**Use when:** Before raising a pull request.

Reviews changed files against 8 quality gates:

| Gate | What it checks |
|---|---|
| Architecture | Dependency direction, no DbContext outside Infrastructure |
| SOLID | Single responsibility, open/closed, dependency inversion |
| Null safety | Nullable enabled, no unexplained `!` operators |
| Async | CancellationToken everywhere, no `.Result` / `.Wait()` |
| Logging | Structured logs, no PII, correct severity levels |
| Security | No raw SQL, no secrets in code, all inputs validated |
| Error handling | Typed exceptions, RFC 7807 Problem Details, no silent catches |
| Test coverage | Every handler tested, AAA naming, no logic in test assertions |

Outputs a **PASS / FAIL** verdict with file:line citations and suggested fixes.

---

#### `/add-angular-feature`
**Use when:** Adding a new page or component to the Angular UI.

Scaffolds a lazy-loaded feature with component, service, typed API integration, route, and guard — following the project's existing Angular structure.

---

### Auto-loaded Project Spec

`.github/copilot-instructions.md` is loaded automatically by Copilot for every conversation in this repo. It encodes all architecture, naming, logging, testing, and security rules — so Copilot never suggests patterns that violate project standards.

> **For Engineering Managers:** Skills are the primary mechanism for maintaining quality at scale. New developers are productive from day one without lengthy onboarding. The spec file ensures AI suggestions stay consistent with the team's agreed patterns.

### Recommended Developer Workflow

```
1. Clone repo → open wex_project/ in VS Code
2. Install GitHub Copilot extension
3. New feature from a story?      → /implement-requirement
4. New domain concept (entity)?   → /add-feature
5. Adding a write operation?      → /add-command
6. Adding a read operation?       → /add-query
7. Before raising a PR?           → /code-quality-check
```

---

---

## 📦 Tech Stack

| Layer | Technology |
|---|---|
| API | ASP.NET Core 8, MediatR 12, FluentValidation 11, Serilog |
| Authentication | JWT Bearer (HS256), `Microsoft.AspNetCore.Authentication.JwtBearer` |
| Data | Entity Framework Core 8, PostgreSQL 16 (Npgsql) |
| Resilience | Polly 8 (exponential-backoff retry), `IMemoryCache` |
| Observability | Serilog structured logging, Correlation ID middleware, Health Checks |
| UI | Angular 19 (standalone), Angular Material, TypeScript |
| Testing | xUnit, NSubstitute, FluentAssertions, `Microsoft.AspNetCore.Mvc.Testing` |
| Containerisation | Docker, Docker Compose |
