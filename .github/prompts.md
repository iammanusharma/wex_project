# AI-Assisted Project Playbook
## Step-by-Step Prompts from Ideation to Production

> Use this as your personal playbook every time you start a new project.
> Copy-paste each prompt, fill in the `[brackets]`, and follow the steps in order.
> Never skip a phase — each one builds context for the next.

---

## PHASE 0 — BEFORE YOU OPEN AN IDE

> Do this on paper or in a notes file. No AI yet. This is your thinking.

### Questions to answer yourself first

```
1. What problem am I solving? (One sentence — not a feature list)
2. Who uses this system?
3. What are the 3 most likely failure modes?
4. What external systems does this depend on?
5. Who will maintain this after I build it?
6. What is the team size and skill distribution?
7. What does "done" look like — what metrics prove it works?
```

> **Why before AI?** If you ask AI to define your problem, it will define a generic version of it. You need to own the problem statement before you ask for help building the solution.

---

## PHASE 1 — ESTABLISH THE CONTEXT FILE
### (Do this before writing a single line of application code)

This is the most important step. Everything after this is filtered through this file.

---

### Prompt 1.1 — Generate the architecture constitution

```
I am starting a new [web API / full-stack app / microservice] project.

The system is: [describe what it does in 2-3 sentences]

Tech stack:
- Backend: [e.g. .NET 8 / Node.js / Python FastAPI]
- Frontend: [e.g. Angular 19 / React / None]
- Database: [e.g. PostgreSQL / SQL Server / MongoDB]
- Auth: [e.g. JWT / OAuth2 / API Key]

The team is: [e.g. 6 engineers, mix of mid and senior, full-stack]

Generate a copilot-instructions.md file for this project that encodes:
1. Architecture pattern and strict dependency rules (e.g. Clean Architecture layers)
2. Naming conventions for every artifact type (commands, queries, services, repos, tests)
3. Error handling standards (exception types, HTTP status mapping, response format)
4. Logging standards (structured logging rules, what to log, what NOT to log)
5. Security rules (input validation, secrets, SQL injection, PII)
6. Testing standards (tools, naming convention, structure, what must be tested)
7. Async/await rules
8. Configuration management rules

For each rule, include:
- The rule itself
- WHY the rule exists (for a developer reading it)
- An example of the WRONG pattern
- An example of the CORRECT pattern

This file will be loaded by GitHub Copilot automatically for every conversation.
```

---

### Prompt 1.2 — Review and harden the context file

```
Review the copilot-instructions.md you just generated.

For each section, ask:
1. Is this rule specific enough that a developer cannot misinterpret it?
2. Does it include a concrete wrong/right example?
3. Are there any common AI-generated anti-patterns this section doesn't protect against?

Specifically check:
- Does the logging section explicitly prohibit string interpolation in log calls?
- Does the security section cover auth responses that could enable user enumeration?
- Does the error handling section specify the exact HTTP status for each error type?
- Does the test section require tests for ALL failure paths, not just happy path?

Improve any sections that are too vague.
```

---

### Prompt 1.3 — Generate the quality gate skill

```
Based on the copilot-instructions.md above, generate a pre-PR code quality check skill.

The skill should:
1. Review all changed files against every rule in the instructions file
2. Be structured as numbered gates (Architecture, SOLID, Null Safety, Async, Logging, Security, Error Handling, Tests)
3. For each gate: list specific checks as checkboxes
4. Output format: PASS gates listed, then VIOLATIONS with file:line:suggestion, then final PASS/FAIL verdict
5. Never pass a file with a security violation regardless of other gate results

Save this as .github/skills/code-quality-check.md
```

---

## PHASE 2 — SYSTEM DESIGN

### Prompt 2.1 — Architecture decision record

