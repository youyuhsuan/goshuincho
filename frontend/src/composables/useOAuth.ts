import { ref, readonly } from "vue";
import { useRoute } from "vue-router";
// Composables
import useApiAuth from "@/composables/api/useApiAuth";

const { createGoogleAuthorization, createGoogleToken } = useApiAuth();

const useOAUth = () => {
  const isLoading = ref<boolean>(false);
  const route = useRoute();

  const initiateGoogleLogin = async (): Promise<void> => {
    const existingUser = localStorage.getItem("user_data");
    if (existingUser) {
      console.log("Already logged in");
      return;
    }

    isLoading.value = true;

    try {
      sessionStorage.setItem("return_url", window.location.pathname);
      window.location.href = (await createGoogleAuthorization()).data;
    } catch (error: unknown) {
      console.error("OAuth initialization error:", error);
    } finally {
      isLoading.value = false;
    }
  };

  const processGoogleCallback = async () => {
    isLoading.value = true;

    try {
      // Extract parameters from URL
      const code = route.query.code as string;
      const state = route.query.state as string;
      const errorParam = route.query.error as string;

      // Handle OAuth errors
      if (errorParam) {
        const errorDescription = route.query.error_description as string;
        throw new Error(`OAuth Error: ${errorDescription || errorParam}`);
      }

      // Validate required parameters
      if (!code || !state) {
        throw new Error("Missing authorization code or state parameter");
      }

      const response = await createGoogleToken(code, state);
      localStorage.setItem("user_data", JSON.stringify(response));
    } catch (error: unknown) {
      console.error("OAuth callback error:", error);
    } finally {
      isLoading.value = false;
    }
  };

  return {
    isLoading: readonly(isLoading),
    initiateGoogleLogin,
    processGoogleCallback,
  };
};

export default useOAUth;
