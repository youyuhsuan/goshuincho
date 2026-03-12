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
  const route = useRoute();

  const { loginUser, getSession, logoutUser } = useApiUser();
  const { createGoogleAuthorization, createGoogleToken } = useApiAuth();

  const login = async (values: LoginRequest) => {
    await loginUser(values);
    isAuthenticated.value = true;
  };

  const logout = async () => {
    await logoutUser();
    isAuthenticated.value = false;
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
    google: {
      initiateGoogleLogin,
      processGoogleCallback,
    },
  };
});

export default useAuthStore;
