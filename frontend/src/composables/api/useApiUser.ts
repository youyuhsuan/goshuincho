import instance from "@/composables/api/useApi";
import { API_ENDPOINTS } from "@/config/apiConfig";
import type { LoginRequest, RegisterRequest } from "@/types/userType";

const useApiUser = () => {
  // User CRUD operations
  const createUser = (payload: RegisterRequest) =>
    instance.post(API_ENDPOINTS.USERS, payload);

  const getUser = (userId?: string) =>
    instance.get(
      userId ? `${API_ENDPOINTS.USERS}/${userId}` : API_ENDPOINTS.USERS,
    );

  const updateUser = (userId: string, payload: Partial<RegisterRequest>) =>
    instance.put(`${API_ENDPOINTS.USERS}/${userId}`, payload);

  const deleteUser = (userId: string) =>
    instance.delete(`${API_ENDPOINTS.USERS}/${userId}`);

  // Session management
  // Login
  const createSession = (payload: LoginRequest) =>
    instance.post(API_ENDPOINTS.SESSIONS, payload);

  // Get current session info
  const getSession = () => instance.get(API_ENDPOINTS.SESSIONS);

  // Logout
  const deleteSession = () => instance.delete(API_ENDPOINTS.SESSIONS);

  // Refresh access token
  const refreshSession = () =>
    instance.post(`${API_ENDPOINTS.SESSIONS}/refresh`);

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
