---
name: implement-user-story
description: >
  Read an ADO User Story and implement it with unit tests. Use when: implement story,
  code from ADO requirement, implement work item, build feature from story,
  read ADO story and write code, implement user story ID, write code for requirement.
  Fetches the story from ADO, clarifies requirements with the user, then implements
  code and unit tests once confirmed.
argument-hint: "Paste the ADO User Story ID (e.g. 12345) or the full ADO work item URL"
---

# Implement User Story

Fetches an Azure DevOps User Story, engages the user to confirm understanding of requirements, then implements the feature with unit tests.

## When to Use

- "Implement story 12345"
- "Read ADO story and write the code for it"
- "Implement work item https://dev.azure.com/org/project/_workitems/edit/12345"
- "Code the requirement from this ADO story"
- "Build the feature described in story [ID]"

## Prerequisites

- Azure CLI (`az`) installed and logged in (`az login`)
- `azure-devops` extension installed (auto-installed by the preflight script if missing)
- Active session with Work Items Read permission

---

## Standards Reference

All implementation work must conform to the following standards files. Read them before writing any code:

| File | Purpose |
|---|---|
| [`.github/standards/STANDARDS.md`](../standards/STANDARDS.md) | Capstone Logistics Architecture Governance Standards — security, observability, data, API, and services principles |
| [`.github/standards/CODING-STANDARDS.md`](../standards/CODING-STANDARDS.md) | Concrete coding rules — SOLID, naming, DI, error handling, logging, testing, and security conventions |

---

## Procedure

Follow every phase in order. Do **not** skip Phase 2, Phase 3, or Phase 4's manual verification step.

---

### Phase 1 — Pre-flight & Fetch

**1a. Run pre-flight checks (always first)**

```powershell
.\.github\skills\implement-user-story\scripts\preflight.ps1
```

The script checks:
| Check | What happens on failure |
|---|---|
| `az` CLI installed | Prints install URL and stops |
| `azure-devops` extension | Auto-installs; stops if install fails |
| `az login` session | Prints `az login` fix instruction and stops |

If the script exits with code 1, **stop here** and show the user the exact error output. Do not attempt to fetch the story until pre-flight passes.

If pre-flight passes, continue to 1b.

**1b. Collect input**

If the user has not already provided a story ID or URL, use `vscode_askQuestions` to ask:
> "Paste the ADO User Story ID or the full work item URL."

**1c. Run the fetch script**

```powershell
.\.github\skills\implement-user-story\scripts\fetch-story.ps1 -StoryInput "<ID or URL>"
```

Parse the script output to extract:
- **Title**
- **Description** — strip any HTML tags to plain text before displaying
- **Acceptance Criteria** — strip any HTML tags to plain text before displaying

If the fetch script exits with a non-zero code, report the exact error output and stop. Do not guess or invent story content.

---

### Phase 2 — Understand & Clarify Requirements

**2a. Present your understanding**

Show the user a structured summary in this exact format:

```
## Story #<ID> — <Title>

**My Understanding**
<2–4 sentence plain-English summary: what needs to be built, who uses it, why it matters>

**What I plan to implement**
- <key component / change 1>
- <key component / change 2>
- <key component / change 3>

**Assumptions**
- <tech stack, file location, or approach assumptions>

**Questions**
1. <any ambiguity that must be resolved before coding>
2. <…>
```

**2b. After presenting the summary, always end with this exact prompt:**

> "Please answer the questions above (if any), or confirm this understanding is correct so we can move to Phase 3 — implementation planning."

**2c. Iterate until confirmed**

Keep the conversation open. For each user response:
- Update the understanding summary
- Strike through or remove resolved questions
- Add new questions if the reply raises new ambiguities
- End every reply with the same prompt from 2b until the user explicitly confirms (e.g. "yes", "looks good", "proceed", "that's right")

Do **not** write any code until the user confirms.

---

### Phase 3 — Confirm Implementation