```
I am designing [system name]. Here is what it needs to do:
[paste your problem statement and requirements]

Generate an Architecture Decision Record (ADR) covering:

1. CONTEXT: What problem are we solving and what constraints exist?

2. DECISION: What architectural pattern do we use and why?
   - Justify the choice over 2 alternatives you considered
   - What does this pattern cost the team (learning curve, complexity)?
   - What does it give the team (parallelism, testability, maintainability)?

3. LAYER BREAKDOWN: What belongs in each layer?
   - What can and cannot reference what
   - Where business logic lives
   - Where external dependencies live

4. DATA MODEL: What are the core entities and their invariants?
   - What rules must be enforced at the entity level (not just validation)?
   - What cannot be in an invalid state?

5. EXTERNAL DEPENDENCIES: For each external system:
   - What is the failure mode?
   - How do we handle it gracefully?
   - What is our resilience strategy?

6. TRADEOFFS: What are you consciously giving up with this design?

7. WHAT CHANGES IN PRODUCTION vs THIS DESIGN: What simplifications are acceptable now but need to change before production?
```

---

### Prompt 2.2 — API contract design

```
Design the API contract for [system name].

For each endpoint:
1. HTTP method and route (versioned: /api/v1/...)
2. Request body schema with field-level validation rules
3. Response body schema
4. Every possible HTTP status code and when it is returned
5. Example request and response for the happy path
6. Example response for each error case

Rules:
- All errors must be RFC 7807 Problem Details format (application/problem+json)
- Validation errors (400) must include field-level detail
- No stack traces in any response
- Auth errors (401) must use the same message for wrong username AND wrong password

Also specify:
- API versioning strategy
- Pagination strategy (if applicable)
- Rate limiting strategy (if applicable)
```

---

### Prompt 2.3 — Data model design

```
Design the data model for [system name].

For each entity:
1. Fields with types and constraints
2. Business invariants that must be enforced at the entity level (not in a controller or service)
3. Factory method pattern — what is the only valid way to create this entity?
4. What state transitions are allowed?
5. What exceptions should be thrown when invariants are violated?

Also design:
- Database schema (table names, column types, indexes, constraints)
- EF Core configuration needed
- What rounding/precision rules apply to financial or decimal fields?

Flag any fields where the default .NET behaviour is wrong for this domain
(e.g. decimal rounding, DateTime vs DateOnly, UTC vs local time).
```

---

## PHASE 3 — ENVIRONMENT & PROJECT SETUP

### Prompt 3.1 — Project scaffold

```
Scaffold a [.NET 8 / Node / Python] solution for [system name] following Clean Architecture.

Create the project structure:
- [ProjectName].Domain — entities, interfaces, exceptions, value objects
- [ProjectName].Application — CQRS handlers, validators, service interfaces, pipeline behaviors
- [ProjectName].Infrastructure — EF Core, external API clients, auth, repositories
- [ProjectName].API — controllers, middleware, program.cs, swagger
- tests/[ProjectName].Domain.Tests
- tests/[ProjectName].Application.Tests
- tests/[ProjectName].Infrastructure.Tests
- tests/[ProjectName].API.IntegrationTests

Also generate:
- .editorconfig enforcing naming conventions from the instructions file
- .gitignore (include environment-specific secret files)
- docker-compose.yml with [database] + api + [ui if applicable]
- GitHub Actions CI workflow: restore → format check → build → unit tests → integration tests
- README.md with: what it does, how to run it, how to run tests, environment config table

Do NOT generate any business logic yet. Structure only.
```

---

### Prompt 3.2 — Environment configuration strategy

```
Generate the configuration strategy for [system name].

Create these appsettings files with the right content for each:
- appsettings.json (shared base defaults — committed)
- appsettings.Development.json (Docker Compose / CI overrides — committed)
- appsettings.UAT.json (UAT with #{token}# placeholders for secrets — committed)
- appsettings.Local.json (developer machine — git-ignored, never committed)
- appsettings.Production.json (prod secrets — git-ignored, never committed)

Rules:
- No secrets in any committed file
- JWT secrets and DB passwords go only in Local.json and Production.json
- UAT uses pipeline token replacement syntax
- All config must be bound to strongly-typed Options classes (not magic string lookups)

Also add a startup check that fails fast with a clear message if required config is missing.
```

---

## PHASE 4 — DOMAIN LAYER FIRST

