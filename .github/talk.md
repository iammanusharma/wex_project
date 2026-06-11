# Engineering Manager Interview Prep
## AI-First SDLC — From Ideation to Delivery

---

## OPENING — Your EM Positioning Statement (90 seconds)

> *"I want to walk you through how I approached this project — not just as an engineer writing code, but as someone thinking end-to-end about how a team would build, own, and evolve this system responsibly.*
>
> *The project itself is a corporate payments API — store a transaction in USD, retrieve it converted to any foreign currency using live US Treasury data. But the more interesting story is the decisions I made before a line of code was written: how I defined the architecture so a team could work on it without stepping on each other, how I embedded AI into the development workflow so quality scales with team size, how I thought about responsible AI adoption, and how I would measure whether any of it was actually working.*
>
> *I'll take you from problem framing through architecture, through the AI-assisted delivery model, through what I'd track in production — and I'll be honest about what I'd do differently if I were running this with a real team of eight engineers."*

---

## PHASE 1 — IDEATION & PROBLEM FRAMING

> Before any technical decision, spend time on the problem statement — not the requirements, the **problem**.

### The questions I asked before designing anything

**1. What are the failure modes?**
> The US Treasury API is external. It can be slow, down, or return unexpected data. The architecture needs to handle all three gracefully — not crash, not silently return wrong data, and not cause cascading failures.

**2. Who are the consumers of this system?**
> A REST API consumed by a UI and potentially other services. The contract — request/response shape, error format — needs to be stable, versioned, and predictable. RFC 7807 Problem Details is not just a standard — it's a contract with consumers.

**3. What does the team need to be able to do independently?**
> A feature developer shouldn't have to wait for a database developer. This shaped the CQRS decision more than any technical preference.

**4. What is the simplest thing that could go wrong in production?**
> For a financial system: wrong rounding. Chose `MidpointRounding.AwayFromZero` over the .NET default banker's rounding before writing the data model — that's a financial correctness decision, not a coding decision.

---

## PHASE 2 — ARCHITECTURE DECISIONS

### Decision 1: Clean Architecture with strict layer boundaries

```
Domain ← Application ← Infrastructure → API
```

**The team argument (not the technical one):**
- A junior engineer can add to the Application layer without understanding EF Core
- An infrastructure engineer can swap PostgreSQL without touching business logic
- A new team member finds any piece of code in under 30 seconds — every concept has exactly one place it belongs

**The tradeoff I consciously accepted:**
> Used `EnsureCreated()` instead of EF Core migrations for local startup. Wrong for production — loses schema versioning and rollback capability. Made that call explicitly for reviewer experience and documented it. An EM's job is to make conscious tradeoffs and communicate them.

---

### Decision 2: CQRS with MediatR

**Why it matters for a team:**
> Two engineers can work on the 'store transaction' command and the 'retrieve transaction' query simultaneously, in the same feature folder, with zero merge conflicts.

**The onboarding cost question I always ask:**
> "What does this cost a developer who doesn't know it yet?" MediatR has a learning curve. Acceptable because the pattern is consistent throughout — you learn it once and apply it everywhere.

---

### Decision 3: FluentValidation as a pipeline behaviour

> Validation runs automatically before every handler. You can't forget it, skip it, or duplicate it.

**For a team:** A developer writing a new command doesn't have to remember to call validation manually. The pipeline handles it. This improves the average quality of every developer on the team — not just the ones who would have remembered anyway.

---

### Decision 4: Interfaces at every layer boundary

Every external dependency — database, Treasury API, token service — is hidden behind an interface.

**For testing:** Every handler unit-tested with mocked dependencies. No database, no HTTP calls. Fast, deterministic. Fast tests mean developers run them constantly instead of waiting for CI.

---

### Decision 5: External resilience — Polly + cache

- **Polly retry with exponential backoff:** 3 attempts, doubling the wait each time. Why exponential? Prevents thundering herd — 100 simultaneous retries after a fixed 2 seconds would hit the API simultaneously again.
- **60-minute in-memory cache per currency+date pair — including null results.** Caching nulls prevents hammering the API for unsupported currencies.

