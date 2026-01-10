import instance from "@/composables/api/useApi";
import { API_ENDPOINTS } from "@/config/apiConfig";
import type { LoginRequest, RegisterRequest } from "@/types/userType";

const useApiUser = () => {
  // User CRUD operations
  const createUser = (payload: RegisterRequest) =>
    instance.post(API_ENDPOINTS.USERS, payload);

  const getUser = (userId?: string) =>
    instance.get(
      userId ? `${API_ENDPOINTS.USERS}/${userId}` : API_ENDPOINTS.USERS
    );

  const updateUser = (userId: string, payload: Partial<RegisterRequest>) =>
    instance.put(`${API_ENDPOINTS.USERS}/${userId}`, payload);

  const deleteUser = (userId: string) =>
    instance.delete(`${API_ENDPOINTS.USERS}/${userId}`);

  // Session management
  const createSession = (payload: LoginRequest) =>
    instance.post(API_ENDPOINTS.SESSIONS, payload); // Login

  const getSession = () => instance.get(API_ENDPOINTS.SESSIONS); // Get current session info

  const deleteSession = () => instance.delete(API_ENDPOINTS.SESSIONS); // Logout

  const getGoogleOAuth = () =>
    instance.post<string>(API_ENDPOINTS.OAUTH.AUTHORIZATIONS, {
      provider: "google",
    });

  return {
    // RESTful user operations
    registerUser: createUser,
    getUser,
    updateUser,
    deleteUser,

    // RESTful session operations
    loginUser: createSession,
    getSession,
    logoutUser: deleteSession,

    getGoogleOAuth,
  };
};

export default useApiUser;
