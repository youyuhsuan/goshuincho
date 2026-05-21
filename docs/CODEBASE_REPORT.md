# Codebase Maturity Report — Designare

**Date:** 2026-05-20  
**Branch assessed:** `feature-authentication`  
**Stack:** Vue 3 + TypeScript (frontend) · ASP.NET Core 10 + C# (backend)  
**Total LOC:** ~8,036 (frontend ~5,007 · backend ~3,029)

---

## Summary

Designare is a well-structured early-stage project with solid architectural foundations but significant production-readiness gaps. The development team demonstrates good instincts — typed end-to-end, clean separation of concerns, JWT with refresh rotation — but the project lacks the safety nets (tests, CI/CD, rate limiting) required before public deployment.

**Overall Maturity: 5.3 / 10**

---

## 1. Architecture Quality — 7/10

### Strengths
- Clear layer separation: `views → composables/API → stores → router`
- Backend follows controller → service → repository with interface-based DI
- Two Axios instances (`instance` / `authInstance`) cleanly split public vs. authenticated calls
- Token refresh queue prevents concurrent 401 races
- Pinia stores have clear, non-overlapping responsibilities (auth, loading, settings)
- Custom exception types map to HTTP semantics via `GlobalErrorHandlingMiddleware`

### Weaknesses
- No API versioning strategy (`/api/v1/` missing)
- No health check endpoint
- `AzureBlobStorageService` is stubbed — storage is non-functional
- `FavoriteGoods` serialized as JSON string in SQL Server (schema smell)
- Shrine geo-search loads all candidates into memory before filtering (unbounded at scale)
- No circuit breaker or retry logic for external services (Resend, Azure Blob)

---

## 2. Code Organization — 7/10

### Frontend (`frontend/src/`)

| Directory | Purpose | Quality |
|---|---|---|
| `views/` | Route-level pages | Good |
| `components/` | Feature-grouped UI | Good |
| `composables/api/` | One composable per resource | Good |
| `stores/` | Pinia state (auth, loading, setting) | Good |
| `config/` | Endpoints, routes, i18n | Good |
| `types/` | Domain interfaces | Good |
| `schemas/` | Zod validation | Incomplete — only `authSchemas.ts` |
| `utils/` | Pure helpers | Good |
| `directives/` | Custom Vue directives | Good |

**Gaps:**
- Zod schemas exist only for auth; user/shrine/setting schemas missing
- No component error boundaries
- No loading skeleton states
- Global loading spinner cannot express per-request granularity

### Backend (`backend/`)

| Directory | Purpose | Quality |
|---|---|---|
| `Controllers/` | HTTP surface, thin | Good |
| `Services/` | Business logic, interface-backed | Good |
| `Models/` | EF Core entities | Good |
| `DTOs/` | Request/response contracts | Good |
| `Data/` | DbContext | Good |
| `Middleware/` | Error handling | Good |
| `Migrations/` | Schema history | Minimal (1 migration) |

**Gaps:**
- No audit fields (`UpdatedAt`, `DeletedAt`) on entities
- No soft delete pattern
- No request ID correlation between logs
- No performance metrics (query durations)

---

## 3. Test Coverage — 2/10

| Layer | Unit | Integration | E2E | Coverage |
|---|---|---|---|---|
| Frontend stores | ~154 LOC (auth, loading) | — | — | ~3% |
| Frontend components | None | — | — | 0% |
| Frontend composables | None | — | — | 0% |
| Frontend API layer | None | — | — | 0% |
| Backend controllers | None | — | None | 0% |
| Backend services | None | — | None | 0% |
| E2E (Playwright) | — | — | 1 trivial test | ~0% |

**Critical untested paths:**
- Login / logout / token refresh flow
- Password reset (forgot → email → reset)
- OAuth (Google authorization → token exchange)
- Form validation (client-side Zod + server-side error display)
- 401 → silent refresh → retry queue
- Concurrent requests during token refresh

---

## 4. Scalability Concerns

### High Impact
1. **Shrine geo-search loads all matching records into memory** — `ShrineService` fetches candidates via EF then filters in C#. Will degrade with >50k shrines. Fix: push distance calculation to SQL (use a computed column or spatial index).
2. **No database connection pooling configuration** — EF Core default pooling; not tuned for concurrent load.
3. **Token blacklist grows unbounded** — `TokenBlacklist` table has no scheduled cleanup. Expired JTIs accumulate. Fix: background job to purge expired entries.
4. **Global loading spinner** — single boolean in `loading.store`; impossible to show concurrent request states in UI.

