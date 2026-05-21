# Frontend Architecture PRD — Designare

**Author:** Engineering  
**Date:** 2026-05-21  
**Scope:** Vue 3 frontend (`frontend/src/`) only  
**Status:** Draft — pending eng review  
**Current maturity:** 2.5/10 frontend-specific (testing: 2/10, validation: 5/10, performance: unbasellined)

---

## Problem Statement

The Designare frontend has a well-designed composable/store architecture but lacks the safety layer required for production:

- **~2% test coverage** — auth flow, components, and composables are all untested
- **Validation schemas exist only for auth** — other forms have no centralized validation
- **Global loading spinner** cannot express concurrent request states
- **No performance baselines** — bundle size, LCP, and FCP are unknown
- **No error boundaries** — any render error crashes the full page

This PRD defines the architecture decisions, implementation patterns, and sprint plan to reach production readiness.

---

## 1. Testing Strategy

### Philosophy: Test pyramid, not test trophy

```
         /‾‾‾‾‾‾‾‾‾‾‾‾\
        /   E2E (10%)   \          Playwright — critical user flows only
       /‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾\
      / Component (30%)  \         @vue/test-utils — rendering + interaction
     /‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾\
    /    Unit (60%)        \        Vitest — composables, stores, utils, schemas
   /‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾\
```

**Rationale:** Composables and stores are pure logic — unit-testable in isolation. Components are view-layer wiring — test rendering and user interaction, not internal state. E2E tests cover multi-step flows that no unit test can validate (token refresh, redirect chains, session persistence).

---

### 1.1 Unit Tests (Vitest)

**Target:** All composables, stores, utils, schemas  
**Location:** `composable/__tests__/`, `stores/__tests__/`, `utils/__tests__/`, `schemas/__tests__/`  
**Coverage target:** 80% line coverage on `composables/` and `schemas/`

#### Composables

```typescript
// Pattern: wrap in withSetup to get reactivity context
function withSetup<T>(composable: () => T): T {
  let result!: T
  const app = createApp({ setup() { result = composable(); return () => {} } })
  app.mount(document.createElement('div'))
  return result
}
```

Priority order:
1. `useAsyncState` — isLoading lifecycle, error capture, successMessage, onError callback
2. `useAsyncAction` — same but no data return
3. `useMessage` — toast severity mapping
4. `useApiAuth` — mock axios; verify endpoint calls and payload shape
5. `useApiUser` / `useApiShrines` — same pattern

#### Stores

Already partially tested (`auth.store.spec.ts`, `loading.store.spec.ts`). Gaps to fill:

`auth.store`:
- `initialize()` with valid persisted tokens → authenticated state
- `initialize()` with expired tokens → logout triggered
- `login()` success → tokens stored, `currentUser` populated
- `login()` failure → error state, no token stored
- `logout()` → tokens cleared, idle timer cancelled
- `refreshToken()` → new tokens stored, old revoked
- `refreshToken()` failure → logout triggered
- Idle timer fires after 15 min inactivity → refresh called

`setting.store`:
- Theme toggle → `.app-dark` class added/removed on `<html>`
- Language change → i18n locale updated
- OS `prefers-color-scheme` followed for unauthenticated users

#### Schemas (Zod)

```typescript
// Pattern: test valid + boundary + invalid for each field
describe('loginSchema', () => {
  it('accepts valid credentials', () => {
    expect(loginSchema.safeParse({ email: 'a@b.com', password: 'Valid1!' }).success).toBe(true)
  })
  it('rejects malformed email', () => {
    const result = loginSchema.safeParse({ email: 'notanemail', password: 'Valid1!' })
    expect(result.success).toBe(false)
    expect(result.error.flatten().fieldErrors.email).toBeDefined()
  })
})
```

---

### 1.2 Component Tests (@vue/test-utils + Vitest)

**Target:** All components in `components/auth/`, `components/setting/`, `components/common/`  
**Location:** `components/**/__tests__/`  
**Rule:** No real API calls. Mock composables at the module level.

#### Mock pattern for composables

```typescript
// Mock useApiAuth so components don't need a real HTTP layer
vi.mock('@/composables/api/useApiAuth', () => ({
  useApiAuth: () => ({
    login: vi.fn().mockResolvedValue({ accessToken: 'tok', refreshToken: 'ref' }),
  }),
}))
```

#### Test surface per component

| Component | What to test |
|---|---|
| `LoginForm` | Renders fields; submit with valid data → emits/calls login; invalid email → error shown; server error → inline message |
| `RegisterForm` | Password mismatch → field error; duplicate email (server 409) → inline message |
| `ForgotPasswordForm` | Submit shows success regardless of email existence |
| `Header` (auth) | Shows login link when unauth; shows avatar when auth |
| `GoogleOAuthButton` | Click calls `useApiOAuth().getAuthorizationUrl()` |
| `Avatar` | Shows image when URL present; shows initials fallback when not |
| `ErrorBoundary` | Renders fallback when child throws; retry re-mounts child |

