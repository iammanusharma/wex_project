# Hack-to-Hire Prompts Playbook
## AI-Assisted Development Under Time Pressure

> This is the **condensed, time-boxed version** of the full `ai-project-prompts-playbook.md`.
> Designed for 4–8 hour hackathons where you need a working demo AND must demonstrate
> engineering manager thinking — not just that it works, but that you know the tradeoffs.

---

## The Golden Rule for Hackathons

> **The context file still comes first — even with 4 hours on the clock.**
> 30 minutes on the context file saves 2 hours fixing AI mistakes.
> This is the one thing you do not cut.

---

## Key Differences from the Full Playbook

| Full Playbook | Hack-to-Hire |
|---|---|
| Phase 0 takes an hour | Phase 0 takes 10 minutes |
| Build everything | Build the ONE best feature — skip the rest intentionally |
| Tests for all paths | One test that proves the core logic — make it count |
| Full CI/CD pipeline | `docker-compose up` is enough |
| All resilience patterns | Note what you'd add — show you know it's missing |
| Full observability | Health endpoint + one structured log line |
| Comprehensive context file | Lean context file — 30 lines you follow beats 200 you ignore |

---

## Timeline Overview

```
00:00 – 00:10   Phase 0   Think before you prompt (no AI)
00:10 – 00:30   Phase 1   Context file + architecture decision
00:30 – 01:00   Phase 2   Scaffold entire working skeleton
01:00 – 03:30   Phase 3   Core feature end-to-end
03:30 – 03:45   Phase 4   One proof-of-life test
03:45 – 04:00   Phase 5   Demo prep
```

---

## PHASE 0 — Think Before You Prompt (10 minutes, no AI)

Answer these yourself first. Do not skip this.

```
1. What is the ONE sentence problem I am solving?
2. What is the ONE feature that best demonstrates the solution?
3. What are the 3 things that if I get wrong will fail the demo?
4. What is my tech stack (backend / frontend / database)?
5. What will I intentionally NOT build — and what will I SAY about it?
```

> **Why no AI yet?** If you ask AI to define your problem, it defines a generic version.
> You need to own the problem statement before you ask for help building the solution.

---

## PHASE 1 — Problem Framing + Context File (00:10–00:30)

### Prompt 1.1 — Frame the problem and identify what to cut

```
I have [X hours] for a hackathon. The problem I am solving is:
[paste the problem statement exactly as given]

In that time I need to demonstrate:
1. Sound architecture with clear reasoning
2. A working core feature (not everything — just the best one)
3. AI-assisted development workflow
4. Engineering manager thinking — tradeoffs, team scalability, what I'd do with more time

Help me:
1. Identify the ONE core feature that best demonstrates the solution
2. Identify what to intentionally cut and what I should say about it in the demo
3. Propose the simplest architecture that is still defensible to a senior engineer
4. List the 3 things that, if I get wrong, will fail the demo
5. What is the "wow moment" in this demo — the one thing judges will remember?
```

---

### Prompt 1.2 — Generate a lean context file

```
I am building [one sentence description] for a hackathon.
Tech stack: [backend / frontend / database]
Time available: [X hours]

Generate a lean copilot-instructions.md for this project covering:
1. Architecture pattern and dependency rules (3 bullet points max per layer)
2. Naming conventions (key ones only — commands, queries, handlers, tests)
3. Error handling standard (one pattern applied consistently everywhere)
4. Logging standard (structured only — show wrong vs right example)
5. Security non-negotiables (minimum: no secrets in code, no PII in logs, no raw SQL)

Also include a section: "HACKATHON SHORTCUTS — INTENTIONAL TRADEOFFS"
List every shortcut I am taking with a one-line note on what the production version would be.
These TODOs are deliberate — they show I know what I cut.

Keep it to 40–60 lines. A file I follow beats a file I ignore.
```

---

## PHASE 2 — Scaffold the Entire Skeleton (00:30–01:00)

### Prompt 2.1 — One-command scaffold

```
Scaffold a complete working skeleton for [project name].

Requirements:
- Compiles and runs immediately with [docker-compose up / dotnet run / npm start]
- Layer structure in place (Domain / Application / Infrastructure / API) even if mostly empty
- One end-to-end "proof of life" route (e.g. GET /health returns 200, GET /ping returns the time)
- Swagger UI enabled in dev mode
- Health endpoint at /health

Tech stack: [your stack]

Shortcuts I am explicitly taking:
- [e.g. In-memory DB / SQLite instead of PostgreSQL]
- [e.g. No auth, or hardcoded test Bearer token: "test-token"]
- [e.g. No migrations — EnsureCreated only]
- [e.g. No retry policy — just a direct HTTP call]

For EACH shortcut, add a TODO comment in the code:
// HACKATHON: Using [shortcut] for speed. Production: [what the real version would be]

These comments are intentional — they demonstrate engineering judgment, not incompetence.

After scaffolding, give me the ONE command to verify it is running.
```