**Purpose:** This is the explicit go/no-go gate before any code is written. Phase 3 exists to ensure the user has reviewed and agreed to the full implementation plan (components, assumptions, and scope) presented in Phase 2. No files should be created or modified until the user gives a clear "yes" here. If the user wants to refine anything — scope, approach, file locations, edge cases — they should do so now by returning to Phase 2, not after implementation has started.

Once understanding is confirmed, ask exactly one question using `vscode_askQuestions`:

> "I have a clear picture of the requirement. Should I go ahead and write the implementation?"

Options: `Yes, implement it` / `No, let me refine further`

If the user chooses "No", return to Phase 2.

---

### Phase 4 — Implement

**Purpose:** Write only the production code changes described in the story. No test files are touched in this phase. After delivering the implementation, the user must manually verify everything looks correct before unit tests are written.

#### 4a. Explore the codebase first

Use `semantic_search` and `grep_search` to locate:
- Relevant existing files (services, controllers, repositories, models)
- An analogous existing feature to match patterns

Summarise what you found in 2–3 sentences before touching any file.

#### 4b. Write the implementation

- Read `.github/standards/CODING-STANDARDS.md` and `.github/standards/STANDARDS.md` before writing any code.
- Follow existing conventions exactly (naming, folder structure, DI patterns) — and where they conflict with the standards files, flag the conflict to the user.
- Implement **only** what the story describes — no extra features.
- Handle only error cases stated in the Acceptance Criteria.
- Do **not** create or modify any test files in this phase.

**Standards checklist — verify each point before presenting the implementation summary:**

| Check | Rule (from CODING-STANDARDS.md) |
|---|---|
| No hardcoded secrets | Credentials, connection strings, and API keys must not appear as string literals |
| Constructor injection | All dependencies injected via constructor — no `new` inside business logic |
| SOLID | Each new class has a single responsibility; depends on abstractions, not concretions |
| Structured logging | Use Serilog with structured properties — no `Console.WriteLine` |
| Input validation | Inputs validated at service boundary before passing to business logic |
| Async | `async`/`await` used throughout — no `.Result` or `.Wait()` blocking |
| Error handling | Exceptions caught specifically; transient failures use Polly retry |

#### 4c. Present an implementation summary and ask for manual verification

After all production code edits, output:

```
## Implementation Summary

**Files changed**
- `path/to/file` — <one-line description of what changed>
- …

**Out of scope / deferred**
- <anything not implemented yet>

**Standards Compliance**
| Standard | Status | Notes |
|---|---|---|
| No hardcoded secrets (CODING-STANDARDS §3) | ✅ / ⚠️ / ❌ | <brief note or "N/A"> |
| Constructor injection / DI (CODING-STANDARDS §2) | ✅ / ⚠️ / ❌ | <brief note or "N/A"> |
| SOLID — Single Responsibility (CODING-STANDARDS §1) | ✅ / ⚠️ / ❌ | <brief note or "N/A"> |
| SOLID — Dependency Inversion (CODING-STANDARDS §1) | ✅ / ⚠️ / ❌ | <brief note or "N/A"> |
| Structured logging — Serilog (CODING-STANDARDS §6) | ✅ / ⚠️ / ❌ | <brief note or "N/A"> |
| Input validation at boundary (CODING-STANDARDS §7) | ✅ / ⚠️ / ❌ | <brief note or "N/A"> |
| Async/await — no blocking calls (CODING-STANDARDS §10) | ✅ / ⚠️ / ❌ | <brief note or "N/A"> |
| Error handling — specific catches + Polly (CODING-STANDARDS §5) | ✅ / ⚠️ / ❌ | <brief note or "N/A"> |
| Auth enforced at service boundary (STANDARDS §5) | ✅ / ⚠️ / ❌ | <brief note or "N/A"> |
| Naming conventions followed (CODING-STANDARDS §4) | ✅ / ⚠️ / ❌ | <brief note or "N/A"> |
```
> Legend: ✅ Compliant — ⚠️ Partial / deviation noted — ❌ Not compliant (explain in Notes) — N/A Not applicable to this story

