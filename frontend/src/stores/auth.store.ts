import { defineStore } from "pinia";
import { ref, readonly } from "vue";
import { useRoute } from "vue-router";
// Router
import router from "@/router";
// Composables
import useApiUser from "@/composables/api/useApiUser";
import useApiAuth from "@/composables/api/useApiAuth";
// Type
import type { LoginRequest, User } from "@/types/userType";

const useAuthStore = defineStore("auth", () => {
  const user = ref<User | null>(null);
  const isAuthenticated = ref<boolean>(false);

  const { loginUser, getSession, logoutUser, refreshSession } = useApiUser();
  const { createGoogleAuthorization, createGoogleToken } = useApiAuth();

  let refreshInterval: ReturnType<typeof setInterval> | null = null;
  const ACCESS_TOKEN_EXPITY = 4.5 * 60 * 1000; // 4.5 minutes

  // Check session on app startup
  const startInactivityTimer = () => {
    refreshInterval = setInterval(async () => {
      try {
        await refreshSession();
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

  // Check session on app startup
  const checkSession = async () => {
    user.value = (await getSession()).data;
    isAuthenticated.value = true;
    startInactivityTimer();
  };

  const login = async (values: LoginRequest) => {
    await loginUser(values);
    isAuthenticated.value = true;
    startInactivityTimer();
  };

  const logout = async () => {
    await logoutUser();
    isAuthenticated.value = false;
    stopInactivityTimer();
    router.push("/");
  };

  const initiateGoogleLogin = async () =>
    (window.location.href = (await createGoogleAuthorization()).data);

  const processGoogleCallback = async (code: string, state: string) => {
    await createGoogleToken(code, state);
    isAuthenticated.value = true;
  };

  return {
    user,
    isAuthenticated: readonly(isAuthenticated),
    login,
    logout,
    checkSession,
    stopInactivityTimer,
    google: {
      initiateGoogleLogin,
      processGoogleCallback,
    },
  };
});

export default useAuthStore;
