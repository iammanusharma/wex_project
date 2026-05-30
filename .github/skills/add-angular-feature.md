---
description: Scaffold a new Angular 19 standalone feature (component + service + route + tests) for WEX Corporate Payments UI.
---

Ask the user for:
1. Feature name (e.g. "transactions", "reports")
2. Component name (e.g. "create-transaction", "transaction-detail")
3. What the component does (one sentence)

Then create files under `src/WEX.UI/src/app/features/{featureName}/`:

**1. Model** (`core/models/{featureName}.models.ts` or extend existing)
- TypeScript interfaces only — no classes
- Match API response shape exactly (camelCase)

**2. Service** (`core/services/{featureName}.service.ts`)
- `@Injectable({ providedIn: 'root' })`
- Use `inject()` pattern for dependencies
- Return `Observable<T>` — never subscribe inside the service
- One method per API endpoint

**3. Component** (`features/{featureName}/components/{componentName}/{componentName}.component.ts`)
- `standalone: true`
- `signal()` for all local reactive state
- `inject()` for all dependencies (no constructor injection)
- Reactive Forms with `FormBuilder` + validators matching backend rules exactly
- Angular Material components (mat-card, mat-form-field, mat-button, mat-spinner)
- `@if` / `@for` control flow syntax (not `*ngIf` / `*ngFor`)
- Inline template + styles

**4. Route** — add lazy-loaded entry to `src/app/app.routes.ts`

**5. Tests**
- Component spec: `TestBed` + `HttpClientTestingModule`
- Service spec: `HttpClientTestingModule` + `HttpTestingController`
- Test naming: `should {expectedBehaviour} when {scenario}`
- Cover: renders without error, form validation, correct API URL called

Refer to `.github/copilot-instructions.md` before writing any code.