### Prompt 4.1 — Domain entity

```
Create the [EntityName] domain entity following these rules from our copilot-instructions.md:
[paste relevant rules]

Requirements:
- [field]: [type], [constraints]
- [field]: [type], [constraints]

Rules:
1. Sealed class
2. All properties private set
3. Private parameterless constructor for EF Core only
4. Static factory method Create() as the only valid way to construct the entity
5. All invariants enforced at construction — throw typed exceptions, not generic ones
6. Rounding rule for decimal fields: [specify AwayFromZero or other]
7. No external dependencies (no EF Core, no MediatR, no anything)

For each invariant violation, create a typed exception in Domain.Exceptions:
- [ExceptionName] for [condition] — maps to HTTP [status]

After creating the entity, tell me:
1. What assumptions did you make?
2. What edge cases did you NOT handle that I should think about?
3. Is there any .NET default behaviour that could be surprising for this entity?
```

---

### Prompt 4.2 — Repository interface

```
Create the repository interface for [EntityName] in Domain.Interfaces.

Methods needed:
- [describe each operation: add, get by id, list, update, delete]

Rules:
- Interface only — no implementation here
- All methods async with CancellationToken as last parameter
- Return types: use nullable reference types (T? for not-found, not exceptions)
- No EF Core or infrastructure types in the interface
- XML doc comments on every method explaining when null is returned vs when exception is thrown
```

---

### Prompt 4.3 — Domain unit tests

```
Write unit tests for [EntityName] covering:

1. All happy path factory method calls with valid data
2. Every invariant violation (one test per rule) — verify the correct typed exception is thrown
3. Boundary conditions for every constrained field (e.g. exactly at the limit, one over the limit)
4. Rounding behaviour for any decimal fields — test the specific midpoint case (e.g. 1.005)

Rules:
- xUnit + FluentAssertions
- Test naming: MethodName_Scenario_ExpectedBehaviour
- AAA structure with blank line between sections
- No logic in tests — one assertion per concept
- Theory tests for boundary/rounding cases with InlineData

After writing tests, run them mentally and tell me:
- Which test is most likely to catch a future regression?
- Which edge case did I not include that I should?
```

---

## PHASE 5 — APPLICATION LAYER

### Prompt 5.1 — Command (write operation)

```
Create a Command for [operation name] in Application.Features.[FeatureName].Commands.[OperationName].

Operation: [describe what it does]
Inputs: [list each input with type and validation rule]
Output: [what is returned on success]

Generate:
1. [OperationName]Command as a sealed record implementing IRequest<[OutputType]>
2. [OperationName]CommandValidator using FluentValidation
   - One rule per validation requirement
   - Reference domain constants for limits (don't hardcode values)
   - Custom error messages for each rule
3. [OperationName]CommandHandler implementing IRequestHandler
   - Use domain entity factory method — don't construct entities directly
   - Structured logging before and after the operation
   - Use CancellationToken throughout
   - No try/catch — let typed exceptions propagate to the global handler

Rules from our instructions file:
[paste relevant rules]

After generating, tell me:
- Is there any business logic that belongs in the Domain layer, not this handler?
- What are all the ways this operation can fail and which exceptions handle each?
```

---

### Prompt 5.2 — Query (read operation)

```
Create a Query for [operation name] in Application.Features.[FeatureName].Queries.[OperationName].

Operation: [describe what it retrieves and any transformation]
Inputs: [list each input with validation rule]
Output: [describe the response shape]

Generate:
1. [OperationName]Query as a sealed record implementing IRequest<[ResponseType]>
2. [OperationName]Response as a sealed record (DTO — no domain entities in response)
3. [OperationName]QueryValidator using FluentValidation
4. [OperationName]QueryHandler implementing IRequestHandler
   - Fetch from repository
   - Throw typed domain exception if not found (don't return null from handler)
   - Map entity to response DTO — no domain objects in the response
   - Structured logging

Rules:
- Handlers never return null — either a value or a typed exception
- Response DTOs contain no domain logic
- External service calls go through interfaces, not concrete classes
```