### Medium Impact
5. **No request caching** — every API call hits the server; no client-side TTL cache for stable data (shrine suggestions, user profile).
6. **No request deduplication** — identical concurrent requests each fire independently.
7. **Single migration** — schema changes require careful migration authoring with zero downtime strategy undefined.

---

## 5. Technical Debt

### Security Debt
| Issue | Risk | Effort |
|---|---|---|
| No rate limiting on `/login`, `/register`, `/forgot-password` | High — brute force | Low |
| No account lockout after N failed logins | High — credential stuffing | Low |
| `access_token` cookie missing explicit `HttpOnly`/`Secure` flags in code | Medium | Low |
| No CSRF mitigation (relies on `SameSite` only) | Medium | Medium |
| OAuth `state` param not validated server-side | Medium — CSRF on OAuth | Low |
| No input sanitization middleware | Medium — stored XSS risk | Low |
| Password complexity enforced client-side only | Low | Low |

### Functional Debt
| Issue | Impact |
|---|---|
| `AzureBlobStorageService` stubbed | Profile picture upload non-functional |
| Email template hardcoded in Chinese | Localization blocked |
| Shrine search scales poorly | Performance cliff at volume |
| `FavoriteGoods` as JSON string | Querying/filtering impossible in SQL |
| No `UpdatedAt` / `DeletedAt` on entities | Audit trail impossible |

### Frontend Debt
| Issue | Impact |
|---|---|
| Zod schemas only for auth | Copy-paste validation risk in other forms |
| No error boundary components | Uncaught render errors crash full page |
| `useAsyncState` used for side effects too | Semantic confusion, harder to test |
| No optimistic updates | UI feels slow on mutations |

---

## 6. Recommendations

### P0 — Before any public traffic

1. **Rate limiting** — add ASP.NET rate limiting middleware to `/api/auth/*` endpoints. 5 req/min per IP on login/reset, 3 req/min on register.
2. **Account lockout** — lock account for 15 min after 5 consecutive failed logins. Expose `X-RateLimit-*` headers.
3. **Cookie hardening** — verify `HttpOnly`, `Secure`, `SameSite=Strict` on both `access_token` and `refresh_token` cookies in `CookieService`.
4. **Auth flow tests** — write Playwright e2e tests covering register → login → refresh → logout and password reset happy path.
5. **Fix blob storage** — `AzureBlobStorageService` must be functional before profile picture upload ships.

### P1 — Within next sprint

6. **CI pipeline** — GitHub Actions: `npm run lint && npm run type-check && npm run test:unit` on every PR; `dotnet test` when added.
7. **Backend unit tests** — test `UserService`, `JwtTokenGenerator`, `TokenBlacklistService` (the three most security-critical services).
8. **Shrine geo-search in SQL** — replace in-memory Haversine with SQL spatial query or raw SQL distance formula.
9. **Token blacklist cleanup** — background `IHostedService` to purge expired JTIs nightly.
10. **OAuth state validation** — validate `state` parameter server-side in `OAuthService`.

### P2 — Hardening pass

11. **Extend Zod schemas** — add user and settings schemas; enforce at form boundaries.
12. **Error boundary components** — wrap async-loaded route sections.
13. **Audit fields** — add `UpdatedAt` (auto-set via EF `SaveChanges` override) and soft-delete `DeletedAt` to `User`.
14. **Health check** — `app.MapHealthChecks("/health")` with DB ping.
15. **API versioning** — prefix routes `/api/v1/`; plan migration path for v2.
16. **Request ID** — inject `X-Request-Id` header; thread through Serilog context.

### P3 — Long-term

17. Docker Compose for local dev (frontend + backend + SQL Server).
18. Storybook for component documentation.
19. `FavoriteGoods` normalization to a proper join table.
20. Bundle analysis and code splitting audit.

---

## Maturity Scorecard

| Category | Score | Key Gap |
|---|---|---|
| Architecture | 7/10 | No versioning, stubbed storage |
| Type safety | 9/10 | Near-complete |
| Error handling | 6/10 | No retry/circuit breaker |
| Validation | 6/10 | Incomplete schema coverage |
| **Testing** | **2/10** | **Critical — almost nothing tested** |
| **Security** | **5/10** | **No rate limiting or lockout** |
| Logging | 5/10 | No request IDs, no perf metrics |
| Documentation | 4/10 | Swagger only |
| **CI/CD** | **1/10** | **None** |
| Code quality | 7/10 | Clean, minor duplication |

**Overall: 5.3 / 10 — Not production-ready. Good foundation.**