---

## PHASE 3 — HOW I RAN AI-ASSISTED DEVELOPMENT

### The framing I use for teams

> AI tools are the fastest way to either scale quality or scale inconsistency. Which one you get depends entirely on the context you give the AI and the review practices you build around it.

---

### Step 1: Define the context before any developer touches AI

**What I built: `copilot-instructions.md`**

Written before the first line of application code. Encodes:
- Architecture rules — which layer owns what (hard rules, not guidelines)
- Naming conventions — one name, everywhere, for the same concept
- Error handling rules — typed domain exceptions only
- Logging rules — structured `{TransactionId}`, never string interpolation
- Test rules — naming, structure, tools

**The EM leverage point:**
> You write it once, update it as the team learns, and it shapes every AI-assisted interaction across the entire team. A new engineer and the AI get the same rules at the same time.

---

### Step 2: Build skills as reusable team workflows

Six prompt templates built, each encoding the team's agreed process:

| Skill | When to use |
|---|---|
| `/implement-requirement` | Starting a user story — full layer-by-layer guided implementation with approval gate |
| `/add-feature` | New domain concept — full vertical slice from entity to controller to tests |
| `/add-command` | New write operation — Command, Validator, Handler, unit tests |
| `/add-query` | New read operation — Query, Response DTO, Validator, Handler, unit tests |
| `/add-angular-feature` | New UI page — component, service, typed API integration, route, guard |
| `/code-quality-check` | Before every PR — 8-gate review with PASS/FAIL verdict and file:line citations |

**The design principle for every skill:**
> Must have a human approval checkpoint before generating code. The developer sees the plan, approves it, then the AI implements. Keeps the engineer in control and thinking.

---

### Step 3: The pre-PR quality agent — the most important team tool

Before every PR, the developer runs `/code-quality-check`. Reviews all changed files against 8 gates:

1. Architecture — correct dependency direction, no DbContext outside Infrastructure
2. SOLID — single responsibility, dependency inversion
3. Null safety — no silent null reference failures, typed exceptions
4. Async patterns — CancellationToken everywhere, no `.Result` or `.Wait()`
5. Logging — structured format, no PII, correct severity
6. Security — no raw SQL, no secrets in code, all inputs validated
7. Error handling — typed exceptions, RFC 7807, no silent catches
8. Test coverage — every handler tested, AAA naming, no logic in assertions

**Why this matters for a team lead:**
> Human code reviewers are expensive attention. When a senior engineer spends 30 minutes catching naming violations and missing cancellation tokens, that's 30 minutes not spent on product logic or mentoring. The pre-PR agent handles mechanical checks. Humans focus on decisions.

---

### Where AI failed — and what I did about it

#### Failure 1 — Security bug: username in 401 response

**What happened:** The AI's first version included the username in the `InvalidCredentialsException` message response — a user enumeration vulnerability.

**What I did as an EM:** Didn't just fix the line. Added the rule to the context file with an explicit example of the wrong pattern. Added it to the security gate in the quality check skill. **Every mistake becomes a permanent improvement to the team's guardrails.**

---

#### Failure 2 — Wrong Treasury API field names

**What happened:** AI generated plausible-looking API integration code using field names that didn't match the actual Treasury API. Code compiled, looked correct, would have failed silently in production.

**The lesson for teams:** AI cannot reliably know the exact schema of a third-party API it hasn't seen. **Mandate: before building any external service client, a developer must make one real call to the API and verify the response structure manually. Do this on day one, not after building everything.**

---

#### Failure 3 — String interpolation in structured logs

**What happened:** Repeatedly produced `_logger.LogInformation($"Stored {id}")` — string interpolation instead of structured logging. Violated our rule even though the context file stated it.

**Why it kept happening:** String interpolation in log calls is overwhelmingly common in training data. The AI's prior was stronger than our rule.

**Fix:** Added an explicit wrong/right counterexample to the context file — not just a rule, but showing the wrong pattern next to the right one. After that, the AI stopped producing the violation. **Describe the wrong pattern next to the right one.**

