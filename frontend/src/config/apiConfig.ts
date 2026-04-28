export const API_CONFIG = {
  BASE_URL: import.meta.env.VITE_BASE_URL,
  TIMEOUT: 10000,
} as const;

export const API_ENDPOINTS = {
  // User Resource
  USER: "/api/users",
  // GET: /:id: Get specific user
  // PUT: /:id: Update user
  // DELETE: /:id: Delete user

  // Auth Resource
  AUTH: {
    ME: "/api/auth/me",
    // GET: Get current authenticated user info
    REGISTER: "/api/auth/register",
    // POST: Register a new user account
    LOGIN: "/api/auth/login",
    // POST: Authenticate user and issue tokens
    LOGOUT: "/api/auth/logout",
    // POST: Logout current user
    REFRESH: "/api/auth/refresh",
    // POST: Issue a new Access Token using a valid Refresh Token
  },

  // OAuth Resource
  OAUTH: {
    AUTHORIZATIONS: "/api/oauth/authorizations",
    // POST: Create authorization (get authorization code)
    TOKENS: "/api/oauth/tokens",
    // POST: Exchange authorization code for access token
  },
  SHRINES: {
    BASE: "/api/shrines",
    // GET: /:id: Get specific shrine
    SUGGESTIONS: "/api/shrines/suggestions",
    // GET: Get shrine suggestions based on query
    FEATURED: "/api/shrines/featured",
    // GET: Get featured shrines for homepage
  },
} as const;
