import { ref, readonly, computed } from "vue";
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

    const {
      getCurrentAuth,
      loginUser,
      logoutUser,
      refreshAccessToken: refreshAccessTokenApi,
    } = useApiAuth();
    const { createGoogleAuthorization, createGoogleToken } = useApiOAuth();

    let refreshInterval: ReturnType<typeof setInterval> | null = null;
    const ACCESS_TOKEN_EXPITY = 4.5 * 60 * 1000; // 4.5 minutes

    // Check session on app startup
    const startInactivityTimer = () => {
      refreshInterval = setInterval(async () => {
        try {
          await refreshAccessToken();
        } catch {
          await logout();
        }
      }, ACCESS_TOKEN_EXPITY);
    };

    // Stop the timer when user logs out or when the component is onBeforeUnmount
    const stopInactivityTimer = () => {
      if (refreshInterval) {
        clearInterval(refreshInterval);
        refreshInterval = null;
      }
    };

    // Authenticated user with email, password and remenberMe
    const login = async (values: LoginRequest) => {
      const {
        accessToken: accessTokenData,
        refreshToken: refreshTokenData,
      }: TokenResponse = (await loginUser(values)).data;

      // Store token in localStorage on success
      accessToken.value = accessTokenData;
      refreshToken.value = refreshTokenData;

      user.value = (await getCurrentAuth()).data;
      isAuthenticated.value = true;
    };

    // Revoke token on the server
    const logout = async () => {
      await logoutUser();

      // Reset all auth state
      isAuthenticated.value = false;
      accessToken.value = null;
      refreshToken.value = null;
      user.value = null;

      stopInactivityTimer();
      router.push("/");
    };

    // Restore authenticated store in initialize amount
    const initialize = async () => {
      if (!accessToken.value || !refreshToken.value) return;

      user.value = (await getCurrentAuth()).data;
      isAuthenticated.value = true;
    };

    const refreshAccessToken = async () => {
      const {
        accessToken: accessTokenData,
        refreshToken: refreshTokenData,
      }: TokenResponse = (await refreshAccessTokenApi()).data;

      // Store token in localStorage on success
      accessToken.value = accessTokenData;
      refreshToken.value = refreshTokenData;
    };

    // OAuth
    const initiateGoogleLogin = async () =>
      (window.location.href = (await createGoogleAuthorization()).data);

    const processGoogleCallback = async (code: string, state: string) => {
      await createGoogleToken(code, state);
      isAuthenticated.value = true;
    };

    return {
      user,
      isAuthenticated: readonly(isAuthenticated),
      accessToken,
      refreshToken,
      refreshAccessToken,
      login,
      logout,
      initialize,
      stopInactivityTimer,
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
