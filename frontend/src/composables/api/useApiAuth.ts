// composables
import { instance, authInstance } from "@/composables/api/useApi";
// Config
import { API_ENDPOINTS } from "@/config/apiConfig";
// Stores
import useAuthStore from "@/stores/auth.store";
// Types
import type {
  LoginRequest,
  RegisterRequest,
  TokenResponse,
} from "@/types/authType";

const useApiAuth = () => {
  // Get current authenticated user info
  const getCurrentAuth = () => authInstance.get(API_ENDPOINTS.AUTH.ME);

  // Authenticate user and issue tokens
  const loginUser = (payload: LoginRequest) =>
    instance.post<TokenResponse>(API_ENDPOINTS.AUTH.LOGIN, payload);

  // Authenticate user and issue tokens
  const registerUser = (payload: RegisterRequest) =>
    instance.post(API_ENDPOINTS.AUTH.REGISTER, payload);

  // Logout current user
  const logoutUser = () => authInstance.post(API_ENDPOINTS.AUTH.LOGOUT);

  // Issue a new Access Token using a valid Refresh Token
  const refreshAccessToken = () =>
    instance.post<TokenResponse>(API_ENDPOINTS.AUTH.REFRESH, {
      refreshToken: useAuthStore().refreshToken,
    });

  return {
    getCurrentAuth,
    loginUser,
    registerUser,
    logoutUser,
    refreshAccessToken,
  };
};

export default useApiAuth;
