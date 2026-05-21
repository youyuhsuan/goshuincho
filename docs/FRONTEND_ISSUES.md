# Frontend GitHub Issues — Designare

Scope: Vue 3 frontend only (`frontend/src/`).  
Format: paste each block into GitHub → New Issue, or use `gh issue create`.

---

## P0 — Critical (block production)

---

### [P0-FE-01] Auth flow E2E tests missing — login, logout, refresh, reset

**Labels:** `testing`, `P0`, `frontend`, `e2e`  
**Estimate:** 3–5 days

**Problem**  
The only Playwright test visits the root URL. The entire authentication flow — register, login, logout, token refresh, forgot/reset password — is untested. A regression in any of these paths would go undetected until a user reports it.

**Scope**  
File: `frontend/e2e/auth.spec.ts` (new)

**Acceptance criteria**
- [ ] Register → verify redirect to authenticated home
- [ ] Login with valid credentials → authenticated state in Pinia store
- [ ] Login with wrong password → error message rendered, no redirect
- [ ] Login with unregistered email → error message rendered
- [ ] Logout → store cleared, redirect to `/auth`
- [ ] Token refresh: mock expired access token, verify silent refresh fires and request retries
- [ ] Forgot password form: submit known email → success state shown
- [ ] Forgot password form: submit unknown email → success state shown (no enumeration)
- [ ] Password reset: valid token → success, redirect to login
- [ ] Password reset: expired/invalid token → error state shown
- [ ] Protected route access without auth → redirect to `/auth`
- [ ] OAuth button renders and links to correct endpoint

**Test setup notes**
- Use Playwright `storageState` fixture to seed authenticated state for tests that don't need to test login itself
- Mock the API layer via `page.route()` to avoid backend dependency in CI

---

### [P0-FE-02] Token refresh has no UI feedback — users see blank states during refresh

**Labels:** `bug`, `P0`, `frontend`, `ux`  
**Estimate:** 1 day

**Problem**  
When the access token expires, `authInstance` silently fires a refresh and queues concurrent requests. During this window (up to ~500ms), any in-flight request is suspended. Components using `useAsyncState` show no loading state because `isLoading` is not re-triggered — it was already false when the request appeared to succeed the first time.

**Files affected**
- `frontend/src/composables/api/useApi.ts` (interceptor)
- `frontend/src/composables/useAsyncState.ts`
- `frontend/src/stores/loading.store.ts`

**Acceptance criteria**
- [ ] During token refresh window, loading state is visible in affected components (or global indicator activates)
- [ ] Queued requests resume and resolve normally after refresh
- [ ] If refresh fails (e.g. refresh token expired), user is redirected to `/auth` with session-expired message — not left on a blank/broken page
- [ ] E2E test covers the redirect-on-refresh-failure path

---

### [P0-FE-03] Verify and harden frontend cookie/token storage assumptions

**Labels:** `security`, `P0`, `frontend`  
**Estimate:** 0.5 days

**Problem**  
`auth.store.ts` persists `accessToken` and `refreshToken` to `localStorage["auth"]`. If the backend ever switches to `HttpOnly` cookies, this creates a split-brain state. Additionally, `localStorage` tokens are readable by any JS on the page (XSS risk).

**Files affected**
- `frontend/src/stores/auth.store.ts`
- `frontend/src/composables/api/useApi.ts`

**Acceptance criteria**
- [ ] Audit: document whether tokens come from `localStorage` or cookies in both dev and prod
- [ ] If `localStorage`: add comment explaining the trade-off; ensure `accessToken` is not logged anywhere
- [ ] `authInstance` interceptor reads token from the single source of truth (not duplicated reads)
- [ ] Test: verify token is cleared from storage on logout
- [ ] Test: verify token is not present in any `console.log` or Sentry breadcrumb

---

## P1 — High (next sprint)

---

### [P1-FE-04] Component unit tests: zero coverage on all 30 Vue components

**Labels:** `testing`, `P1`, `frontend`  
**Estimate:** 5–8 days

**Problem**  
No `*.spec.ts` files exist for any component. Props, emits, slots, and conditional rendering are all untested.

**Priority order** (highest risk first):
1. `LoginForm.vue` — form validation + server error display
2. `RegisterForm.vue` — same
3. `ForgotPasswordForm.vue` — email validation + success/error states
4. `Header.vue` (auth) — conditional authenticated/unauthenticated rendering
5. `GoogleOAuthButton.vue` — OAuth redirect trigger
6. `Avatar.vue` — conditional image/fallback rendering
7. `PersonalContainer.vue` — profile update form
8. `AppearanceContainer.vue` — theme toggle, cursor settings