---

### Prompt 5.3 — Handler unit tests

```
Write unit tests for [HandlerName] covering ALL of the following:

Happy path:
- [describe expected successful behaviour]

Every failure path:
- [list each exception the handler can throw and what triggers it]

Edge cases:
- [list any data transformation, normalisation, or rounding the handler does]
- [list any external service interactions and their failure modes]

Rules:
- NSubstitute for all mocks: Substitute.For<IInterface>()
- FluentAssertions for all assertions
- Verify mock interactions with .Received(1) where relevant
- Test naming: MethodName_Scenario_ExpectedBehaviour
- No logic or loops in tests
- Each test proves exactly one behaviour

After writing tests, answer:
1. Which mock interaction is most important to verify (not just the return value)?
2. Is there a test case I am missing that a code reviewer would ask about?
```

---

## PHASE 6 — INFRASTRUCTURE LAYER

### Prompt 6.1 — Repository implementation

```
Create the EF Core repository implementation for [EntityName]Repository.

Implement [IEntityNameRepository] using AppDbContext.

Rules:
- Sealed class
- Constructor injection of AppDbContext only
- Use AsNoTracking() on all read operations
- Never expose DbContext outside this class
- SaveChangesAsync after every write — not batched unless specified
- All methods async with CancellationToken

Also create:
- EF Core entity configuration class (IEntityTypeConfiguration<EntityName>)
  - Table name, column names, constraints, indexes
  - Any value conversions needed (e.g. DateOnly, enums)
  - Max length constraints matching domain entity constants

After generating, tell me:
- Is AsNoTracking() correct for all read operations here, or are there cases where tracking is needed?
- Are there any N+1 query risks in these implementations?
```

---

### Prompt 6.2 — External service client

```
Create an HTTP client for [ExternalServiceName].

The service does: [describe what it does]
Base URL: [url]
Endpoint: [endpoint path and query parameter format]
Response format: [describe the JSON response structure]

Generate:
1. [ServiceName]Options — strongly-typed config bound from appsettings
   - BaseUrl, timeout, retry count, cache duration
2. [ServiceName]Response — deserialisation models matching the actual API response
3. [ServiceName]Service implementing I[ServiceName] (interface in Domain or Application)
   - IMemoryCache with TTL from options — include null results in cache
   - IHttpClientFactory registered as typed client
   - Polly retry with exponential backoff (not fixed delay — explain why in a comment)
   - Return null on failure — don't throw from the service, let the handler decide
   - Structured logging: debug on cache hit, info on API call, warning on miss, error on failure

Rules:
- Never throw from the service method — return null for unavailable
- Log the actual URL being called at debug level (helps diagnose API issues)
- Parse response carefully — use TryParse for numeric fields, don't assume format
- Document any known quirks of the external API in comments

Critical: After generating, I need to verify the field names against the real API.
Tell me exactly what curl command I should run to verify the response structure
before I trust any of this code.
```

---

### Prompt 6.3 — Dependency injection wiring

```
Generate the Infrastructure DependencyInjection.cs extension method that registers:
- EF Core DbContext with connection string from config
- All repository implementations (scoped)
- All external service clients with IHttpClientFactory and Polly policy
- All Options bindings
- JWT Bearer authentication with validation parameters from config
- Memory cache

Rules:
- Extension method on IServiceCollection: AddInfrastructure(this IServiceCollection, IConfiguration)
- Options pattern for all config — no raw string reads
- Polly policy as a named private method with a comment explaining the retry strategy
- Fail fast at startup if required config sections are missing

After generating, tell me:
- Are any of these registrations the wrong lifetime (singleton vs scoped vs transient)?
- What happens if the database connection string is missing — does it fail at startup or runtime?
```

---

## PHASE 7 — API LAYER

### Prompt 7.1 — Controller

