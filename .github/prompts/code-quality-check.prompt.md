---
mode: agent
description: >
  Run a comprehensive code quality review against the WEX architecture rules.
  Use this before every PR or after adding a feature.
---

# Code Quality Guardrail Check

Review the current staged/unstaged changes (or the file(s) specified: {{targetFiles}}) against ALL of the following quality gates. Report each violation with file + line number and suggested fix.

---

## Gate 1: Architecture Dependency Rules
- [ ] `WEX.Domain` has no references to Application, Infrastructure, API, or any NuGet except framework
- [ ] `WEX.Application` references only Domain (no EF Core, no HTTP, no Infrastructure types)
- [ ] `WEX.Infrastructure` does NOT contain business logic — only data access and external calls
- [ ] No `DbContext` usage outside of `WEX.Infrastructure`
- [ ] No `HttpClient` usage outside of `WEX.Infrastructure`

## Gate 2: SOLID Principles
- [ ] **S** — Each class has a single clear responsibility
- [ ] **O** — Extension via new classes/handlers, not modifying existing handlers
- [ ] **L** — No `if (x is ConcreteType)` casting in handlers
- [ ] **I** — Interfaces are narrow (not God interfaces)
- [ ] **D** — Handlers depend on interfaces (`IRepository`), never concrete classes (`Repository`)

## Gate 3: Null Safety & Error Handling
- [ ] Nullable reference types enabled in every `.csproj`
- [ ] No `!` null-forgiving operators without comment justification
- [ ] All domain errors throw typed exceptions (no `throw new Exception("...")`)
- [ ] No `try/catch` swallowing exceptions silently (every catch must log or rethrow)
- [ ] API error responses use Problem Details format (RFC 7807)

## Gate 4: Async / Threading
- [ ] All async methods accept `CancellationToken` and pass it down
- [ ] No `async void` methods
- [ ] No `.Result` or `.Wait()` on Tasks (deadlock risk)
- [ ] No `Task.Run()` in web request handlers

## Gate 5: Logging
- [ ] Structured logging used (not string interpolation): `Log.Info("X {Id}", id)` not `Log.Info($"X {id}")`
- [ ] No sensitive data logged (passwords, raw amounts in audit context, PII)
- [ ] CorrelationId included in all log scopes
- [ ] Appropriate log level used (not everything at `Error`)

## Gate 6: Security
- [ ] No raw SQL string concatenation — EF Core parameterised queries only
- [ ] No secrets in code or appsettings.json (use environment variables)
- [ ] Input validation via FluentValidation on all Commands and Queries
- [ ] No stack traces returned in API responses

## Gate 7: Testing Coverage
- [ ] Every new Command handler has unit tests (happy path + all failure paths)
- [ ] Every new Query handler has unit tests
- [ ] Every new API endpoint has integration tests
- [ ] Test names follow: `MethodName_Scenario_ExpectedBehaviour`
- [ ] No test has logic/loops — keep tests simple and deterministic

## Gate 8: Code Style
- [ ] `.editorconfig` rules satisfied (run `dotnet format --verify-no-changes`)
- [ ] No public members without XML doc comments on public APIs
- [ ] `record` types used for Commands, Queries, and DTOs (not classes)
- [ ] `CancellationToken` last parameter in all async method signatures

---

After checking all gates, produce:
1. **PASS** list — gates with no violations
2. **VIOLATIONS** list — each violation with file:line and fix suggestion
3. **Summary** — overall pass/fail verdict