---

## PHASE 4 — DELIVERY METRICS & MEASURING SUCCESS

### Pre-shipping metrics (team health)

| Metric | What it signals | How to measure |
|---|---|---|
| PR cycle time (story → merged) | Sustainable delivery speed | GitHub + Jira integration |
| Quality gate pass rate at first submission | AI helping or creating rework? | CI gate results per developer |
| Review comment count per PR | Pre-PR agent reducing back-and-forth? | GitHub PR analytics |
| Build failure rate | Codebase health | GitHub Actions |
| Test coverage at merge | Maintaining testing bar | Coverage reports in CI |
| Time to first PR for new engineers | Onboarding speed | Manual tracking per hire |

### Post-shipping metrics (system health)

| Metric | What it signals | How to measure |
|---|---|---|
| Treasury API error rate | Resilience working? | Serilog + dashboard |
| Cache hit rate | Caching reducing external load? | Custom log counter |
| P99 response time | Performance acceptable? | Health endpoint + APM |
| 422 rate per currency | Which currencies failing conversion? | Structured logs by currency field |
| 5xx rate | All error paths caught? | Error rate dashboard |

### The metric I'd prioritise above all others

> **Quality gate pass rate at first PR submission.** Below 80% means the context file or skills need work. Either the AI produces code that doesn't meet standards, or developers aren't running the pre-PR check. Both are fixable — but only if you're measuring.

---

## PHASE 5 — TEAM STRUCTURE & WAYS OF WORKING

### Team structure

> Organise around flow, not function. Not 'frontend team' and 'backend team' — that creates handoff delays and diffuses ownership.

- Two full-stack feature squads — each owns a vertical slice from UI to database
- One platform engineer — owns AI tooling, CI/CD, observability infrastructure
- One tech lead — owns architecture decisions and the code review bar

### The AI SDLC guild

> A small cross-functional group (3–4 people): senior engineer, security-aware developer, tech lead. They own the context file, skills library, and quality gates. Changes go through a PR process — same as production code. **The AI tooling is treated as production infrastructure, not a side project.**

### Onboarding the workflow

- **Week 1:** No code. Read context file, run all tests, run quality check against a past PR, pair with senior on one feature using the implement-requirement skill.
- **Week 2:** Own a full feature with the pre-PR check as safety net.

**The goal:** First PR should need review comments only on product logic — never on structure, naming, or patterns.

### Non-negotiable team norms

1. The pre-PR quality check is part of the definition of done. A PR opened without it gets returned, not reviewed.
2. Every AI-generated suggestion gets read before it's committed. The engineer is the author of every line.
3. When AI produces something wrong, the fix goes into the context file — not just into the code.

---

## PHASE 6 — RESPONSIBLE AI

### The four areas I think about

**1. Data safety**
> No customer data, no PII, no real transaction amounts go into AI prompts. A prompt sanitisation practice and explicit team training — not a policy document on a SharePoint. A conversation in every onboarding session and a gate in the pre-PR check.

**2. Attribution and ownership**
> Every AI-generated line is owned by the engineer who committed it. If it's wrong, the engineer is accountable — not the AI. This changes how people review AI output: if you believe the AI is responsible, you skim. If you know you're responsible, you read.

**3. Bias in generated patterns**
> AI models are trained on public code — which has a lot of bad patterns. The context file and skills are the defence. Periodically audit AI-assisted PRs to check whether the AI introduced subtle anti-patterns the quality gates don't yet catch — then add those patterns to the gates.

**4. Over-reliance and skill atrophy**
> If junior engineers always accept the AI's architectural suggestion without understanding why, they can't catch the 20% of cases where the AI is subtly wrong.
>
> Addressed through:
> - Code reviews that ask "why did you structure it this way?"
> - Architecture discussions where developers defend decisions
> - Skills with human approval gates — the developer must engage with the plan before the AI generates code

---

## PHASE 7 — WHAT I WOULD DO DIFFERENTLY WITH A TEAM

