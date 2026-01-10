import type { AxiosResponse } from "axios";
// Composables
import instance from "@/composables/api/useApi";

const useApiAuth = () => {
  // https://developers.google.com/identity/protocols/oauth2/native-app#step1-code-verifier
  // Token management
  const exchangeCodeForToken = (
    authorizationCode: string,
    codeVerifier: string
  ): Promise<AxiosResponse<any, unknown>> => {
    const data = new URLSearchParams({
      code: authorizationCode,
      client_id: import.meta.env.VITE_OAUTH_CLIENT_ID,
      code_verifier: codeVerifier,
      grant_type: "authorization_code",
      redirect_uri: import.meta.env.VITE_OAUTH_REDIRECT_URI,
    });

    return instance.post(import.meta.env.VITE_OAUTH_TOKEN_URI, data, {
      headers: {
        "Content-Type": "application/x-www-form-urlencoded",
      },
    });
  };

  const validateToken = (accessToken: string) =>
    instance.get("https://www.googleapis.com/oauth2/v2/userinfo", {
      headers: {
        Authorization: `Bearer ${accessToken}`,
      },
    });

  return {
    // Token management
    exchangeCodeForToken,
    validateToken,
  };
};

export default useApiAuth;