---

### Prompt 2.2 — Verify the skeleton

```
The skeleton is running. Before I build the core feature, verify:

1. What is the curl command to hit the health endpoint and confirm 200?
2. What is the URL for Swagger UI?
3. Is there anything in the scaffold that will cause a subtle bug later
   (e.g. wrong async pattern, missing CancellationToken, wrong DI lifetime)?
4. Is there anything that looks right but will break when I add a database?

Tell me the 2–3 most likely things to go wrong in the next step.
```

---

## PHASE 3 — Core Feature End-to-End (01:00–03:30)

### Prompt 3.1 — Implement the core feature in strict order

```
Implement [core feature name] using the patterns in our copilot-instructions.md.

The feature does: [describe it in one sentence]
Inputs: [list each input]
Output: [what is returned on success]
Error cases: [list each failure that must be handled]

Implement in this exact order. Pause after each step and wait for me to confirm
before moving to the next:

STEP 1: Domain entity with factory method and invariants
STEP 2: Domain exceptions for each error case
STEP 3: Repository interface (Domain.Interfaces)
STEP 4: Command or Query + Validator + Handler (Application layer)
STEP 5: Repository implementation (Infrastructure)
STEP 6: Controller action (API layer)
STEP 7: Wire DI registration

Rules:
- Do NOT generate nice-to-have features
- Do NOT generate more than I asked for
- After each step: give me the command or curl to verify that step works
  before moving to the next
```

---

### Prompt 3.2 — After each step, quick verification

Use this after each step above:

```
I just completed [step name]. Here is what was generated:
[paste the generated code or describe what was created]

Before I move to the next step:
1. Is there any bug in this code that will show up in the demo?
2. Does this violate any rule in our copilot-instructions.md?
3. What is the curl command or test I should run right now to confirm this step works?
4. Is there anything about the .NET / [your framework] default behaviour I should know
   that could surprise me later?
```

---

### Prompt 3.3 — If something is broken

```
This step is not working. The error is:
[paste the exact error message]

The code is:
[paste the relevant code]

Diagnose in this order:
1. Is this a compilation error, runtime error, or wrong behaviour?
2. What is the most likely cause given our architecture?
3. What is the minimal change to fix it without restructuring anything?
4. Is this a problem I should fix now or note as a known issue and demo around?

Do not rewrite the whole file — give me the surgical fix only.
```

---

## PHASE 4 — One Proof-of-Life Test (03:30–03:45)

### Prompt 4.1 — The one test that matters most

```
I have time for ONE unit test before the demo.

Given the core feature I just built ([feature name]), which single test would:
1. Prove the core business logic is correct
2. Catch the most likely regression if someone changes the code
3. Be the most impressive to a senior engineer reviewing the project

Write that test. Follow the naming convention: MethodName_Scenario_ExpectedBehaviour
Use [xUnit/Jest/pytest] + [FluentAssertions/AssertJ/pytest assertions].
AAA structure with blank lines between sections.

After writing it, tell me: if this test passes, what am I still NOT confident about?
```

---

## PHASE 5 — Demo Prep (03:45–04:00)

### Prompt 5.1 — Build your demo script

```
I have [X] minutes before my hackathon demo. My project is:
[describe what you built in 2 sentences]

Help me prepare:

1. OPENING (90 seconds): What problem I solved and why my approach is right.
   Must be clear to a non-technical judge AND technically rigorous to a senior engineer.

2. DEMO SCRIPT: What to show, in what order, with the "wow moment" first.
   Include: what to click/run, what to say while it loads, what to point to in the code.

3. THREE DECISIONS TO DEFEND: The technical choices I should be ready to explain.
   For each: the decision, the one-line justification, the alternative I rejected and why.

4. HOW TO FRAME THE SHORTCUTS: What I intentionally didn't build.
   Turn each TODO comment into a confident statement about production readiness,
   not an apology about running out of time.

5. THE QUESTION I HOPE THEY ASK: Where I have a great answer ready.

6. THE QUESTION I AM WORRIED ABOUT: How to handle it honestly and confidently.

My audience: [engineering managers / senior engineers / technical recruiters / all three]
```

---

### Prompt 5.2 — The "scale to a team" answer

Prepare this — you will be asked it:

