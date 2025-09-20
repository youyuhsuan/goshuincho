export const API_CONFIG = {
  BASE_URL: import.meta.env.VITE_BASE_URL,
  TIMEOUT: 10000,
} as const;

export const API_ENDPOINTS = {
  // User Resource
  USERS: "/api/users",
  // POST: Create user (register)
  // GET: Get user list
  // GET /:id: Get specific user
  // PUT /:id: Update user
  // DELETE /:id: Delete user

  // Session Resource
  SESSIONS: "/api/sessions",
  // POST: Create session (login)
  // DELETE /:id: Delete session (logout)
  // GET: Get current session
  // PUT /:id: Update session (refresh token)

  // Token Resource
  TOKENS: "/api/tokens",
  // POST: Create new token (refresh)
  // DELETE /:id: Revoke token
  // GET /:id: Validate token

  // OAuth Resource
  OAUTH: {
    AUTHORIZATIONS: "/api/oauth/authorizations",
    // POST: Create authorization (get authorization code)
    TOKENS: "/api/oauth/tokens",
    // POST: Exchange authorization code for access token
    REVOCATIONS: "/api/oauth/revocations",
    // POST: Revoke access token
  },
} as const;
