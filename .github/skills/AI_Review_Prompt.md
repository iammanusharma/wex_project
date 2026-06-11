# Standardized AI Architecture Review Prompt

_Instructions for Leads: Copy the entire prompt below — including the embedded governance standards — and paste it into Copilot (or your approved AI tool). Fill in only the two bracketed fields at the top. The AI will explore and assess the application based on what it can discover from your codebase, diagrams, or attached files._

---

**Prompt:**

Act as an expert Enterprise Software Architect performing a comprehensive quality-focused architecture review. The primary goal is to evaluate how well the application is built — assessing maintainability, testability, security, observability, and separation of concerns — and to produce a realistic improvement plan the team can own and execute. Where structural or architectural defects exist within the application, call them out as quality problems with concrete remediation steps. Do not frame the review as a transformation exercise.

**Application being reviewed:**

1. **Application Name:** [Insert Application Name]
2. **Primary Use Case / Business Function:** [Briefly describe what this app does and who uses it]

Explore the application's codebase, structure, and any available documentation to discover the current architecture. Do not ask for architecture details to be provided — infer them from what is available.

**Key Assessment Directives:**

- **Primary Focus — Quality Improvement:** The review is primarily a quality assessment. Evaluate how well-built the application is across all layers. Produce findings and recommendations the team can realistically act on.
- **Architectural Defects as Quality Issues:** Where the application has structural problems that violate the governance standards below, treat these as quality defects and include them in the remediation plan. Do not frame them as transformation roadmap items unless the fix itself is large-scale.
- **Governance Alignment:** Use the Capstone Logistics Architecture Governance Standards (embedded below) as the quality baseline. Flag deviations as risks, not as gaps from a target platform model.
- **Scope of Review:** Focus on architecture and structural design; do not address low-level coding style or linting — those are handled separately.
- **Areas to Evaluate:** Ensure your review explicitly covers all of the following layers and cross-cutting concerns:
  - UI (User Interface)
  - API
  - Backend
  - Data Layer
  - Instrumentation (Telemetry, logging, and monitoring)
  - Testability
  - Security
  - Application of SOLID coding practices

---

## Capstone Logistics: Architecture Governance & Standards

This document defines the quality standards and architectural principles that Capstone Logistics applications are expected to meet. It serves as the evaluation baseline for architecture reviews.

**Primary Purpose — Quality Improvement:** The principal goal of these reviews is to assess how well each application is built: Is it maintainable? Testable? Secure? Observable? Does it separate concerns appropriately? The remediation and roadmap outputs should focus on realistic quality improvements that each team can own and execute.

**Secondary Purpose — Architectural Awareness:** Where an application exhibits structural patterns that are fundamentally at odds with good architecture (e.g., a UI directly querying a database, shared mutable databases across service boundaries, hard-coded credentials), these must be identified as architectural quality defects and included in the remediation plan. These are not transformation requests — they are quality problems with a quality fix.

### 1. Services Architecture & Communication

#### North Star Vision

The long-term target state for Capstone Logistics is a platform model: cohesive platforms composed of well-defined services that deliver business capabilities. Full decomposition into fine-grained microservices is an eventual goal for larger, complex systems — but it requires business consensus and strategic investment and should **not** be assumed as an output of these reviews.

#### Pragmatic Assessment Stance

Applications in this portfolio range from small utilities to large, complex systems. Reviews must be **right-sized to the application**. The primary question is always: _"What quality improvements can this team realistically make?"_ — not _"How far is this from a microservices platform?"_

For smaller applications, a well-structured modular monolith that scores well on testability, security, observability, and separation of concerns is a fully acceptable and successful outcome. Large-scale architectural transformation requires business consensus and strategic investment and is explicitly **out of scope** for these reviews. Incremental, team-owned improvements are the goal.

#### Services & Communication Principles

- **Appropriate Decomposition:** Favor separating clearly distinct business capabilities into independent deployable units where the complexity and team size justify it. For smaller applications, a well-structured modular monolith is a valid and often preferable starting point over premature decomposition.
- **Event-Driven Cooperation:** Systems should collaborate asynchronously via events wherever practical to improve resilience and reduce temporal coupling. This is directionally correct even for applications not yet fully decomposed.
- **Minimize Direct Service-to-Service Communication:** Avoid synchronous HTTP/RPC chains between services. Where inter-service calls are necessary, prefer pub/sub or message queues to reduce coupling.
- **Choreography over Orchestration:** Favor decentralized choreography (services reacting to events independently) rather than centralized orchestrators that embed business flow logic, preventing single points of failure and domain logic leakage.

