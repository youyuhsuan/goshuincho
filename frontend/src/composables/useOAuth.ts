import { ref, readonly } from "vue";
// Composables
import useApiUser from "@/composables/api/useApiUser";
// Config
import { API_ENDPOINTS } from "@/config/apiConfig";

const { getGoogleOAuth } = useApiUser();

const useOAUth = () => {
  const isLoading = ref<boolean>(false);

  // Sign in with google
  const signInWithGoogle = async (): Promise<void> => {
    const existingUser = localStorage.getItem("user_data");
    if (existingUser) {
      console.log("Already logged in");
      return;
    }

    isLoading.value = true;

    try {
      sessionStorage.setItem("return_url", window.location.pathname);
      window.location.href = (await getGoogleOAuth()).data;
    } catch (error: unknown) {
      console.error("OAuth initialization error:", error);
    } finally {
      isLoading.value = false;
    }
  };

  return {
    isLoading: readonly(isLoading),
    signInWithGoogle,
  };
};

export default useOAUth;
