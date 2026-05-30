---
mode: agent
description: Scaffold a new Angular standalone feature (component + service + route + tests).
---

# Add Angular Feature: {{featureName}}

**Description**: {{featureDescription}}

Add a new Angular 19 standalone feature to `src/WEX.UI/src/app/features/{{featureName}}/`.

---

## Step 1 — Model (`core/models/` or feature `models/`)

If new API response shapes are needed, add them to:
`src/app/core/models/transaction.models.ts` (or a new `{{featureName}}.models.ts`)

```typescript
// Immutable interfaces only — no classes
// Match exactly to API response shape (camelCase)
export interface {{ResponseType}} { ... }
```

## Step 2 — Service (`core/services/` or feature `services/`)

Add or extend a service:
- Use `inject()` pattern (not constructor injection)
- Inject `HttpClient` and `environment.apiBaseUrl`
- Return `Observable<T>` — never subscribe inside the service
- One method per API call, named to match intent (`create`, `getById`, `getWithCurrency`)

## Step 3 — Component(s)

Create standalone component(s) under:
`src/app/features/{{featureName}}/components/{{componentName}}/{{componentName}}.component.ts`

Rules:
- `standalone: true` always
- Use `signal()` for local reactive state (not BehaviorSubject)
- Use `inject()` for dependencies (not constructor)
- Reactive Forms with `FormBuilder` and validators mirroring backend rules
- Angular Material components for UI (mat-card, mat-form-field, mat-button, etc.)
- Use `@if` / `@for` control flow (not `*ngIf` / `*ngFor`)
- Inline template and styles for small components; separate files if > 80 lines

## Step 4 — Route

Add a lazy-loaded route to `src/app/app.routes.ts`:
```typescript
{
  path: '{{routePath}}',
  loadComponent: () =>
    import('./features/{{featureName}}/components/{{componentName}}/{{componentName}}.component')
      .then(m => m.{{ComponentClass}}),
}
```

## Step 5 — Tests

Create component and service tests:
- `{{componentName}}.component.spec.ts` — use `TestBed` + `HttpClientTestingModule`
- `{{serviceName}}.service.spec.ts` — use `HttpClientTestingModule` + `HttpTestingController`

Test naming: `should {{expectedBehaviour}} when {{scenario}}`

Cover:
- Component renders without errors
- Form validates correctly (required, maxlength, pattern)
- Service calls correct URL with correct method
- Error responses surface via the error interceptor

---

Refer to `.github/copilot-instructions.md` for project-wide standards before writing code.
