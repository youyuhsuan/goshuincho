# GitHub Issues — Designare Codebase

Copy each block into GitHub → Issues → New Issue, or use the GitHub CLI:
`gh issue create --title "..." --body "..." --label "..."`

---

## P0 — Critical (block production)

### Issue 1: Add rate limiting to auth endpoints
**Labels:** `security`, `P0`, `backend`

No rate limiting on `/api/auth/login`, `/api/auth/register`, `/api/auth/forgot-password`. Vulnerable to brute force and credential stuffing.

**Acceptance criteria:**
- [ ] Login: max 5 req/min per IP
- [ ] Register: max 3 req/min per IP  
- [ ] Forgot-password: max 3 req/min per IP
- [ ] Returns `429 Too Many Requests` with `Retry-After` header
- [ ] Use ASP.NET Core built-in rate limiting middleware (`AddRateLimiter`)

---

### Issue 2: Add account lockout after failed login attempts
**Labels:** `security`, `P0`, `backend`

No lockout mechanism. Attacker can attempt unlimited passwords against a known email.

**Acceptance criteria:**
- [ ] Lock account for 15 min after 5 consecutive failed logins
- [ ] Return 423 or 401 with clear message (no email enumeration)
- [ ] Track failed attempts in DB (add `FailedLoginCount`, `LockoutUntil` to `User`)
- [ ] Reset counter on successful login

---

### Issue 3: Harden auth cookie flags
**Labels:** `security`, `P0`, `backend`

Verify `access_token` and `refresh_token` cookies in `CookieService` explicitly set `HttpOnly = true`, `Secure = true`, `SameSite = Strict`. Currently not confirmed in code.

**Acceptance criteria:**
- [ ] Both cookies have `HttpOnly`, `Secure`, `SameSite=Strict` in `CookieService`
- [ ] Verified in browser DevTools (Application → Cookies)

---

### Issue 4: Validate OAuth state parameter server-side
**Labels:** `security`, `P0`, `backend`

OAuth `state` parameter is not validated in `OAuthService`. Vulnerable to CSRF on OAuth callback.

**Acceptance criteria:**
- [ ] Generate cryptographically random `state` on `/api/oauth/authorizations`
- [ ] Store in server-side session or signed cookie
- [ ] Validate on `/api/oauth/tokens` — reject mismatches with 400

---

### Issue 5: Fix AzureBlobStorageService stub
**Labels:** `bug`, `P0`, `backend`

`AzureBlobStorageService` returns a placeholder URL. Profile picture upload is non-functional.

**Acceptance criteria:**
- [ ] Connect to real Azure Blob Storage using connection string from user-secrets
- [ ] `UploadAsync` returns a real CDN URL
- [ ] `DeleteAsync` removes the blob
- [ ] Max 5 MB, JPEG/PNG/WebP enforced server-side

---

## P1 — High (next sprint)

### Issue 6: Add CI pipeline (GitHub Actions)
**Labels:** `devops`, `P1`

No CI exists. PRs merge without lint, type-check, or test validation.

**Acceptance criteria:**
- [ ] `frontend-ci.yml`: runs `npm run lint`, `npm run type-check`, `npm run test:unit` on every PR
- [ ] `backend-ci.yml`: runs `dotnet build` and `dotnet test` on every PR
- [ ] Both block merge on failure

---

### Issue 7: Write auth flow E2E tests
**Labels:** `testing`, `P1`, `frontend`

Zero E2E tests cover the authentication flow. The single existing Playwright test only visits the root URL.

**Acceptance criteria:**
- [ ] Register → login → verify authenticated state
- [ ] Login with wrong password → error message displayed
- [ ] Token refresh: simulate expiry, verify silent refresh
- [ ] Logout → redirected to `/auth`
- [ ] Forgot password → reset password happy path

---

### Issue 8: Write backend unit tests for security-critical services
**Labels:** `testing`, `P1`, `backend`

No C# unit tests exist. `UserService`, `JwtTokenGenerator`, and `TokenBlacklistService` are untested.

**Acceptance criteria:**
- [ ] `UserService`: register duplicate email → ConflictException; invalid credentials → null
- [ ] `JwtTokenGenerator`: valid token round-trip; expired token rejected
- [ ] `TokenBlacklistService`: revoked JTI rejected; non-revoked JTI passes
- [ ] `ResetPasswordAsync`: expired token → exception; reuse after use → exception

---

### Issue 9: Fix shrine geo-search memory scaling issue
**Labels:** `performance`, `P1`, `backend`

`ShrineService` fetches all shrine candidates into C# memory and runs Haversine there. Breaks at scale.

**Acceptance criteria:**
- [ ] Distance calculation pushed to SQL (raw SQL with distance formula or spatial index)
- [ ] Query returns pre-filtered, pre-sorted results
- [ ] Verified with `EXPLAIN` / EF query log showing no full-table load

