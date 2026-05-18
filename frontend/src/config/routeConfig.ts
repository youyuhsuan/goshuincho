const ROUTE_CONFIGS = {
  HOME: "/",
  ABOUT: "/about",
  AUTH: "/auth",
  AUTH_LOGIN: "/auth/login",
  AUTH_REGISTER: "/auth/register",
  AUTH_FORGOT_PASSWORD: "/auth/forgot-password",
  OAUTH: "/oauth/callback",
  SETTING: "/settings",
  SHRINES: "/shrines",
} as const;

export default ROUTE_CONFIGS;