### 2. Data Architecture

- **Proper Data Isolation:** Each service must own its data and database. Direct database integration (sharing tables or schemas across distinct services) is expressly forbidden. Data should only be exposed via APIs or published events.

### 3. Infrastructure & Deployment

- **Containerization:** All applications and services should be containerized (e.g., Docker, Kubernetes) to ensure consistency across development, testing, and production environments.
- **Vendor Agnosticism & Dependencies:** Minimize lock-in to specific vendor cloud services.
  - _Exception:_ Microsoft products (e.g., .NET, SQL Server) are acceptable and heavily utilized. However, do not assume this permits heavy reliance on proprietary Microsoft Azure PaaS services if open-standard containerized/event-driven alternatives exist.

### 4. Observability & Instrumentation

- **Comprehensive Instrumentation:** Applications must be fully instrumented to provide high visibility. This includes structured logging, distributed tracing (especially vital for event-driven systems), and health/performance monitoring metrics.

### 5. Security Architecture

- **Security by Design:** Security must be treated as a first-class architectural concern, not a retrofit. Threat modeling and access control decisions should be made at design time.
- **Authentication & Authorization:** All APIs and services must enforce authentication (preferably via a centralized identity provider) and authorization at the service boundary. No service should trust implicit caller identity.
- **Least Privilege:** Services, processes, and users must operate with the minimum permissions required. Overly broad access to databases, message queues, or infrastructure is an architectural defect.
- **Data Protection:** Sensitive data must be encrypted at rest and in transit. Secrets and credentials must never be hardcoded or stored in source control — use a secrets management solution.
- **Input Validation:** All inputs entering a service boundary (API endpoints, event payloads, UI forms) must be validated and sanitized to prevent injection attacks and data corruption.

### 6. API Standards

- **Contract-First Design:** APIs must be defined by an explicit contract (e.g., OpenAPI/Swagger specification) before implementation begins. The contract is the source of truth.
- **Versioning:** APIs must be versioned from day one. Breaking changes require a new version; backward compatibility must be maintained for a defined deprecation period.
- **RESTful Conventions:** Public-facing and internal synchronous APIs should follow RESTful conventions (proper use of HTTP verbs, status codes, and resource-based URLs).
- **No Direct Database Exposure:** APIs are the only permitted mechanism for external data access. No service should expose direct database connections or raw query interfaces.
- **Async-First:** Wherever possible, APIs should prefer async/event-based patterns over synchronous blocking calls, especially for long-running operations.

### 7. UI Architecture

- **Clear Separation of Concerns:** The UI layer must not contain business logic. It should be responsible only for presentation and user interaction, delegating all business rules to the backend via APIs.
- **API-Driven:** UI applications must communicate exclusively through well-defined APIs. Direct database access or service-layer imports from the frontend are forbidden.
- **Accessibility & Standards Compliance:** UI implementations should follow WCAG accessibility guidelines and be built on established, maintainable frameworks.
- **State Management:** UI state management should be explicit and predictable. Avoid implicit global state that creates tight coupling between unrelated UI components.

### 8. Coding & Quality Standards

- **SOLID Principles:** Codebases should aggressively apply SOLID principles to ensure maintainability, testability, and clear separation of concerns at the component level.
- **Testability:** Architectures must support automated testing at all levels (unit, integration, and end-to-end) by applying dependency injection, mocking, and clear system boundaries.

### 9. Tactical & Hygiene Standards

- **Framework & Runtime Versions:** Applications must run on actively supported framework/runtime versions. EOL versions are a security and maintainability defect. Target LTS releases.
- **Dependency Management:** Third-party dependencies must be current, actively maintained, and free of known vulnerabilities. Unused or abandoned packages must be removed.
- **Configuration & Secrets Management:** All environment-specific config must be externalized. Secrets must never appear in source control.
- **Error Handling & Resilience:** Consistent structured error handling is required. Unhandled exceptions surfacing to end users are a defect. Transient failures must be handled gracefully.
- **Code Organization:** Follow a consistent folder/namespace structure. Remove dead code and duplicated logic.
- **Build, CI/CD & Deployment:** Builds must be automated and repeatable. Deployments must be scripted. Automated tests must run in CI and block deployment on failure.