**1. Treat the context file as a living team artifact from day one**
> Update it in every sprint retrospective: "Did we catch any AI patterns this sprint that our rules don't cover?" The context file should improve every two weeks, not just when something goes wrong.

**2. Build the eval harness in sprint one**
> An eval harness is a set of test scenarios for the AI tooling — give it a requirement, check if the generated code passes all quality gates. Without it, you can't know if a change to the context file improved or degraded AI output. It's CI/CD for your prompts.

**3. Instrument AI-specific metrics from the first deployment**
> How often does the pre-PR quality check produce violations per developer per sprint? Which gates fail most often? If 70% of violations are in the logging gate, the logging rules need to be clearer or the skills need a better example.

**4. Run a formal threat model session before launch**
> The user enumeration vulnerability was caught during development by code review. In a team setting, run a structured threat modelling session (STRIDE or similar) with the whole team before any external release — to find bugs *and* to build the team's security thinking.

**5. Have an explicit 'AI usage escalation path'**
> When a developer isn't sure whether a prompt or usage is safe, they need a clear person to ask — not just a policy document. Designate that person from week one. Log the answers so they become team reference.

**6. Retire EnsureCreated() for EF Core migrations before first staging deploy**
> The friction of one extra command is worth the rollback safety in production.

---

## Q&A — EM-Specific Questions with Strong Answers

---

**Q: How do you balance moving fast with maintaining quality when adopting AI tools?**

> You don't balance them — you design it so they reinforce each other. The pre-PR quality agent is the mechanism: developers move faster because AI handles mechanical work, and quality is maintained because the agent catches violations before human review. Speed and quality are in conflict only when quality relies on human vigilance alone. Automate the vigilance and you break the tradeoff.
>
> Leading indicator: quality gate pass rate at first PR submission. If it's high and cycle time is decreasing — you've achieved both. If cycle time drops but quality drops too, the AI is making people faster but sloppier. Fix the context, not the people.

---

**Q: How do you onboard engineers who have never used AI-assisted development?**

> Three stages:
> 1. **Observation** — pair with an experienced developer for one full feature, watching the workflow
> 2. **Guided practice** — use the skills on a low-risk task with senior review
> 3. **Independent** — own a full feature with the pre-PR check as safety net
>
> Don't give someone a Copilot license and a 'getting started' video. Unguided AI adoption produces unguided results. The skills are the onboarding curriculum — the developer learns the right approach by doing it.

---

**Q: How do you handle AI code that passes quality gates but is architecturally wrong in a subtle way?**

> Quality gates catch structural violations. They can't catch 'this is technically correct but violates the intended design direction'. That's what tech lead review is for.
>
> The approach: make the quality gates good enough to eliminate easy failures, so human reviewers have bandwidth for hard ones. When a subtle architectural issue gets through, it goes into an Architecture Decision Record and a note in the context file — "we once did X, it caused Y, don't do it again". The codebase's institutional memory grows.

---

**Q: What's your view on AI agents that autonomously submit PRs?**

> Valuable for the right scope. Excellent for well-defined, bounded tasks with clear acceptance criteria — writing tests for an existing function, updating dependencies, fixing a specific linting violation.
>
> Risky for anything requiring understanding intent — new features, refactoring, architectural decisions.
>
> Rule for teams: the task must have a definition of done that can be verified without human judgment. If you can't write a test that definitively says "this is correct", the task is too ambiguous for an autonomous agent. Every agent-submitted PR goes through the same review bar as a human-submitted one.

---

**Q: How do you make the business case for investing in AI tooling infrastructure?**

> Three numbers:
> - **Onboarding time to first PR** — before and after the skills workflow
> - **PR cycle time** — before and after
> - **Post-merge defect rate** — before and after
>
> These map directly to business outcomes: faster onboarding = cheaper ramp-up, faster cycle time = more features per sprint, lower defect rate = less incident response time.
>
> Also frame it as risk reduction: one prevented production security incident pays for the entire AI tooling investment.

---

**Q: What is the most important thing an EM gets wrong when rolling out AI tools?**