Then tell the user:

> "I've implemented the changes above. Please review the files, run the application, and manually verify everything looks correct. When you're happy, let me know and I'll move on to Phase 5 — writing the unit tests."

Do **not** proceed to Phase 5 until the user explicitly confirms (e.g. "looks good", "proceed", "yes").

---

### Phase 5 — Write Unit Tests

**Purpose:** Now that the implementation is verified, add unit tests that cover every Acceptance Criteria item. No production code should be changed in this phase unless a bug is discovered during test writing — in that case, flag it to the user before making any fix.

#### 5a. Identify the testing framework

Use `grep_search` or `file_search` to confirm the testing framework in use (xUnit, NUnit, MSTest, Jest, etc.) and locate an analogous existing test file to follow as a pattern.

#### 5b. Write the unit tests

- Read `.github/standards/CODING-STANDARDS.md` Section 8 (Testing Standards) before writing any tests.
- Use the same testing framework already present in the solution (xUnit is the standard).
- Cover **every** Acceptance Criteria item with at least one test.
- Include happy-path and the failure/edge cases stated in the AC.
- Name tests using the pattern: `MethodName_Scenario_ExpectedResult`.
- Structure every test with `// Arrange`, `// Act`, `// Assert` sections.
- Use Moq (or NSubstitute where already present) for mocking — no real external dependencies in unit tests.
- Do **not** change any production code files.

#### 5c. Present a test summary

After all test edits, output:

```
## Unit Test Summary

**Test files changed**
- `path/to/FileTests` — <one-line description of tests added>

**Tests added**
- `MethodName_Scenario_ExpectedResult` — covers AC item N
- …

**Out of scope** (not tested)
- <anything explicitly deferred>

**Testing Standards Compliance**
| Standard | Status | Notes |
|---|---|---|
| xUnit framework used (CODING-STANDARDS §8) | ✅ / ⚠️ / ❌ | <brief note or "N/A"> |
| Test naming: MethodName_Scenario_ExpectedResult (CODING-STANDARDS §8) | ✅ / ⚠️ / ❌ | <brief note or "N/A"> |
| Every AC item covered by at least one test (CODING-STANDARDS §8) | ✅ / ⚠️ / ❌ | <brief note or "N/A"> |
| Arrange/Act/Assert structure (CODING-STANDARDS §8) | ✅ / ⚠️ / ❌ | <brief note or "N/A"> |
| Moq used for mocking — no real external dependencies (CODING-STANDARDS §8) | ✅ / ⚠️ / ❌ | <brief note or "N/A"> |
| Tests are fully isolated — no shared mutable state (CODING-STANDARDS §8) | ✅ / ⚠️ / ❌ | <brief note or "N/A"> |
| No hardcoded real credentials in test fixtures (CODING-STANDARDS §3) | ✅ / ⚠️ / ❌ | <brief note or "N/A"> |
```
> Legend: ✅ Compliant — ⚠️ Partial / deviation noted — ❌ Not compliant (explain in Notes) — N/A Not applicable to this story

---

## Quality Standards

| Check | Requirement |
|---|---|
| Scope | Only implements what is in the story's Description + AC |
| Tests | Every AC item has at least one corresponding test |
| Conventions | Matches existing file structure, naming, and DI patterns |
| No guessing | Ambiguities resolved in Phase 2 before any code is written |
| No silent failures | If fetch script fails, report and stop immediately |
| Verified before tests | Unit tests are written only after the user manually confirms the implementation is correct |
| Standards compliant | All code conforms to `.github/standards/CODING-STANDARDS.md` and `.github/standards/STANDARDS.md` |
| No hardcoded secrets | Zero credentials, connection strings, or API keys in source-controlled files |
| SOLID | Classes follow Single Responsibility and Dependency Inversion at minimum |
| Structured logging | Serilog structured properties used — no Console.WriteLine in production code |
| Async | async/await used throughout — no blocking .Result or .Wait() calls |
