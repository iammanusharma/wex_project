# Capstone Logistics — Architecture Governance Standards

> **Purpose:** This is the editable reference for Capstone Logistics architecture standards.
> It is consumed by AI skills (e.g., `implement-user-story`) to ensure all generated code
> and reviews align with these principles. Update this file when standards evolve.

---

## 1. Services Architecture & Communication

- **Appropriate Decomposition:** For smaller applications, a well-structured modular monolith is valid. Large-scale decomposition requires business consensus — it is not an output of a story implementation.
- **Event-Driven Cooperation:** Prefer async/event-based collaboration over synchronous HTTP chains wherever practical.
- **Minimize Direct Service-to-Service Calls:** Prefer pub/sub or message queues over synchronous RPC chains between services.
- **Choreography over Orchestration:** Favor decentralized choreography (services reacting to events) over centralized orchestrators that embed business flow logic.

---

## 2. Data Architecture

- Each service must own its data. Direct database integration (shared tables/schemas across services) is forbidden.
- Data is exposed only via APIs or published events — never via direct DB connections to external consumers.

---

## 3. Infrastructure & Deployment

- All services should be containerized (Docker/Kubernetes) for environment consistency.
- Minimize vendor lock-in. Microsoft/.NET/SQL Server are acceptable. Heavy reliance on proprietary Azure PaaS where open-standard alternatives exist should be justified.

---

## 4. Observability & Instrumentation

- Applications must emit **structured logs** (key-value pairs, not free-text strings).
- **Distributed tracing** must be implemented, especially across async boundaries.
- Health and performance **metrics** must be exposed for monitoring.
- Log correlation IDs must be propagated across service calls.

---

## 5. Security Architecture

- **No hardcoded credentials or secrets in source control.** Use environment variables or Azure Key Vault.
- All APIs must enforce **authentication** (centralized identity provider) and **authorization** at the service boundary.
- **Least privilege:** Services, processes, and users operate with minimum required permissions.
- **Sensitive data** must be encrypted at rest and in transit.
- **Input validation:** All inputs at service boundaries (API endpoints, event payloads, UI forms) must be validated and sanitized.

---

## 6. API Standards

- **Contract-first:** OpenAPI/Swagger spec is defined before implementation.
- **Versioned from day one:** Breaking changes require a new version; backward compatibility maintained for a defined deprecation period.
- **RESTful conventions:** Proper HTTP verbs, status codes, and resource-based URLs.
- **No direct database exposure:** APIs are the only permitted external data access mechanism.
- **Async-first:** Prefer async patterns, especially for long-running operations.

---

## 7. UI Architecture

- UI must not contain business logic — presentation and user interaction only.
- UI communicates exclusively through well-defined APIs. Direct DB access or service-layer imports are forbidden.
- Follow WCAG accessibility guidelines.
- State management must be explicit and predictable — no implicit global state.

---

## 8. Coding & Quality Standards

- **SOLID Principles:** Applied aggressively to ensure maintainability, testability, and clear separation of concerns.
  - **S** — Single Responsibility: each class/method has one reason to change.
  - **O** — Open/Closed: open for extension, closed for modification.
  - **L** — Liskov Substitution: subtypes must be substitutable for base types.
  - **I** — Interface Segregation: prefer narrow, focused interfaces over fat ones.
  - **D** — Dependency Inversion: depend on abstractions, not concretions; use DI containers.
- **Testability:** Architecture must support unit, integration, and end-to-end tests via DI, mocking, and clear system boundaries.

---

## 9. Tactical & Hygiene Standards

- **Framework versions:** Run on actively supported, LTS framework/runtime versions. EOL versions are a defect.
- **Dependencies:** Must be current, maintained, and free of known vulnerabilities. Remove unused/abandoned packages.
- **Configuration & Secrets:** All environment-specific config externalized. Secrets never in source control.
- **Error handling:** Consistent structured error handling. Unhandled exceptions surfacing to users are a defect. Transient failures handled with retry/circuit-breaker patterns.
- **Code organization:** Consistent folder/namespace structure. Remove dead code and duplicated logic.
- **CI/CD:** Builds automated and repeatable. Tests run in CI and block deployment on failure.