> Treating it as a tool problem instead of a culture problem. You buy the licenses, set up the IDE extensions, and wonder why quality didn't improve.
>
> Second mistake: measuring the wrong things. Counting AI suggestions accepted or lines of code generated are vanity metrics. The metrics that matter are delivery speed, quality at first submission, and developer experience. If developers feel more capable with the tools than without them, adoption is working. If they feel like they're babysitting a system that generates plausible-looking bugs, something in the context or the workflow is broken.

---

## CLOSING — The EM Summary (60 seconds)

> *"I approached this as an end-to-end ownership problem: design a system that a team can work on at scale, embed quality guardrails that don't rely on individual vigilance, adopt AI in a way that makes doing the right thing the path of least resistance, and measure outcomes — not activity.*
>
> *The architecture choices — Clean Architecture, CQRS, typed exceptions, interface boundaries — are team decisions, not technical preferences. They exist to give a team of eight engineers the ability to move fast without creating chaos for each other.*
>
> *The AI tooling — context file, skills, quality agent — is the answer to 'how do you maintain quality and consistency as the team grows?' You don't do it through code review heroics. You do it by encoding your standards into the tools your team uses every day.*
>
> *And responsible AI isn't a compliance checkbox. It's the foundation that makes everything else work — because a team that doesn't trust its tools, or that ships AI-assisted vulnerabilities, loses both confidence and credibility.*
>
> *That's the model I know how to build."*

---

## EM CHEAT SHEET — One-Liners for Any Question

| Topic | Your answer |
|---|---|
| Architecture decisions | "Team decisions first, technical preferences second — designed so 8 engineers move fast without conflict" |
| CQRS | "Parallel development without merge conflicts — writes and reads evolve independently" |
| AI context engineering | "Onboarding document that every developer and the AI read simultaneously" |
| Pre-PR quality agent | "Frees human reviewers to focus on decisions, not checklists" |
| Responsible AI | "Data safety, engineer accountability, bias auditing, and preventing skill atrophy" |
| Measuring AI productivity | "Quality gate pass rate + PR cycle time + onboarding speed — not lines generated" |
| What I'd do differently | "Eval harness from sprint one, formal threat model before launch, context file in every retro" |
| Autonomous agents | "Right for bounded verifiable tasks — wrong for anything requiring intent or judgment" |
| Business case | "Onboarding cost, delivery speed, defect rate — one prevented production incident covers the investment" |
| Biggest EM mistake | "Treating AI as a tool rollout instead of a culture change" |

---

## DEVELOPER PROCESS NARRATIVE — How I Actually Built It With AI

> This section covers what I asked the model, why I asked it that way, what failed, what I changed, and how I verified the result.

---

### Before writing any code — context first

> The first thing I did was not open a chat window and say "build me a payment API". I spent time designing the context first. I wrote the `copilot-instructions.md` file before any application code.
>
> If you give an AI an open-ended task with no context, you get code that is technically functional but architecturally random — it matches patterns from Stack Overflow or whatever was most common in training data.

---

### Domain layer — PurchaseTransaction entity

**What I asked:**
> "I need a domain entity for a purchase transaction. Description capped at 50 characters, a date, and a USD amount that must be positive and rounded to 2 decimal places. Use a factory method pattern. Enforce all rules at construction time. Make it sealed. Private setters, private parameterless constructor for EF Core."

**Why so specific:** If I just said "create a PurchaseTransaction class", the AI would give me public properties with setters, no validation, fully mutable. I wanted the domain to enforce its own rules.

**What came back wrong:** The first version used `Math.Round(amount, 2)` — which uses banker's rounding by default in .NET. `1.555` would round unpredictably depending on the preceding digit. Wrong for financial data.

**The fix:**
```csharp
AmountUsd = Math.Round(amountUsd, 2, MidpointRounding.AwayFromZero);
```

**How I verified it:** Theory test with boundary cases:
```csharp
[InlineData(10.005, 10.01)]   // AwayFromZero — this is the critical case
[InlineData(10.004, 10.00)]
```

