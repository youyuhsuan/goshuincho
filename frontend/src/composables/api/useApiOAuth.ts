// Composables
import { instance, authInstance } from "@/composables/api/useApi";
// Config
import { API_ENDPOINTS } from "@/config/apiConfig";
// Types
import type { TokenResponse } from "@/types/authType";

const useApiOAuth = () => {
  const createGoogleAuthorization = (state: string) =>
    instance.post<string>(API_ENDPOINTS.OAUTH.AUTHORIZATIONS, {
      provider: "google",
      state,
    });

  const createGoogleToken = (code: string) =>
    authInstance.post<TokenResponse>(API_ENDPOINTS.OAUTH.TOKENS, {
      code,
      provider: "google",
    });

  return {
    createGoogleAuthorization,
    createGoogleToken,
  };
};

export default useApiOAuth;