**Acceptance criteria (per component)**
- [ ] Renders without errors given valid props
- [ ] All conditional branches (loading, error, success, empty) covered
- [ ] User interactions (input, submit, click) trigger correct emits or store mutations
- [ ] Uses `@vue/test-utils` + `vitest`; mocks Pinia stores via `createTestingPinia`
- [ ] No real API calls in unit tests (mock `useApi` or composable return values)

---

### [P1-FE-05] Expand Zod schemas — user, shrine, settings forms unvalidated

**Labels:** `validation`, `P1`, `frontend`  
**Estimate:** 2 days

**Problem**  
Only `authSchemas.ts` exists. Profile update, shrine search, and settings forms use ad hoc validation or none at all. This creates inconsistent error messages and duplicated validation logic across components.

**Files to create**
```
frontend/src/schemas/userSchemas.ts
frontend/src/schemas/shrineSchemas.ts
frontend/src/schemas/settingSchemas.ts
```

**Acceptance criteria**

`userSchemas.ts`:
- [ ] `updateProfileSchema` — displayName (1–50 chars), bio (0–200 chars), optional URL fields
- [ ] `updatePasswordSchema` — currentPassword required, newPassword + confirmPassword with cross-field equality check

`shrineSchemas.ts`:
- [ ] `shrineSearchSchema` — keyword (0–100 chars), locale enum (`en`/`zh`), optional lat/lng range
- [ ] `shrineSearchSchema` used in `SearchBar.vue`

`settingSchemas.ts`:
- [ ] `appearanceSchema` — theme enum, cursor type enum, language enum

All schemas:
- [ ] Export inferred TypeScript type alongside schema
- [ ] Used in their respective form components via `@primevue/forms` Zod resolver
- [ ] Unit tested (valid input passes, invalid input returns correct field errors)

---

### [P1-FE-06] Add Vue error boundary components

**Labels:** `reliability`, `P1`, `frontend`  
**Estimate:** 1 day

**Problem**  
No `onErrorCaptured` wrapper exists. A runtime error in any async-loaded section (e.g. shrine search results, user profile) crashes the entire page. Users see a white screen with no recovery path.

**Files to create**
```
frontend/src/components/common/ErrorBoundary.vue
```

**Acceptance criteria**
- [ ] `ErrorBoundary.vue` uses `onErrorCaptured` to catch descendant errors
- [ ] Shows fallback UI slot with a retry button
- [ ] Logs error to console in dev; sends to error reporting service if configured
- [ ] Wraps at minimum: route-level `<RouterView>` in `App.vue`, async shrine search results section
- [ ] Does NOT swallow errors silently — re-throws after capturing if in dev mode
- [ ] Unit test: verify fallback renders on child throw; verify retry re-mounts child

---

### [P1-FE-07] Composable unit tests — useAsyncState, useAsyncAction, useMessage untested

**Labels:** `testing`, `P1`, `frontend`  
**Estimate:** 2 days

**Problem**  
The three core composables (`useAsyncState`, `useAsyncAction`, `useMessage`) are used everywhere but have zero tests. A regression here breaks every data-fetching operation across the app.

**Files to create**
```
frontend/src/composables/__tests__/useAsyncState.spec.ts
frontend/src/composables/__tests__/useAsyncAction.spec.ts
frontend/src/composables/__tests__/useMessage.spec.ts
```

**Acceptance criteria**

`useAsyncState`:
- [ ] `isLoading` is `true` during async fn, `false` after
- [ ] `data` populated on success
- [ ] `error` populated on failure
- [ ] `showErrorToast: true` calls `useMessage().error()`
- [ ] `successMessage` triggers `useMessage().success()` on resolve
- [ ] `onError` callback fires with the error

`useAsyncAction`:
- [ ] Same loading/error/success behavior as `useAsyncState`
- [ ] Does not expose `data` (returns `void`)

`useMessage`:
- [ ] Each method (`success`, `error`, `warn`, `info`) calls PrimeVue `useToast` with correct severity

---

## P2 — Medium (hardening pass)

---

### [P2-FE-08] Bundle analysis — unknown PrimeVue tree-shaking effectiveness

**Labels:** `performance`, `P2`, `frontend`  
**Estimate:** 0.5 days

**Problem**  
`unplugin-vue-components` auto-imports PrimeVue components. Unknown whether unused components are excluded from the production bundle. No baseline bundle size measurement exists.

