---
description: Run a comprehensive code quality guardrail check against WEX Corporate Payments architecture and coding standards. Use before every PR or after adding a feature.
---

Review the current changes (or files specified by the user) against all quality gates below.
Report every violation with **file path + line number + suggested fix**.

---

## Gate 1: Architecture Dependency Rules
- [ ] `WEX.Domain` — zero references to Application, Infrastructure, API, or any external NuGet
- [ ] `WEX.Application` — references Domain only; no EF Core, no HTTP clients
- [ ] `WEX.Infrastructure` — no business logic; only data access and external calls
- [ ] No `DbContext` outside `WEX.Infrastructure`
- [ ] No `HttpClient` outside `WEX.Infrastructure`

## Gate 2: SOLID Principles
- [ ] **S**: Each class has one clear responsibility
- [ ] **O**: New behaviour added via new handlers/classes, not by modifying existing ones
- [ ] **L**: No `if (x is ConcreteType)` casting in handlers
- [ ] **I**: Interfaces are narrow and focused
- [ ] **D**: Handlers depend on interfaces, never concrete implementations

## Gate 3: Null Safety & Error Handling
- [ ] Nullable reference types enabled in every `.csproj`
- [ ] No `!` null-forgiving operators without a justifying comment
- [ ] Domain errors use typed exceptions (no `throw new Exception("...")`)
- [ ] No silent `catch` blocks — every catch must log or rethrow
- [ ] API errors return RFC 7807 Problem Details format

## Gate 4: Async / Threading
- [ ] All async methods accept and pass `CancellationToken`
- [ ] No `async void`
- [ ] No `.Result` or `.Wait()` on Tasks
- [ ] No `Task.Run()` inside web request handlers

## Gate 5: Logging
- [ ] Structured logging: `Log.Info("X {Id}", id)` — not `Log.Info($"X {id}")`
- [ ] No sensitive data logged
- [ ] CorrelationId in all log scopes
- [ ] Correct log level used (not everything at Error)

## Gate 6: Security
- [ ] No raw SQL string concatenation
- [ ] No secrets in code or appsettings.json
- [ ] FluentValidation on all Commands and Queries
- [ ] No stack traces in API responses

## Gate 7: Test Coverage
- [ ] Every new Command handler has unit tests (happy + all failure paths)
- [ ] Every new Query handler has unit tests
- [ ] Every new API endpoint has integration tests
- [ ] Test names follow: `MethodName_Scenario_ExpectedBehaviour`
- [ ] No logic/loops inside tests

## Gate 8: Code Style
- [ ] Run `dotnet format --verify-no-changes` — zero violations
- [ ] `record` types used for Commands, Queries, DTOs
- [ ] `CancellationToken` is last parameter in all async signatures
- [ ] Public API members have XML doc comments

---

Produce:
1. **PASS** — gates with no violations
2. **VIOLATIONS** — each with file:line and fix suggestion  
3. **Verdict** — PASS or FAIL with count of violations
