import { ref, computed, readonly } from "vue";

// Composables
import useApiAuth from "@/composables/api/useApiAuth";

// Types
import type { StateData, PKCEData } from "@/types/oauthType";

const { exchangeCodeForToken, validateToken } = useApiAuth();

const useOAUth = () => {
  const isLoading = ref<boolean>(false);

  // OAuth config
  // Google's OAuth` endpoint for requesting an access token
  const oauth2GoogleEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
  const STORAGE_KEY = "oauth_pkce";
  const FLOW_TIMEOUT = 10 * 60 * 1000; // 10 min
  const UNRESERVED_CHARS =
    "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~";

  // Generate PKCE (Proof Key for Code Exchange) parameters for OAuth 2.0
  // Compliant with RFC 7636: https://datatracker.ietf.org/doc/html/rfc7636#section-4.1
  const generatePKCE = async (
    length = 64
  ): Promise<{ codeVerifier: string; codeChallenge: string }> => {
    if (length < 43 || length > 128) {
      throw new Error(
        `Code verifier length must be between 43 and 128 characters, got: ${length}`
      );
    }

    try {
      // Create a 32-byte array (256 bits) for high entropy
      // 512 bits = 8 bits/byte × 64 bytes = cryptographic strength
      const array = new Uint8Array(length);

      // Fill array with cryptographically secure random values (0-255 each)
      // Uses OS-level entropy sources for unpredictable randomness
      window.crypto.getRandomValues(array);

      // Generate code verifier using RFC 7636 unreserved characters
      let codeVerifier = "";
      for (let i = 0; i < length; i++) {
        codeVerifier += UNRESERVED_CHARS[array[i] % UNRESERVED_CHARS.length];
      }

      // Encode string as UTF-8 bytes for hashing algorithm
      // SHA-256 creates a one-way hash (cannot reverse to get original)
      const digest = await window.crypto.subtle.digest(
        "SHA-256",
        new TextEncoder().encode(codeVerifier)
      );

      // Convert ArrayBuffer to byte array, then to Base64URL format
      const codeChallenge = btoa(String.fromCharCode(...new Uint8Array(digest)))
        .replace(/[+/]/g, (char: string) => (char === "+" ? "-" : "_"))
        .replace(/=/g, "");

      return { codeVerifier, codeChallenge };
    } catch (error: unknown) {
      throw new Error(`Failed to generate PKCE parameters: ${error}`);
    }
  };

  // Generate state parameters for CSRF protection
  const generateState = (): StateData => ({
    uuid: window.crypto.randomUUID(),
    timestamp: Date.now(),
    return_url: window.location.pathname,
  });

  // https://github.com/soofstad/react-oauth2-pkce/issues/139

  // Stores PKCE (Proof Key for Code Exchange) data in localStorage
  const storePKCEData = async (
    state: string,
    codeVerifier: string
  ): Promise<string | undefined> => {
    try {
      const pkceData: PKCEData = {
        state: state,
        codeVerifier: codeVerifier,
        timestamp: Date.now(),
        expiresAt: Date.now() + FLOW_TIMEOUT,
        inProgress: true,
        flowId: window.crypto.randomUUID(),
      };

      localStorage.setItem(STORAGE_KEY, JSON.stringify(pkceData));

      // Set up automatic cleanup mechanism to prevent localStorage bloat
      setTimeout(() => {
        const current = localStorage.getItem(STORAGE_KEY);
        if (!current) {
          console.error("Local storage does not have the required data");
          return;
        }

        const currentData = JSON.parse(current) as PKCEData;
        if (currentData.flowId === pkceData.flowId)
          localStorage.removeItem(STORAGE_KEY);
      }, FLOW_TIMEOUT);

      return pkceData.flowId;
    } catch (error: unknown) {
      console.error("Failed to store PKCE data:", error);
      return;
    }
  };

  //  Retrieves PKCE (Proof Key for Code Exchange) data from localStorage
  const retrievePKCEData = (): PKCEData | null => {
    try {
      const data = localStorage.getItem(STORAGE_KEY);
      if (!data) {
        console.error("Local storage does not have the required data");
        return null;
      }

      const parsed: PKCEData = JSON.parse(data) as PKCEData;

      // Check if the PKCE has expired based on the stored expiration timestamp
      if (Date.now() > parsed.expiresAt) {
        localStorage.removeItem(STORAGE_KEY);
        return null;
      }

      return parsed;
    } catch (error: unknown) {
      console.error(`Error retrieving PKCE: ${error}`);
      localStorage.removeItem(STORAGE_KEY);
      return null;
    }
  };

  // Check if there is an active OAuth flow by verifying PKCE data exists
  const hasActiveFlow = computed(() => {
    return retrievePKCEData() !== null;
  });

  // Random stagger utility to prevent multi-tab race conditions
  const withRandomStagger = (
    fnc: (...args: unknown[]) => void,
    maxDelay = 1000
  ): void => {
    setTimeout(fnc, Math.random() * maxDelay);
  };

  // Sign in with google
  const signInWithGoogle = async (): Promise<void> => {
    if (
      !import.meta.env.VITE_OAUTH_CLIENT_ID ||
      !import.meta.env.VITE_OAUTH_REDIRECT_URI
    )
      throw new Error(
        "Missing OAuth configuration: VITE_OAUTH_CLIENT_ID or VITE_OAUTH_REDIRECT_URI"
      );

    if (hasActiveFlow.value) {
      console.warn("OAuth flow already in progress");
      return;
    }

    isLoading.value = true;

    try {
      await new Promise((resolve) => withRandomStagger(resolve, 500));

      if (hasActiveFlow.value) {
        console.warn("OAuth flow started by another tab");
        return;
      }

      // Generate PKCE parameters for security
      const { codeChallenge, codeVerifier } = await generatePKCE();
      const state = btoa(JSON.stringify(generateState())); // Object => string => base64 string

      storePKCEData(state, codeVerifier);

      // Configure OAuth authorization parameters
      const params = new URLSearchParams({
        client_id: import.meta.env.VITE_OAUTH_CLIENT_ID,
        response_type: "code",
        state: state,
        scope: "openid email profile",
        redirect_uri: import.meta.env.VITE_OAUTH_REDIRECT_URI,

        // PKCE parameters
        code_challenge: codeChallenge,
        code_challenge_method: "S256",
      });

      // Redirect user to Google's authorization server
      window.location.href = `${oauth2GoogleEndpoint}?${params}`;
    } catch (error: unknown) {
      console.error("OAuth initialization error:", error);
    } finally {
      isLoading.value = false;
    }
  };

  // Handle OAuth callback after user grants permission
  const handleOAuthCallback = async () => {
    // Extract parameters from the current URL
    const urlParams = new URLSearchParams(window.location.search);

    // Get the authorization code and state from Google
    const authorizationCode = urlParams.get("code");
    const returnedState = urlParams.get("state");
    const error = urlParams.get("error");

    // Handle authorization errors
    if (error) {
      console.error("OAuth authorization error:", error);
      const errorDescription = urlParams.get("error_description");
      alert(`Authorization failed: ${errorDescription || error}`);
      return;
    }

    // Validate required parameters
    if (!authorizationCode || !returnedState) {
      console.error("Missing authorization code or state parameter");
      alert("Invalid OAuth callback parameters");
      return;
    }

    try {
      // Verify state parameter to prevent CSRF attacks
      const storedStateString = localStorage.getItem(STORAGE_KEY);
      if (!storedStateString) {
        throw new Error(
          "No stored OAuth data found - session may have expired"
        );
      }

      const storedData = JSON.parse(storedStateString);

      if (!storedData.state || storedData.state !== returnedState) {
        throw new Error("State parameter mismatch - possible CSRF attack");
      }

      // Retrieve PKCE code verifier
      retrievePKCEData();

      const tokenResult = await exchangeCodeForToken(
        authorizationCode,
        storedData.codeVerifier
      );
      const userResult = await validateToken(tokenResult.data.access_token);

      localStorage.setItem("access_token", tokenResult.data.access_token);
      if (tokenResult.data.refresh_token) {
        localStorage.setItem("refresh_token", tokenResult.data.refresh_token);
      }
      localStorage.setItem("user_data", JSON.stringify(userResult.data));
      localStorage.removeItem(STORAGE_KEY);
    } catch (error: unknown) {
      console.error("OAuth callback error:", error);
    }
  };

  return {
    isLoading: readonly(isLoading),
    signInWithGoogle,
    handleOAuthCallback,
  };
};

export default useOAUth;
