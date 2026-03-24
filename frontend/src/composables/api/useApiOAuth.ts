// Composables
import { instance, authInstance } from "@/composables/api/useApi";
// Config
import { API_ENDPOINTS } from "@/config/apiConfig";
// Types
import type { TokenResponse } from "@/types/authType";

const useApiOAuth = () => {
  const createGoogleAuthorization = () =>
    instance.post<string>(API_ENDPOINTS.OAUTH.AUTHORIZATIONS, {
      provider: "google",
    });

  const createGoogleToken = (code: string, state: string) =>
    authInstance.post<TokenResponse>(API_ENDPOINTS.OAUTH.TOKENS, {
      code,
      state,
      provider: "google",
    });

  return {
    createGoogleAuthorization,
    createGoogleToken,
  };
};

export default useApiOAuth;
