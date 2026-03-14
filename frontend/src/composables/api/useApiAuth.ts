// Composables
import { instance, authInstance } from "@/composables/api/useApi";
// Config
import { API_ENDPOINTS } from "@/config/apiConfig";

const useApiAuth = () => {
  const createGoogleAuthorization = () =>
    instance.post<string>(API_ENDPOINTS.OAUTH.AUTHORIZATIONS, {
      provider: "google",
    });

  const createGoogleToken = (code: string, state: string) =>
    authInstance.post(API_ENDPOINTS.OAUTH.TOKENS, {
      code,
      state,
      provider: "google",
    });

  return {
    // Token management
    createGoogleAuthorization,
    createGoogleToken,
  };
};

export default useApiAuth;