```
Create [FeatureName]Controller for [system name].

Endpoints:
[For each endpoint:]
- HTTP method + route
- Request model (record)
- Response model (record)
- MediatR command or query it dispatches
- All HTTP status codes with ProducesResponseType attributes

Rules:
- Controller is thin — no business logic
- Constructor injection of IMediator only
- Async all the way with CancellationToken from the action method
- XML doc comments on every action (used for Swagger)
- Sealed class, [ApiVersion("1.0")] attribute
- [Authorize] on the controller (or specific actions)
- Map request model to command/query — do NOT pass request models to Application layer

After generating:
- Does any action contain logic that should be in a handler?
- Are all the ProducesResponseType attributes complete for every error case?
```

---

### Prompt 7.2 — Global exception handler

```
Create a GlobalExceptionHandler implementing IExceptionHandler.

Map these exceptions to HTTP responses:
[List each exception type → HTTP status → Problem Details title and detail]

Rules:
- Never expose stack traces in responses
- All responses use RFC 7807 ProblemDetails with ContentType application/problem+json
- Validation exceptions (FluentValidation) map to 400 with field-level error dictionary
- Auth exceptions use STATIC messages — never include usernames, emails, or identifying info
- Use C# switch expression (not if/else chain)
- Log every exception at Error level with exception type and status code
- Return true from TryHandleAsync to stop further handling

Security check: For each exception that relates to authentication or user lookup,
verify the error message cannot be used to determine whether a username exists.
```

---

### Prompt 7.3 — Middleware: Correlation ID

```
Create CorrelationIdMiddleware for [system name].

Behaviour:
1. Read X-Correlation-ID from request header if present
2. If not present, generate a new GUID
3. Push to Serilog LogContext so all log lines in this request include it
4. Echo the correlation ID back in the response header

Rules:
- Middleware must work even if Serilog is not yet configured (failsafe)
- The response header must be set BEFORE the next middleware runs (not after)
- Use a constant for the header name

Why the response header matters: Clients need the correlation ID to match their
logs to server logs when reporting errors. Without it, debugging is blind.
```

---

## PHASE 8 — INTEGRATION TESTS

### Prompt 8.1 — Integration test setup

```
Create the integration test setup for [system name] API.

Use:
- Microsoft.AspNetCore.Mvc.Testing (WebApplicationFactory)
- In-memory database (or Testcontainers if PostgreSQL-specific behaviour needed)
- Test-specific appsettings (appsettings.Testing.json) with known JWT config

Create:
1. CustomWebApplicationFactory that overrides:
   - Database connection (in-memory or test container)
   - JWT config (known test secret, issuer, audience)
   - Any external HTTP services (WireMock or NSubstitute)
2. A helper method to get a valid JWT token for tests
3. A base test class with HttpClient and auth setup

Rules:
- No real external API calls in integration tests — mock all HTTP dependencies
- Tests must be runnable without any running infrastructure
- JWT config in tests must match what the API validates
```

---

### Prompt 8.2 — Integration tests per endpoint

```
Write integration tests for [endpoint] covering:

Happy path:
- [describe the successful scenario with specific inputs and expected outputs]

Auth:
- Request with no token returns 401
- Request with expired token returns 401

Validation:
- [One test per validation rule — test the boundary, not just a random bad input]

Error paths:
- [Each domain error the endpoint can produce with the expected HTTP status]

Rules:
- Use HttpClient from WebApplicationFactory
- Assert on: status code, Content-Type header, response body shape
- For 400 errors: assert the field-level error is present in the response
- For 422 errors: assert the detail message explains why
- Test naming: Endpoint_Scenario_ExpectedBehaviour
```

---

## PHASE 9 — OBSERVABILITY & DOCKER

### Prompt 9.1 — Structured logging setup

```
Configure Serilog for [system name] with:

Local/Development:
- Console sink with readable format
- Minimum level: Debug for our namespaces, Warning for Microsoft/System

Production:
- JSON format (structured, machine-readable)
- Minimum level: Information

Enrich every log line with:
- CorrelationId (from LogContext — set by middleware)
- MachineName
- ThreadId
- Environment name

appsettings sections for each environment.

Rules:
- All log calls use structured format: Log.Info("X {Id}", id) — never interpolation
- Never log: request bodies, passwords, tokens, PII
- Always log: transaction IDs, user IDs (not usernames), operation names, durations
```

