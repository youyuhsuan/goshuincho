import { instance, authInstance } from "@/composables/api/useApi";
import { API_ENDPOINTS } from "@/config/apiConfig";
import type { LoginRequest, RegisterRequest } from "@/types/userType";

const useApiUser = () => {
  // User CRUD operations
  const createUser = (payload: RegisterRequest) =>
    instance.post(API_ENDPOINTS.USERS, payload);

  const getUser = (userId?: string) =>
    authInstance.get(
      userId ? `${API_ENDPOINTS.USERS}/${userId}` : API_ENDPOINTS.USERS,
    );

  const updateUser = (userId: string, payload: Partial<RegisterRequest>) =>
    authInstance.put(`${API_ENDPOINTS.USERS}/${userId}`, payload);

  const deleteUser = (userId: string) =>
    authInstance.delete(`${API_ENDPOINTS.USERS}/${userId}`);

  // Session management
  // Login
  const createSession = (payload: LoginRequest) =>
    instance.post(API_ENDPOINTS.SESSIONS, payload, {
      withCredentials: true,
    });

  // Get current session info
  const getSession = () => authInstance.get(API_ENDPOINTS.SESSIONS);

  // Logout
  const deleteSession = () => authInstance.delete(API_ENDPOINTS.SESSIONS);

  // Refresh access token
  const refreshSession = () =>
    authInstance.post(`${API_ENDPOINTS.SESSIONS}/refresh`);

  return {
    // user operations
    registerUser: createUser,
    getUser,
    updateUser,
    deleteUser,

    // session operations
    loginUser: createSession,
    getSession,
    logoutUser: deleteSession,
    refreshSession,
  };
};

export default useApiUser;
