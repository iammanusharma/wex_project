# Capstone Logistics — Coding Standards

> **Purpose:** This file defines the concrete coding rules that all implementation work must follow.
> It is referenced by AI skills (e.g., `implement-user-story`) during Phase 4 and Phase 5.
> Update this file as team conventions evolve.
>
> Parent governance document: [STANDARDS.md](./STANDARDS.md)

---

## 1. SOLID Principles

| Principle | Rule |
|---|---|
| **Single Responsibility** | Each class and method must have exactly one reason to change. Split classes that mix concerns (e.g., business logic + data access in one class). |
| **Open/Closed** | Extend behaviour via new classes or strategy injection — do not modify existing working logic to add features. |
| **Liskov Substitution** | Any subclass or interface implementation must be fully substitutable for its base type without changing caller behaviour. |
| **Interface Segregation** | Define narrow, focused interfaces. Never force callers to implement methods they do not need. |
| **Dependency Inversion** | Depend on abstractions (interfaces), not concrete implementations. All dependencies must be injected via constructor injection. |

---

## 2. Dependency Injection

- Register all services, repositories, and clients in the DI container — never `new` them inside business logic.
- Constructor injection is mandatory. Property and method injection are not permitted.
- Inject `IConfiguration` or typed options objects for settings — never read `Environment.GetEnvironmentVariable` directly inside business/domain classes (only in composition root / startup).

---

## 3. Secrets & Configuration

- **No secrets in source code or config files committed to source control.**
- Secrets are resolved from environment variables or Azure Key Vault at the composition root (startup/`Program.cs`).
- Connection strings, API keys, OAuth credentials, and passwords must never appear as string literals in any `.cs`, `.json`, `.xml`, or `.yaml` file that is source-controlled.
- `appsettings.json` may contain non-sensitive defaults and structural keys only. Use `appsettings.{Environment}.json` (gitignored) or environment overrides for sensitive values.

---

## 4. Naming Conventions (.NET / C#)

| Construct | Convention | Example |
|---|---|---|
| Classes | `PascalCase` | `OrderRepository` |
| Interfaces | `IPascalCase` | `IOrderRepository` |
| Methods | `PascalCase` | `GetOrderById` |
| Private fields | `_camelCase` | `_orderRepository` |
| Local variables | `camelCase` | `orderTotal` |
| Constants | `PascalCase` or `UPPER_SNAKE` | `MaxRetryCount` |
| Test methods | `MethodName_Scenario_ExpectedResult` | `GetOrder_WhenNotFound_ReturnsNull` |

---

## 5. Error Handling

- Never swallow exceptions silently (`catch { }` or `catch (Exception) { }` with no logging/rethrow).
- Catch specific exception types. Only catch `Exception` at the outermost boundary (middleware/global handler).
- Use structured logging to record exceptions — include correlation IDs and relevant context.
- Transient failures (DB timeouts, HTTP 5xx) must use retry with exponential back-off (Polly is the approved library).
- Do not surface stack traces or internal error details to API consumers — return a structured error response.

---

## 6. Logging & Observability

- Use **structured logging** (Serilog is the approved library). No `Console.WriteLine` or `Debug.Write` in production code.
- Every log statement must include relevant context as structured properties, not interpolated strings:
  ```csharp
  // ✓ Correct
  _logger.LogInformation("Order processed {OrderId} for partner {PartnerId}", orderId, partnerId);

  // ✗ Wrong
  _logger.LogInformation($"Order processed {orderId} for partner {partnerId}");
  ```
- Log at the correct level: `Debug` for diagnostics, `Information` for business events, `Warning` for recoverable issues, `Error` for failures.
- Propagate correlation IDs across all service calls and log them on every request.

---

## 7. Security

- **Input validation:** Validate and sanitize all inputs at service boundaries. Use data annotations or FluentValidation.
- **Least privilege:** Never request broader DB permissions, Azure roles, or filesystem access than the feature requires.
- **Authentication/Authorization:** Never bypass `[Authorize]` or equivalent on endpoints that access business data. Validate claims/roles at the controller or policy level.
- **No hardcoded credentials** — see Section 3.
- **SQL:** Use parameterized queries or ORMs. Raw string-concatenated SQL is forbidden.

---

## 8. Testing Standards

- **Framework:** xUnit (current standard). Do not introduce alternative frameworks.
- **Test naming:** `MethodName_Scenario_ExpectedResult` — e.g., `GetOrder_WhenIdIsNegative_ThrowsArgumentException`.
- **Coverage target:** Every Acceptance Criteria item must have at least one test (happy path + stated failure/edge cases).
- **Mocking:** Use Moq (or NSubstitute where already present). Do not use real external dependencies (DB, HTTP) in unit tests.
- **Arrange-Act-Assert:** Structure every test with explicit `// Arrange`, `// Act`, `// Assert` sections.
- **No magic strings:** Extract expected values into named constants or variables in tests.
- **Test isolation:** Each test must be fully independent — no shared mutable state between tests.

---

## 9. Code Organisation

- Follow the existing folder/namespace structure in the solution. Do not create new top-level projects without discussion.
- One class per file. File name must match class name exactly.
- Remove dead code — do not comment it out and leave it.
- No duplicate logic — extract shared behaviour into a shared service or helper, registered via DI.
- Keep methods short: a method that exceeds ~30 lines is a candidate for decomposition.

---

## 10. API Design

- Follow RESTful conventions: proper HTTP verbs (`GET`, `POST`, `PUT`, `PATCH`, `DELETE`), correct status codes.
- All endpoints must return structured responses — never raw strings or untyped `object`.
- Validate request models at the API boundary using data annotations or FluentValidation before passing to business logic.
- APIs must be versioned (`/api/v1/...`) from the first release.
- Use `async`/`await` throughout the call chain — do not block async code with `.Result` or `.Wait()`.