**Key tradeoff defended:** The entity has a private parameterless constructor for EF Core. A domain entity aware that an ORM might construct it is an impurity. In a pure DDD world, you'd use a separate read model. Cost was not worth it for this project — documented explicitly.

---

### Application layer — ValidationBehavior

**What I asked:**
> "Create a MediatR pipeline behavior that runs all registered FluentValidation validators before the handler. Throw ValidationException with all failures. If no validators registered, pass through."

**What the AI missed:** First version didn't handle the case where no validators are registered.

**The fix I added:**
```csharp
if (!_validators.Any())
{
    return await next();
}
```
> Without this, a command type with no validator would still work but the intent was ambiguous. With it, the behavior is explicit.

---

### Handler — the double-rounding decision

**What the AI got structurally right but I improved:**

First version:
```csharp
var transaction = PurchaseTransaction.Create(request.Description, request.TransactionDate, request.AmountUsd);
```

What I changed:
```csharp
var roundedAmount = decimal.Round(request.AmountUsd, 2, MidpointRounding.AwayFromZero);
// log: rounded from {OriginalAmount} to {AmountUsd}
var transaction = PurchaseTransaction.Create(..., roundedAmount);
```

**Why:** The log should show what was actually stored. If a user sends 49.999, I want to log that I stored 50.00, not that I received 49.999. Double rounding is idempotent — rounding an already-rounded value changes nothing. The benefit is log accuracy.

---

### Structured logging fix — the context file improvement

**What AI kept producing:**
```csharp
_logger.LogInformation($"Storing transaction: {request.Description}");
```

**What I needed:**
```csharp
_logger.LogInformation("Storing purchase transaction. Description: {Description}", request.Description);
```

**What I did:** Added an explicit wrong/right example to the context file. After that, the AI stopped producing string-interpolated logs in this project. **The lesson: don't just describe the right pattern — show the wrong pattern next to it.**

---

### Treasury API — the hardest integration

**What I asked:**
> "I need to call the US Treasury Reporting Rates of Exchange API. Given an ISO 4217 currency code and a purchase date, return the most recent exchange rate within the last 6 months. Return null if not found. Use IMemoryCache with 60-minute TTL keyed on currency+date. The API uses currency names not ISO codes — I need a separate mapper."

**What failed:** The AI produced a URL using `currency_code:eq:EUR`. The actual field is `country_currency_desc` with values like `"Euro Zone-Euro"`. Code compiled, looked correct, would have returned nothing against the real API.

**The fix:** Called the real API manually first (`?fields=country_currency_desc&page[size]=200`), verified all 37 currency mappings, found 4 wrong entries, fixed them.

**The lesson:** Test against the real API on day one. Not after building everything.

---

### Security vulnerability caught in review

**What the AI produced:**
```csharp
Detail = ice.Message  // contained: "Invalid username: john@test.com"
```

**Why it's a bug:** User enumeration — an attacker can discover which usernames exist by watching error messages.

**The fix:**
```csharp
Detail = "Invalid username or password."
```

**What I did as an EM:** Fixed the line AND added the rule to the context file AND added it to the security gate in the quality check skill. **A single bug became a permanent guardrail.**

---

### Testing approach — list cases before writing tests

**My prompt order:**
1. "Given this handler, list every test case that should exist — happy path, every error path, every edge case. Give me names in MethodName_Scenario_ExpectedBehaviour format. Don't write code yet."
2. Review the list, add missing cases
3. "Now write the tests for this list"

**Why this order:** If you ask the AI to write tests directly, it writes the easy ones — usually just the happy path. Asking for the case list first gives you a complete picture before code is written.

**Test I'm most proud of defending:**
```csharp
public async Task Handle_ValidQuery_NormalisesTargetCurrencyToUppercase()
```
> The handler does `ToUpperInvariant()` before calling the exchange rate service. A user might send `eur` lowercase. Without normalisation, the Treasury call would fail. This test catches any future removal of the normalisation immediately.

---

*Document created: 2026-06-11*
*Project: WEX Corporate Payments — github.com/iammanusharma/wex_project*
