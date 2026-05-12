# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Designare is a full-stack web app with a **Vue 3 frontend** (project root) and an **ASP.NET Core (.NET 10) backend** (`/backend`).

## Commands

All frontend commands run from the project root:

```sh
npm run dev          # Start Vite dev server (http://localhost:5173)
npm run build        # Type-check + build for production
npm run type-check   # Run vue-tsc only
npm run lint         # Run oxlint then eslint (both with --fix)
npm run test:unit    # Run Vitest unit tests
npm run test:e2e     # Run Playwright e2e tests (build first on CI)
```

Run a single unit test file:
```sh
npm run test:unit -- src/components/__tests__/HelloWorld.spec.ts
```

Backend (from `/backend`):
```sh
dotnet run           # Starts API on http://localhost:5286
```

Swagger UI available at `http://localhost:5286/swagger` in development.

## Architecture

### Frontend (`frontend/src/`)

**`@` alias** resolves to `frontend/src/` (configured in `vite.config.ts`).

**PrimeVue components are auto-imported** via `unplugin-vue-components` — no explicit imports needed in `.vue` files. SVGs imported as Vue components via `vite-svg-loader`.

**Layer structure:**
- `views/` — route-level page components
- `components/` — reusable UI components (subdirectories group by feature)
- `composables/api/` — one composable per API resource (`useApiAuth`, `useApiUser`, `useApiShrines`, etc.)
- `stores/` — two Pinia stores: `auth.store.ts` and `setting.store.ts`
- `config/` — `apiConfig.ts` (endpoints + base URL), `routeConfig.ts` (route paths), `locales/` (i18n JSON)
- `types/` — TypeScript interfaces per domain
- `utils/` — pure utility functions
- `directives/` — custom Vue directives

**Async pattern:** Use `useAsyncState()` composable from `composables/useAsyncState.ts` to wrap API calls — it provides reactive `data`, `isLoading`, `error`, and an `execute` function, eliminating manual try/catch/finally.

**API layer** (`composables/api/useApi.ts`):
- `instance` — unauthenticated Axios instance (public routes)
- `authInstance` — authenticated instance that attaches the Bearer token, handles 401s by silently refreshing the access token, and queues concurrent failed requests

**Pinia stores with persistence:**
- `auth.store` — persists `accessToken` + `refreshToken` to `localStorage["auth"]`. Handles login, logout, token refresh (idle timer fires after 15 min inactivity), Google OAuth flow, and app initialization from persisted tokens.
- `setting.store` — persists `userTheme` to `localStorage["theme"]` and `currentLanguage` to `localStorage["locale"]`. Unauthenticated users follow the OS `prefers-color-scheme`.

**Dark mode** is toggled by adding/removing the `.app-dark` class on `<html>`. The PrimeVue theme is configured with `darkModeSelector: ".app-dark"`.

**Custom directive `v-cursor-hover`** — registered globally as `v-cursor-hover`. Scales the custom cursor on hover by writing to `settingStore.cursor.size`. Accepts an optional binding value for the hover size.

**i18n** — `vue-i18n` v11, `legacy: false`. Locale files at `config/locales/en.json` and `config/locales/zh.json`. Initial locale is restored from `localStorage["locale"]`.

**Route metadata conventions:**
- `meta: { requiresAuth: true }` — redirects unauthenticated users to `/auth`
- `meta: { fullscreen: true }` — signals to `App.vue` that the nav/layout should be hidden

### Backend (`backend/`)

ASP.NET Core Web API (.NET 10) following a controller → service → repository pattern.

**Auth:** JWT Bearer tokens signed with **RSA keys** (`private_key.pem` / `public_key.pem`). The access token is read from the `access_token` **httpOnly cookie** (not the Authorization header) — configured in `JwtBearerEvents.OnMessageReceived` in `Program.cs`.

**Services registered for DI:**
| Interface | Implementation |
|---|---|
| `IUserService` | `UserService` |
| `IJwtTokenGenerator` | `JwtTokenGenerator` |
| `ITokenBlacklistService` | `TokenBlacklistService` |
| `ITokenRepository` | `TokenRepository` |
| `IOAuthService` | `OAuthService` (singleton) |
| `ICookieService` | `CookieService` |
| `IStorageService` | `AzureBlobStorageService` |
| `IShrineService` | `ShrineService` |

**Database:** SQL Server via Entity Framework Core. Connection string in `appsettings.Development.json` (not committed). On startup in development, `context.Database.EnsureCreated()` is called.

**Logging:** Serilog writes to console + rolling daily log files in `backend/Logs/`. Error-only sink retains 30 days.

**CORS** allows origins: `localhost:5173`, `localhost:3000`, `localhost:8080`, `localhost:5286` with credentials.

**API base path:** `/api/{resource}` — see `frontend/src/config/apiConfig.ts` for the full endpoint map.
