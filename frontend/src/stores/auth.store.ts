import { ref, readonly } from "vue";
// Pinia
import { defineStore } from "pinia";
// Router
import router from "@/router";
// Composables
import useApiAuth from "@/composables/api/useApiAuth";
import useApiOAuth from "@/composables/api/useApiOAuth";
// Type
import type { User } from "@/types/userType";
import type { LoginRequest, TokenResponse } from "@/types/authType";

const useAuthStore = defineStore(
  "auth",
  () => {
    const user = ref<User | null>(null);
    const isAuthenticated = ref<boolean>(false);
    const accessToken = ref<string | null>(null);
    const refreshToken = ref<string | null>(null);

    // Controls global loading state during auth initiallization
    const isLoading = ref<boolean>(false);

    const {
      getCurrentAuth,
      loginUser,
      logoutUser,
      refreshAccessToken: refreshAccessTokenApi,
    } = useApiAuth();
    const { createGoogleAuthorization, createGoogleToken } = useApiOAuth();

    let refreshAccessIntervalId: ReturnType<typeof setInterval> | null = null;
    const ACCESS_TOKEN_EXPIRY = 10 * 1000; // 14.5 * 60 * 1000ms = 14.5 minutes

    // Check access token on app startup
    const startIntervalTimer = () => {
      console.log("[Auth] startIntervalTimer", new Date().toLocaleTimeString());

      if (refreshAccessIntervalId) cancelIntervalTimer();

      refreshAccessIntervalId = setInterval(async () => {
        try {
          await refreshAccessToken();
        } catch {
          await logout();
        }
      }, ACCESS_TOKEN_EXPIRY);
    };

    const cancelIntervalTimer = () => {
      if (!refreshAccessIntervalId) return;

      clearInterval(refreshAccessIntervalId);
      refreshAccessIntervalId = null;
    };

    let idleTimerId: ReturnType<typeof setTimeout> | null = null;
    const RESET_TIME_EXPIRY = 15 * 1000; // 15 * 60 * 1000ms = 15 minutes

    const resetTimer = () => {
      if (idleTimerId) cancelResetTimer();

      if (!refreshAccessIntervalId) startIntervalTimer();

      idleTimerId = setTimeout(() => {
        cancelIntervalTimer();
      }, RESET_TIME_EXPIRY);
    };

    const cancelResetTimer = () => {
      if (!idleTimerId) return;

      clearTimeout(idleTimerId);
      idleTimerId = null;
    };

    // User Actions
    const getUser = async () => {
      user.value = (await getCurrentAuth()).data;
    };

    // Auth Auctions
    // Sets accessToken and refreshToken values in localStorage
    const setAuthState = (token: TokenResponse) => {
      accessToken.value = token.accessToken;
      refreshToken.value = token.refreshToken;
      isAuthenticated.value = true;
    };

    // Authenticated user with email, password and remenberMe
    const login = async (values: LoginRequest) => {
      setAuthState((await loginUser(values)).data);
      await getUser();
    };

    // Revoke token on the server, clear local state and redirect to home
    const logout = async () => {
      await logoutUser();

      // Reset all auth state
      isAuthenticated.value = false;
      accessToken.value = null;
      refreshToken.value = null;
      user.value = null;

      // Clear persisted tokens from localStorage
      localStorage.removeItem("auth");

      router.push("/");
    };

    // Restore authenticated state on app startup using persisted tokens
    const initialize = async () => {
      isLoading.value = true;
      try {
        if (!accessToken.value || !refreshToken.value) {
          isLoading.value = false;
          return;
        }

        // Verity token verify
        await getUser();
        isAuthenticated.value = true;
      } finally {
        isLoading.value = false;
      }
    };

    // Exchange refreshToken for a new access token and updated stored tokens
    const refreshAccessToken = async () => {
      setAuthState((await refreshAccessTokenApi()).data);
    };

    // OAuth Auctions
    const initiateGoogleLogin = async () => {
      window.location.href = (await createGoogleAuthorization()).data;
    };

    const processGoogleCallback = async (code: string, state: string) => {
      setAuthState((await createGoogleToken(code, state)).data);
      await getUser();
    };

    return {
      user,
      isAuthenticated: readonly(isAuthenticated),
      isLoading: readonly(isLoading),
      accessToken,
      refreshToken,
      startIntervalTimer,
      cancelIntervalTimer,
      resetTimer,
      cancelResetTimer,
      refreshAccessToken,
      login,
      logout,
      initialize,
      google: {
        initiateGoogleLogin,
        processGoogleCallback,
      },
    };
  },
  {
    persist: {
      key: "auth",
      pick: ["accessToken", "refreshToken"],
    },
  },
);

export default useAuthStore;