---

**Based on all of the above, please generate an Architecture Review using the following structure exactly:**

---

### Executive Summary

_Audience: CIO and Senior Leadership. Write 3–5 sentences in plain business language. No technical jargon. Summarize the overall health of the application, the most critical risks, and the single most important improvement the team should pursue._

---

### 1. General Assessment

_A technically detailed narrative of the application’s current architectural health. Cover overall structure, key design decisions observed, fitness for purpose, and any systemic concerns. Evaluate across all eight layers: UI, API, Backend, Data Layer, Instrumentation, Testability, Security, and SOLID practices._

---

### 2. Desired State

_Describe what a high-quality, well-architected version of this application looks like — realistic and achievable within a normal team roadmap. Focus on quality and structural improvements, not wholesale transformation. Call out any architectural defects that must be resolved to reach this state._

---

### 3. Strengths & Weaknesses by Component

_This is the core of the review. Organize findings by the application's actual components (e.g., UI, API, Backend Service, Database, Worker/Job, etc.). For each component, assess strengths and weaknesses naturally — weaving in observations about quality, security, testability, instrumentation, data isolation, SOLID practices, and any other relevant governance concerns as they apply. Do not create separate subsections per quality criterion; instead, let the observations flow organically from examining the component. Only include components that exist in the application. For each weakness, include a severity rating (High / Medium / Low)._

#### [Component Name — e.g., UI]

**Strengths**

- **Weaknesses**

-

#### [Component Name — e.g., API]

**Strengths**

- **Weaknesses**

-

#### [Component Name — e.g., Backend Service / Business Logic]

**Strengths**

- **Weaknesses**

-

#### [Component Name — e.g., Database / Data Layer]

**Strengths**

- **Weaknesses**

-

_[Add or remove component sections to match the actual application structure]_

---

### 4. Top 10 Items to Address

_Ranked by impact and urgency. Each item should be specific and actionable — not a restatement of a weakness but a concrete thing the team should do. Mark any item that also qualifies as a Quick Fix with ✓ in the Quick Fix column._

| #   | Item | Layer | Severity | Quick Fix |
| --- | ---- | ----- | -------- | --------- |
| 1   |      |       |          |           |
| 2   |      |       |          |           |
| 3   |      |       |          |           |
| 4   |      |       |          |           |
| 5   |      |       |          |           |
| 6   |      |       |          |           |
| 7   |      |       |          |           |
| 8   |      |       |          |           |
| 9   |      |       |          |           |
| 10  |      |       |          |           |

---

### 5. Quick Fixes

_A prioritized list of improvements that can be completed quickly (days to a few weeks) with immediate, visible impact on product quality. These items must also appear in the Top 10 table above, marked with ✓. Exclude anything requiring significant design, architectural change, or cross-team coordination._

| Priority | Quick Fix | Layer | Expected Impact |
| -------- | --------- | ----- | --------------- |
| 1        |           |       |                 |
| 2        |           |       |                 |
| 3        |           |       |                 |
| 4        |           |       |                 |
| 5        |           |       |                 |

---

### 6. Strategic Recommendation

_Based on your full assessment, provide a single long-term recommendation from the three options below. Choose the option that best fits the application's current state, complexity, and the cost/benefit of improvement. Provide a brief justification (3–5 sentences) explaining why this option is the right call._

| Option          | Description                                                                                            | When It Applies                                                                                                                                  |
| --------------- | ------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Refactor**    | Update dependencies, apply quality fixes, address technical debt — keep the core architecture as-is    | The architecture is fundamentally sound; problems are fixable in place                                                                           |
| **Re-write**    | Re-implement the application from scratch using modern tools and practices — retain the monolith model | The codebase is too degraded to safely improve, but the scope doesn't justify decomposition                                                      |
| **Re-Platform** | Decompose into a cohesive services platform aligned to Capstone's north star                           | The application is large, business-critical, and would deliver significant long-term value through decomposition — requires strategic investment |

**Recommendation:** [ Refactor / Re-write / Re-Platform ]

**Justification:**

>

---

_End of Prompt_
