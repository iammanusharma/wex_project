---
description: >
  Guided requirement implementation. Reads a requirement, explains it, proposes 
  an implementation plan respecting all architecture and quality rules, asks for 
  approval, implements it, then produces a compliance checklist confirming all 
  standards are met.
---

# Implement Requirement

## Step 1 — Gather the Requirement

Ask the user:
> "Please paste or describe the requirement you want to implement. You can paste raw text, a user story, or acceptance criteria."

Wait for their input before proceeding.

---

## Step 2 — Analyse & Explain

Read the requirement carefully, then respond with:

### 📋 Requirement Summary
- Restate the requirement in plain English (2–3 sentences)
- Identify the **input** (what the user/system provides)
- Identify the **output** (what the system returns or does)
- Call out any **validation rules** and **business rules** explicitly
- Flag any **external dependencies** (APIs, databases, third-party services)
- Flag any **edge cases** or **error scenarios** that must be handled

---

## Step 3 — Implementation Plan

Produce a detailed plan structured by Clean Architecture layer.
For each layer, list exactly what files will be created/modified and why.

### Layer Breakdown

**🏛️ Domain Layer** (`src/WEX.Domain`)
- New entities, value objects, or constants needed
- New repository interfaces or service interfaces
- New domain exceptions for each error scenario

**⚙️ Application Layer** (`src/WEX.Application/Features/...`)
- Command(s) or Query(ies) as `record` types implementing `IRequest<T>`
- FluentValidation validators — one rule per validation requirement
- Handler(s) — dependency list, logic flow, exception paths
- Response DTOs

**🔧 Infrastructure Layer** (`src/WEX.Infrastructure`)
- Repository implementations
- External API clients (with Polly resilience + IMemoryCache if applicable)
- EF Core migrations or configuration changes

**🌐 API Layer** (`src/WEX.API/Controllers/v1`)
- Endpoint(s): HTTP verb, route, request/response shape
- HTTP status codes for each scenario (200, 201, 400, 404, 422, etc.)
- Exception mappings to Problem Details

**🧪 Tests**
- Unit tests: list each test case by name (`MethodName_Scenario_Expected`)
- Integration tests: list each scenario to cover

### Quality Guardrails Applied
Explicitly list which guardrails apply to this requirement:
- [ ] Architecture dependency rules enforced
- [ ] SOLID principles applied (call out which ones)
- [ ] Nullable reference types, typed exceptions
- [ ] Async/await + CancellationToken throughout
- [ ] Structured logging with CorrelationId
- [ ] Input validation via FluentValidation
- [ ] No secrets or sensitive data in logs
- [ ] Security headers, no raw SQL
- [ ] Unit + integration tests for all paths

---

## Step 4 — Ask for Approval

Ask the user:
> "Does this plan look correct? Should I proceed with implementation? (yes / adjust plan first)"

**Wait for confirmation before writing any code.**

---

## Step 5 — Implement

If the user confirms, implement layer by layer in this order:
1. Domain (entities, interfaces, exceptions)
2. Application (command/query, validator, handler)
3. Infrastructure (repository, external services)
4. API (controller, exception mappings, DI registration)
5. Tests (unit tests first, then integration tests)

After each layer, briefly state what was created before moving to the next.

Run `dotnet build` after the API layer to catch compile errors before writing tests.
Run `dotnet test` after all layers are complete.

---

## Step 6 — Compliance Report

After implementation, produce a final compliance checklist:

### ✅ Implementation Complete — Compliance Report

**Requirement**: [restate in one line]

#### Architecture Compliance
| Rule | Status | Evidence |
|---|---|---|
| Domain has zero external dependencies | ✅ / ❌ | [file/reason] |
| Application depends only on Domain | ✅ / ❌ | [file/reason] |
| No business logic in Infrastructure or API | ✅ / ❌ | [file/reason] |
| Dependency Injection — interfaces only | ✅ / ❌ | [file/reason] |

#### Quality Guardrails
| Gate | Status | Notes |
|---|---|---|
| SOLID principles | ✅ / ❌ | |
| Nullable reference types | ✅ / ❌ | |
| Typed domain exceptions | ✅ / ❌ | |
| Async + CancellationToken | ✅ / ❌ | |
| Structured logging | ✅ / ❌ | |
| FluentValidation on all inputs | ✅ / ❌ | |
| Security (no raw SQL, no secrets) | ✅ / ❌ | |
| RFC 7807 Problem Details errors | ✅ / ❌ | |

#### Test Coverage
| Test | Type | Outcome |
|---|---|---|
| [test name] | Unit / Integration | ✅ Pass |

#### Build & Test Results
- `dotnet build`: ✅ 0 errors / ❌ [error count]
- `dotnet test`: ✅ All passed / ❌ [failure count]

#### Files Created / Modified
List every file created or modified with a one-line description of its role.

---

Refer to `.github/copilot-instructions.md` throughout for all project standards.