**Acceptance criteria**
- [ ] Run `npx vite-bundle-visualizer` and add screenshot to PR
- [ ] Confirm auto-imported components appear individually (not as full PrimeVue bundle) in the treemap
- [ ] Document current bundle sizes (total JS, largest chunks) in `CODEBASE_REPORT.md`
- [ ] Set size budget in `vite.config.ts` (`build.chunkSizeWarningLimit`) — fail CI if exceeded

---

### [P2-FE-09] Route-level code splitting audit and optimization

**Labels:** `performance`, `P2`, `frontend`  
**Estimate:** 1 day

**Problem**  
Route components may be statically imported. If so, all views load on initial paint — unnecessary code for unauthenticated users hitting `/auth`.

**Files affected**
- `frontend/src/router/index.ts`

**Acceptance criteria**
- [ ] All route components use dynamic `import()` (lazy loading)
- [ ] Verify in browser Network tab: only the current route's chunk loads on navigation
- [ ] Auth views (`LoginForm`, `RegisterForm`, etc.) are in a separate named chunk from main app
- [ ] Settings views lazy-loaded (rarely accessed)
- [ ] No regression in navigation timing (< 200ms chunk load on fast 3G)

---

### [P2-FE-10] Replace global loading spinner with per-request loading states

**Labels:** `ux`, `P2`, `frontend`  
**Estimate:** 3 days

**Problem**  
`loading.store.ts` is a single boolean. All requests share one spinner. Cannot show independent loading states for concurrent requests (e.g. sidebar loading while main content loads). UX is also confusing — any background request triggers the global spinner.

**Files affected**
- `frontend/src/stores/loading.store.ts`
- `frontend/src/composables/useAsyncState.ts`
- `frontend/src/composables/useAsyncAction.ts`
- All components currently reading `loadingStore.isLoading`

**Proposed approach**  
Each `useAsyncState` / `useAsyncAction` call owns its own `isLoading` ref (already the case). Remove global spinner entirely or demote it to explicit opt-in (`{ globalSpinner: true }` option). Components render their own skeleton/spinner using local `isLoading`.

**Acceptance criteria**
- [ ] `loading.store` global spinner removed or restricted to explicit opt-in
- [ ] Each view/component using `useAsyncState` shows its own loading skeleton
- [ ] Concurrent requests (e.g. profile + shrine fetch on a page) show independent loading states
- [ ] `loadingStore` usage audited — no component reads it for conditional rendering unless intentional
- [ ] Visual regression: no spinner flicker on navigation

---

### [P2-FE-11] Accessibility audit — forms and interactive elements

**Labels:** `accessibility`, `P2`, `frontend`  
**Estimate:** 2 days

**Problem**  
No accessibility testing has been done. Auth forms use PrimeVue `FloatLabel` wrappers but ARIA attributes, keyboard navigation, and screen reader announcements are unverified.

**Scope**
- Auth forms: `LoginForm`, `RegisterForm`, `ForgotPasswordForm`
- Navigation: `Menubar` components (desktop + mobile)
- Interactive: `CustomCursor`, `Avatar`, toggle buttons in settings

**Acceptance criteria**
- [ ] All form fields have correct `id` ↔ `label[for]` associations (use `generateFieldIds` util)
- [ ] Error messages linked via `aria-describedby`
- [ ] Submit buttons have descriptive `aria-label` when icon-only
- [ ] Keyboard navigation: tab order logical, no focus traps
- [ ] `CustomCursor` does not interfere with system accessibility cursor settings (respects `prefers-reduced-motion`)
- [ ] Run `axe-core` via Playwright on auth page — zero critical violations
- [ ] Document any intentional deviations from WCAG 2.1 AA

---

### [P2-FE-12] Add loading skeleton states for async data sections

**Labels:** `ux`, `P2`, `frontend`  
**Estimate:** 2 days

**Problem**  
Components using `useAsyncState` show blank content during loading — no skeleton, no placeholder. This creates layout shift and poor perceived performance.

**Components to update**
- Shrine search results (main content area)
- User profile section
- Featured shrines
- Navigation user avatar (while `auth.store` initializes)

**Acceptance criteria**
- [ ] Each component checks `isLoading` and renders a PrimeVue `Skeleton` placeholder matching the content shape
- [ ] Skeleton dimensions match real content to prevent layout shift (CLS < 0.1)
- [ ] No flash of empty content on initial load
- [ ] Tested: skeleton renders while `useAsyncState` is pending; real content renders after resolve