---

### Issue 10: Add token blacklist cleanup job
**Labels:** `backend`, `P1`

`TokenBlacklist` table accumulates expired JTIs indefinitely. No cleanup mechanism.

**Acceptance criteria:**
- [ ] Background `IHostedService` runs nightly
- [ ] Deletes `TokenBlacklist` rows where `ExpiresAt < UTC_NOW`
- [ ] Logs count of purged rows at `Information` level

---

## P2 — Medium (hardening pass)

### Issue 11: Add audit fields to User entity
**Labels:** `backend`, `P2`, `database`

`User` has `CreatedAt` but no `UpdatedAt` or soft-delete `DeletedAt`. Audit trail is incomplete.

**Acceptance criteria:**
- [ ] Add `UpdatedAt` (auto-updated via EF `SaveChanges` override)
- [ ] Add `DeletedAt` nullable (soft delete)
- [ ] `DELETE /api/users/{id}` sets `DeletedAt` instead of hard delete
- [ ] Soft-deleted users excluded from all queries
- [ ] Migration added

---

### Issue 12: Extend Zod validation schemas beyond auth
**Labels:** `frontend`, `P2`

Only `authSchemas.ts` exists. User profile, settings, and search forms lack centralized schemas.

**Acceptance criteria:**
- [ ] `userSchemas.ts` covering profile update fields
- [ ] `shrineSchemas.ts` covering search parameters
- [ ] All forms use schema-based validation (no ad hoc `v-if` rules)

---

### Issue 13: Add Vue error boundary components
**Labels:** `frontend`, `P2`

No error boundary wrapping async-loaded route sections. Render errors crash the full page.

**Acceptance criteria:**
- [ ] `ErrorBoundary.vue` component using `onErrorCaptured`
- [ ] Wraps at least route-level views and async data sections
- [ ] Shows fallback UI with retry option on error

---

### Issue 14: Add health check endpoint
**Labels:** `backend`, `P2`, `devops`

No `/health` endpoint. Load balancers and uptime monitors cannot probe the service.

**Acceptance criteria:**
- [ ] `GET /health` returns 200 with DB connectivity status
- [ ] Uses ASP.NET `MapHealthChecks`
- [ ] Excluded from Swagger and auth middleware

---

### Issue 15: Add request ID correlation to logs
**Labels:** `backend`, `P2`

No `X-Request-Id` threading. Impossible to correlate a frontend error report with a backend log entry.

**Acceptance criteria:**
- [ ] Middleware injects `X-Request-Id` (generate if absent, echo back in response header)
- [ ] Request ID pushed into Serilog `LogContext`
- [ ] All log lines within a request include `RequestId` field

---

## P3 — Low (long-term)

### Issue 16: Normalize FavoriteGoods to join table
**Labels:** `database`, `P3`, `tech-debt`

`User.FavoriteGoods` stored as JSON string in SQL Server. Cannot query or filter in SQL.

**Acceptance criteria:**
- [ ] New `UserFavoriteGoods` table with FK to `Users`
- [ ] Migration with data backfill from JSON column
- [ ] `UserService` updated to use relational queries

---

### Issue 17: Add Docker Compose for local development
**Labels:** `devops`, `P3`

No container setup. Developers must manually configure SQL Server and backend secrets.

**Acceptance criteria:**
- [ ] `docker-compose.yml` spins up SQL Server + backend + frontend
- [ ] `.env.sample` documents all required variables
- [ ] README updated with `docker compose up` quickstart

---

### Issue 18: Add API versioning
**Labels:** `backend`, `P3`

All routes at `/api/` with no version prefix. Breaking changes require coordinated frontend deploys.

**Acceptance criteria:**
- [ ] Routes prefixed `/api/v1/`
- [ ] Version negotiation via URL path (not headers)
- [ ] Old `/api/` routes return 301 or documented deprecation notice

---

### Issue 19: Bundle analysis and lazy loading audit
**Labels:** `frontend`, `P3`, `performance`

No bundle size monitoring. Unknown whether PrimeVue tree-shaking is effective.

**Acceptance criteria:**
- [ ] Run `vite-bundle-visualizer` and document findings
- [ ] Verify PrimeVue auto-import excludes unused components from bundle
- [ ] Route-level code splitting confirmed in network tab

---

### Issue 20: Add Storybook for component documentation
**Labels:** `frontend`, `P3`, `documentation`

No isolated component development or documentation environment.

**Acceptance criteria:**
- [ ] Storybook configured for Vue 3 + PrimeVue
- [ ] Stories for: `LoginForm`, `RegisterForm`, `Avatar`, `CustomCursor`
- [ ] Integrated into CI (visual regression optional)