---

### 1.3 E2E Tests (Playwright)

**Target:** Critical user flows only — do not duplicate component test coverage  
**Location:** `e2e/`  
**Strategy:** Use `page.route()` to mock API responses. Tests should not depend on a live backend.

#### Required test files

```
e2e/
  auth.spec.ts          # login, logout, register, OAuth button
  password-reset.spec.ts # forgot → reset happy path + error states
  token-refresh.spec.ts  # silent refresh + refresh failure redirect
  protected-routes.spec.ts # redirect when unauth, access when auth
```

#### Token refresh E2E (key scenario)

```typescript
test('redirects to /auth when refresh token is expired', async ({ page }) => {
  // Seed expired access token + expired refresh token in localStorage
  await page.addInitScript(() => {
    localStorage.setItem('auth', JSON.stringify({
      accessToken: 'expired.access.token',
      refreshToken: 'expired.refresh.token',
    }))
  })
  // Mock refresh endpoint to return 401
  await page.route('**/api/auth/refresh', route =>
    route.fulfill({ status: 401, body: JSON.stringify({ message: 'Refresh token expired' }) })
  )
  await page.goto('/settings')
  await expect(page).toHaveURL('/auth')
  await expect(page.getByText(/session expired/i)).toBeVisible()
})
```

#### CI integration

```yaml
# .github/workflows/frontend.yml
- name: Run unit tests
  run: npm run test:unit -- --coverage
- name: Run E2E tests
  run: npm run test:e2e
  env:
    CI: true
```

---

## 2. Validation System

### 2.1 Current state

```
frontend/src/schemas/
  authSchemas.ts    ✓ exists (login, register, forgot, reset)
  userSchemas.ts    ✗ missing
  shrineSchemas.ts  ✗ missing
  settingSchemas.ts ✗ missing
```

### 2.2 Schema design rules

**Rule 1 — Co-export inferred type with every schema**

```typescript
// DO
export const updateProfileSchema = z.object({ ... })
export type UpdateProfileInput = z.infer<typeof updateProfileSchema>

// DON'T
// Manual interface that can drift from schema
interface UpdateProfileInput { displayName: string }
```

**Rule 2 — Cross-field validation in schema, not component**

```typescript
export const updatePasswordSchema = z.object({
  currentPassword: z.string().min(1),
  newPassword: z.string().min(8).regex(/[A-Z]/).regex(/[0-9]/),
  confirmPassword: z.string(),
}).refine(
  data => data.newPassword === data.confirmPassword,
  { message: 'Passwords do not match', path: ['confirmPassword'] }
)
```

**Rule 3 — Server errors mapped to field errors, not toasts**

The current pattern uses `useMessage().error()` for all server errors. Field-level server errors (e.g. "email already taken") should appear inline, not as toasts.

```typescript
// In useAsyncAction options:
onError: (error) => {
  if (isValidationError(error)) {
    // error.fields: Record<string, string[]>
    Object.entries(error.fields).forEach(([field, msgs]) => {
      form.setFieldError(field, msgs[0])  // PrimeVue form API
    })
  }
}
```

### 2.3 Schema inventory

#### `userSchemas.ts`

```typescript
export const updateProfileSchema = z.object({
  displayName: z.string().min(1).max(50),
  bio: z.string().max(200).optional(),
})

export const updatePasswordSchema = z.object({
  currentPassword: z.string().min(1),
  newPassword: z.string().min(8),
  confirmPassword: z.string(),
}).refine(d => d.newPassword === d.confirmPassword, {
  message: 'Passwords do not match',
  path: ['confirmPassword'],
})

export const uploadAvatarSchema = z.object({
  file: z.instanceof(File)
    .refine(f => f.size <= 5 * 1024 * 1024, 'Max 5MB')
    .refine(f => ['image/jpeg', 'image/png', 'image/webp'].includes(f.type), 'JPEG, PNG, or WebP only'),
})
```

#### `shrineSchemas.ts`

```typescript
export const shrineSearchSchema = z.object({
  keyword: z.string().max(100).optional(),
  locale: z.enum(['en', 'zh']),
  latitude: z.number().min(-90).max(90).optional(),
  longitude: z.number().min(-180).max(180).optional(),
})

export const shrineSuggestionsSchema = z.object({
  keyword: z.string().min(1).max(100),
  locale: z.enum(['en', 'zh']),
})
```

#### `settingSchemas.ts`