---

### Prompt 9.2 — Docker and health checks

```
Create production-ready Docker configuration for [system name].

1. Dockerfile for the API:
   - Multi-stage build (build stage + runtime stage)
   - Non-root user
   - Health check instruction
   - Minimal base image

2. docker-compose.yml:
   - [database] service with healthcheck
   - API service that depends_on database with condition: service_healthy
   - [UI service if applicable]
   - No secrets hardcoded — use environment variables

3. Health check endpoint at /health:
   - Liveness: is the process running?
   - Readiness: can it reach the database?
   - Wire to EF Core DbContext check

Rules:
- API must not start until database is healthy (use depends_on condition)
- Health endpoint must not require auth
- Response format: standard ASP.NET Core health check JSON
```

---

## PHASE 10 — AI SKILLS FOR THE TEAM

### Prompt 10.1 — Implement-requirement skill

```
Create a Copilot skill for implementing new requirements in [system name].

Save as .github/skills/implement-requirement.md

The skill must:
1. Ask the user to paste the requirement
2. Restate it in plain English with: inputs, outputs, validation rules, error scenarios, external dependencies
3. Produce a layer-by-layer implementation plan (Domain → Application → Infrastructure → API → Tests)
   - List every file to be created or modified
   - List every test case by name
4. STOP and ask for human approval before writing any code
5. Implement in layer order after approval
6. Run build and tests after each layer
7. Produce a compliance report: architecture, quality gates, test results, files created

The approval gate is non-negotiable — the skill must not generate code without it.
The compliance report must include a table of every quality gate from our instructions file.
```

---

### Prompt 10.2 — Add-command and add-query skills

```
Create two Copilot skills:
1. .github/skills/add-command.md — for adding a new write operation
2. .github/skills/add-query.md — for adding a new read operation

Each skill should:
- Ask for the operation name, inputs, output, and which existing feature it belongs to
- Scaffold the correct files in the correct locations
- Generate all unit tests (happy path + all failure paths) in the same step
- Verify the files integrate correctly with existing DI registration
- Run tests after scaffolding

The add-command skill must explicitly ask: "Is there any state change in the Domain
that this command triggers? If yes, should it be represented as a domain method
rather than being constructed directly in the handler?"

The add-query skill must explicitly ask: "Does this query require joining or 
aggregating data? If yes, consider a dedicated read model rather than using the
write-side entity."
```

---

## PHASE 11 — BEFORE YOU SHIP

### Prompt 11.1 — Pre-launch checklist

```
Perform a pre-launch review of [system name] covering:

1. SECURITY REVIEW
   - Run the code-quality-check skill on every file
   - Check every auth endpoint for user enumeration risk
   - Check every external API call — is any sensitive data sent?
   - Check all log statements — is any PII logged?
   - Check all error responses — is any internal detail exposed?

2. RESILIENCE REVIEW
   - For each external dependency: what happens when it is down for 30 seconds? 5 minutes? permanently?
   - Is there a circuit breaker or graceful degradation beyond retry?
   - What is the blast radius of each dependency failure?

3. OBSERVABILITY REVIEW
   - Can you trace a complete request from API entry to database and back using only logs?
   - Does every log line include the correlation ID?
   - Are the health check endpoints correctly wired?
   - What is the first dashboard you would look at if the system went silent at 2am?

4. CONFIGURATION REVIEW
   - Is there any config that, if missing, fails silently rather than at startup?
   - Are all secrets git-ignored?
   - Is the UAT config using token replacement syntax for all secrets?

5. DOCUMENTATION REVIEW
   - Does the README have: what it does, one-command startup, test instructions, environment table?
   - Is every API endpoint documented in Swagger with all status codes?

Produce a table: item, status (PASS/FAIL/REVIEW), action needed.
```

---

### Prompt 11.2 — Define your metrics baseline