```
I built this solo in [X hours]. If an interviewer asks
"how would you run this with a team of 8 engineers?",
give me a 90-second answer covering:

1. How the architecture enables parallel work without conflicts
2. What AI workflow I would put in place from day one
   (context file, skills, pre-PR quality check)
3. The first 3 things I would add that I cut for the hackathon
4. How I would measure if the team is being productive
5. What responsible AI adoption looks like on this team

Keep it conversational — not a list, a story.
```

---

### Prompt 5.3 — The "what would you do differently" answer

```
Prepare my answer to: "What would you do differently if you had more time?"

For [project name], give me a prioritised list of the next 5 things I would build,
in the order I would actually build them, with a one-sentence justification for each.

Structure the answer as an engineering roadmap, not a regret list.
The framing should be: "Here is how this becomes production-ready" —
not "here is what I didn't finish."

Make sure the list covers at least one item in each of:
- Resilience / reliability
- Security
- Observability
- Testing
- Team scalability / AI workflow
```

---

## REUSABLE PATTERNS FOR HACKATHONS

### When you are stuck (use immediately — don't lose 20 minutes)

```
I am stuck on [describe the problem in one sentence].
I have [X] minutes left.

Give me:
1. The most likely cause (one sentence)
2. The fastest fix that unblocks me
3. If the fix takes more than 10 minutes, should I demo around this problem instead?
   If yes, what would I say about it?
```

---

### When a feature is taking too long

```
I am [X] minutes into building [feature] and it is not done.
I have [Y] minutes left total.

Should I:
A) Keep building and demo a partial version
B) Cut this feature and demo what I have
C) Replace it with a simpler version that demonstrates the same concept

Tell me which option and why, given that judges care most about:
[architecture thinking / working demo / AI workflow / all three]
```

---

### Before every code review (quick version)

```
Quick review of [file or component] — I have 5 minutes.

Flag only:
1. Anything that will CRASH in the demo
2. Any security issue that would embarrass me if a judge reads this code
3. Any pattern that directly contradicts our copilot-instructions.md

Do not flag style issues or nice-to-haves. I need the critical list only.
```

---

### Framing shortcuts as decisions (use in the demo)

Instead of: *"I didn't have time to add proper auth"*

Say: *"For the hackathon I used a static Bearer token so we could focus on the core feature. In production this would be JWT with an OAuth2 provider — the `ITokenService` interface is already in place so that's an Infrastructure-only swap."*

Instead of: *"The database is in-memory"*

Say: *"I'm using an in-memory store for zero-config startup. The repository interface is abstracted so swapping to PostgreSQL is one class change in Infrastructure — the rest of the codebase is unaffected."*

Instead of: *"I didn't write many tests"*

Say: *"I wrote the one test that proves the core business logic. With more time I'd add the full suite — the handler test structure is already in place and I know exactly what paths need coverage."*

---

## WHAT JUDGES ARE ACTUALLY LOOKING FOR

In a hack-to-hire for an Engineering Manager role, judges care about:

| What they see | What they are evaluating |
|---|---|
| Your architecture explanation | Can you make good technical decisions under pressure? |
| How you talk about shortcuts | Do you know what you cut and why? |
| Your demo confidence | Can you ship and present? |
| Your answer to "how would a team own this?" | Are you thinking like a manager, not just a developer? |
| Your answer to "what next?" | Do you have a roadmap, not just a prototype? |
| Your AI workflow | Are you using AI as a force multiplier or a crutch? |

> **The differentiator for an EM role:**
> Anyone can demo a working app. What sets you apart is narrating the *decisions* —
> why the architecture is the way it is, what you consciously traded off, and
> how a team of 8 would own this in production.

---

## QUICK REFERENCE — Hackathon Prompt Order

```
00:00  Phase 0:  Answer 5 questions yourself (no AI)
00:10  Prompt 1.1:  Frame the problem, identify the core feature, find the wow moment
00:20  Prompt 1.2:  Generate the lean context file with explicit shortcuts
00:30  Prompt 2.1:  Scaffold the entire working skeleton
00:45  Prompt 2.2:  Verify the skeleton is running
01:00  Prompt 3.1:  Implement core feature step by step (with pauses)
       Prompt 3.2:  Verify after each step
       Prompt 3.3:  Fix if something breaks
03:30  Prompt 4.1:  Write the one proof-of-life test
03:45  Prompt 5.1:  Build demo script
       Prompt 5.2:  Prepare "scale to a team" answer
       Prompt 5.3:  Prepare "what would you do differently" answer
```

---

*Created: 2026-06-11*
*See also: ai-project-prompts-playbook.md (full version) and interview-prep-em-ai-sdlc.md*