```typescript
export const appearanceSchema = z.object({
  theme: z.enum(['light', 'dark', 'system']),
  language: z.enum(['en', 'zh']),
  cursorType: z.enum(['dot', 'stamp']),
})
```

### 2.4 Error handling architecture

**Current flow (broken):**
```
API error → useAsyncAction catches → showErrorToast → toast appears
(field errors never reach the form)
```

**Target flow:**
```
API error
  ├─ 400 ValidationException → parse fields → setFieldError() per field
  ├─ 409 ConflictException   → setFieldError('email', 'Already registered')
  ├─ 422 UnprocessableContent → toast (unexpected server error)
  └─ 5xx                      → toast (server error)
```

**Implementation: `utils/formErrorHandler.ts`**

```typescript
import type { AxiosError } from 'axios'

export type FieldErrors = Record<string, string[]>

export function extractFieldErrors(error: unknown): FieldErrors | null {
  const axiosError = error as AxiosError<FieldErrors>
  if (axiosError?.response?.status === 400) {
    return axiosError.response.data
  }
  return null
}
```

---

## 3. State Management for Concurrent Requests

### 3.1 Problem

`loading.store.ts` exposes a single `isLoading: boolean`. Every `useAsyncState` call currently toggles this. The result:

- Sidebar request triggers full-page spinner
- Two concurrent requests → spinner flickers (toggle on / toggle off / toggle on / toggle off)
- No way to know *which* request is loading

### 3.2 Decision: Remove global spinner, use local loading state

Each `useAsyncState` / `useAsyncAction` already returns `isLoading` as a local ref. The fix is to use it.

**`loading.store` new contract:**

```typescript
// Only for truly global operations: initial app boot, full-page navigation
export const useLoadingStore = defineStore('loading', () => {
  const isAppInitializing = ref(true)  // true until auth.store.initialize() resolves

  function setAppReady() { isAppInitializing.value = false }

  return { isAppInitializing, setAppReady }
})
```

**Component pattern:**

```vue
<script setup lang="ts">
const { data: shrines, isLoading } = useAsyncState(() => useApiShrines().search(params))
</script>

<template>
  <!-- local skeleton, not global spinner -->
  <Skeleton v-if="isLoading" height="200px" />
  <ShrineList v-else :shrines="shrines" />
</template>
```

### 3.3 Concurrent request states

For pages with multiple independent data sources:

```vue
<script setup lang="ts">
const { data: profile, isLoading: profileLoading } = useAsyncState(...)
const { data: shrines, isLoading: shrinesLoading } = useAsyncState(...)
</script>

<template>
  <div class="grid">
    <Skeleton v-if="profileLoading" />
    <UserProfile v-else :profile="profile" />

    <Skeleton v-if="shrinesLoading" />
    <ShrineList v-else :shrines="shrines" />
  </div>
</template>
```

### 3.4 Request deduplication (optional, P3)

If the same request fires twice concurrently (e.g. double-click), deduplicate in the composable layer:

```typescript
const inFlight = new Map<string, Promise<unknown>>()

export function deduplicatedRequest<T>(key: string, fn: () => Promise<T>): Promise<T> {
  if (inFlight.has(key)) return inFlight.get(key) as Promise<T>
  const promise = fn().finally(() => inFlight.delete(key))
  inFlight.set(key, promise)
  return promise
}
```

---

## 4. Performance Targets

### 4.1 Core Web Vitals targets (mobile, 4G)

| Metric | Target | Current | Measurement |
|---|---|---|---|
| LCP (Largest Contentful Paint) | < 2.5s | Unknown | Lighthouse CI |
| FCP (First Contentful Paint) | < 1.8s | Unknown | Lighthouse CI |
| CLS (Cumulative Layout Shift) | < 0.1 | Unknown | Lighthouse CI |
| INP (Interaction to Next Paint) | < 200ms | Unknown | Lighthouse CI |

### 4.2 Bundle size budget

| Chunk | Budget | Action if exceeded |
|---|---|---|
| Initial JS (index chunk) | < 150 KB gzip | Split further |
| Auth chunk | < 80 KB gzip | Audit PrimeVue form components |
| Settings chunk | < 60 KB gzip | Lazy-load heavy components |
| Total JS (all chunks) | < 400 KB gzip | Treemap audit |

Add to `vite.config.ts`:

```typescript
build: {
  rollupOptions: {
    output: {
      manualChunks: {
        'vendor-primevue': ['primevue'],
        'vendor-vue': ['vue', 'vue-router', 'pinia'],
        'vendor-utils': ['axios', 'zod', 'vue-i18n'],
      },
    },
  },
  chunkSizeWarningLimit: 150,  // KB, triggers warning in build output
}
```

### 4.3 Route-level code splitting

All route components must use dynamic import:

```typescript
// router/index.ts — required pattern
{
  path: '/settings',
  meta: { requiresAuth: true },
  component: () => import('@/views/settings/SettingsView.vue'),
}
```

### 4.4 Image optimization

- `compressImage()` util already exists (400px max, JPEG 0.8) — enforce in avatar upload
- Profile images should be served via CDN (blocked on blob storage fix)
- SVG icons via `vite-svg-loader` → inlined (already configured, verify bundle impact)

---

## 5. Three-Sprint Roadmap

### Sprint 1 — Foundation (Weeks 1–2)

**Goal:** Safety net exists for the auth system  
**Definition of done:** Auth flow covered by E2E; composables covered by unit tests; CI blocks on failures

| Task | Issue | Owner | Days |
|---|---|---|---|
| Auth flow E2E tests | P0-FE-01 | Frontend | 4 |
| Token refresh UI feedback | P0-FE-02 | Frontend | 1 |
| Composable unit tests (useAsyncState, useAsyncAction, useMessage) | P1-FE-07 | Frontend | 2 |
| Store unit tests gap-fill (auth.store idle timer, refresh failure) | — | Frontend | 1 |
| GitHub Actions CI pipeline (lint + type-check + test:unit + e2e) | — | DevOps | 1 |

**Sprint 1 exit criteria:**
- [ ] CI passes on every PR
- [ ] Auth flow covered by Playwright (register, login, logout, refresh, reset)
- [ ] `useAsyncState` / `useAsyncAction` unit tested with 80%+ coverage

---

### Sprint 2 — Validation + Reliability (Weeks 3–4)

**Goal:** Forms are consistently validated; crashes are contained; loading states are meaningful

| Task | Issue | Owner | Days |
|---|---|---|---|
| Expand Zod schemas (user, shrine, settings) | P1-FE-05 | Frontend | 2 |
| Field-level server error mapping (formErrorHandler.ts) | — | Frontend | 1 |
| Error boundary component | P1-FE-06 | Frontend | 1 |
| Component unit tests (auth forms — highest priority) | P1-FE-04 | Frontend | 3 |
| Replace global spinner with local loading states | P2-FE-10 | Frontend | 3 |
| Loading skeleton states | P2-FE-12 | Frontend | 2 |

**Sprint 2 exit criteria:**
- [ ] All auth forms have Zod schemas; server field errors appear inline
- [ ] `ErrorBoundary` wraps route-level views
- [ ] No component reads `loadingStore.isLoading` for render logic
- [ ] Skeleton states on all async data sections

---

### Sprint 3 — Performance + Polish (Weeks 5–6)

**Goal:** Performance baselines established; accessibility verified; remaining component tests complete

| Task | Issue | Owner | Days |
|---|---|---|---|
| Bundle analysis + size budget in vite.config | P2-FE-08 | Frontend | 1 |
| Route-level code splitting audit | P2-FE-09 | Frontend | 1 |
| Accessibility audit + fixes | P2-FE-11 | Frontend | 2 |
| Component unit tests (setting, common components) | P1-FE-04 | Frontend | 3 |
| Lighthouse CI integration | — | DevOps | 1 |
| Cookie/token storage audit | P0-FE-03 | Frontend | 0.5 |

**Sprint 3 exit criteria:**
- [ ] Lighthouse CI blocks on LCP > 2.5s or CLS > 0.1
- [ ] Bundle size budget enforced in CI
- [ ] Zero axe-core critical violations on auth page
- [ ] All 30 components have at least one unit test

---

## Appendix: File creation checklist

```
frontend/src/
  schemas/
    userSchemas.ts                     # Sprint 2
    shrineSchemas.ts                   # Sprint 2
    settingSchemas.ts                  # Sprint 2
  utils/
    formErrorHandler.ts                # Sprint 2
  components/
    common/
      ErrorBoundary.vue               # Sprint 2
  composables/
    __tests__/
      useAsyncState.spec.ts           # Sprint 1
      useAsyncAction.spec.ts          # Sprint 1
      useMessage.spec.ts              # Sprint 1
  components/
    auth/__tests__/
      LoginForm.spec.ts               # Sprint 2
      RegisterForm.spec.ts            # Sprint 2
      ForgotPasswordForm.spec.ts      # Sprint 2
      Header.spec.ts                  # Sprint 2
    common/__tests__/
      ErrorBoundary.spec.ts           # Sprint 2
      Avatar.spec.ts                  # Sprint 3
    setting/__tests__/
      PersonalContainer.spec.ts       # Sprint 3
      AppearanceContainer.spec.ts     # Sprint 3
e2e/
  auth.spec.ts                        # Sprint 1
  password-reset.spec.ts              # Sprint 1
  token-refresh.spec.ts               # Sprint 1
  protected-routes.spec.ts            # Sprint 1
```