```
Before going live with [system name], define the metrics baseline.

Generate a metrics definition document covering:

DELIVERY METRICS (measure from day 1 of development):
- How to measure PR cycle time
- How to measure quality gate pass rate at first submission
- How to measure build failure rate

SYSTEM HEALTH METRICS (measure from day 1 in production):
- What structured log fields to query for error rate
- What structured log fields to query for [external service] failure rate
- What the P99 response time target is and how to measure it
- What a "normal" vs "concerning" value is for each metric

AI TOOLING METRICS (measure per sprint):
- How to count quality gate violations per developer
- Which gates fail most often (tells you where to improve the context file)
- How to track onboarding time to first PR

For each metric: name, how to measure it, what a bad value looks like, what action to take.
```

---

## REUSABLE PROMPT PATTERNS

> These patterns work for any project at any phase.

---

### Pattern: "What am I missing?"

```
I have just [described/built/designed] [thing].

Before I move on, what am I most likely missing that:
1. Would cause a production bug
2. Would cause a security vulnerability
3. Would cause a bad developer experience for the next person
4. I would regret not catching in code review

Be specific — give me the scenario, not just the category.
```

---

### Pattern: "Defend the tradeoff"

```
I chose [approach A] over [approach B] for [component].

Play devil's advocate:
1. What is the strongest argument that approach B was actually better?
2. At what scale or team size does approach A start to break down?
3. What is the single most likely failure mode of approach A that approach B would have avoided?

I want to understand what I gave up, not just confirmation that I chose correctly.
```

---

### Pattern: "Review for the 3am engineer"

```
Review [component/file] from the perspective of an on-call engineer who has never
seen this codebase and is woken up at 3am because this component is failing.

Tell me:
1. Is it obvious from the logs what went wrong?
2. Is it obvious from the code what this component is supposed to do?
3. Is there anything that would cause confusion or a wrong assumption under pressure?
4. What information is missing that would help them fix it faster?
```

---

### Pattern: "Security adversarial review"

```
Review [component] as if you are an attacker looking for vulnerabilities.

Specifically check:
1. Can any error message reveal information about internal state or valid users?
2. Can any input field be used for injection (SQL, command, log injection)?
3. Is there any timing difference in responses that could be used for enumeration?
4. Can any endpoint be called without auth that should require it?
5. Is any sensitive data logged, cached, or included in a response where it shouldn't be?

For each finding: severity (Critical/High/Medium), how to exploit it, how to fix it.
```

---

### Pattern: "Explain like I'll defend this in an interview"

```
Explain [technical decision] in a way that:
1. A non-technical stakeholder could understand the business reason
2. A senior engineer would find technically rigorous
3. I could defend if asked "why didn't you just do [simpler alternative]?"

Include: what problem it solves, what it costs, what alternatives exist, and
why this was the right choice for this specific project and team.
```

---

## QUICK REFERENCE — Phase Order

```
Phase 0:  Think before you prompt (no AI yet)
Phase 1:  Context file + quality gates → before any code
Phase 2:  System design + API contract + data model
Phase 3:  Project scaffold + environment config
Phase 4:  Domain layer (entity → interface → tests)
Phase 5:  Application layer (command → query → tests)
Phase 6:  Infrastructure layer (repo → external service → DI)
Phase 7:  API layer (controller → exception handler → middleware)
Phase 8:  Integration tests
Phase 9:  Observability + Docker
Phase 10: Team AI skills
Phase 11: Pre-launch + metrics baseline
```

---

## THE GOLDEN RULES OF AI-ASSISTED DEVELOPMENT

```
1. Context first, code second — never generate code without a context file
2. Test the real API on day one — never trust AI-generated field names for external APIs
3. Wrong patterns become rules — every AI mistake goes into the context file
4. Human approval before generation — every skill has an approval gate
5. Verify, don't assume — run the tests, check the edge cases, call the endpoints
6. You own every line — review every AI suggestion as if a junior wrote it
7. Metrics from the start — define what "working" looks like before you build it
8. Security is not a phase — it is in every prompt, every review, every gate
```

---

*Created: 2026-06-11*
*Use this playbook for every new project. Update it as you learn.*
