import { authInstance } from "@/composables/api/useApi";
import { API_ENDPOINTS } from "@/config/apiConfig";
import type { RegisterRequest } from "@/types/authType";

const useApiUser = () => {
  // Get current authenticated user info
  const getUser = (userId?: string) =>
    authInstance.get(
      userId ? `${API_ENDPOINTS.USER}/${userId}` : API_ENDPOINTS.USER,
    );

  const updateUser = (userId: string, payload: Partial<RegisterRequest>) =>
    authInstance.put(`${API_ENDPOINTS.USER}/${userId}`, payload);

  const deleteUser = (userId: string) =>
    authInstance.delete(`${API_ENDPOINTS.USER}/${userId}`);

  return {
    getUser,
    updateUser,
    deleteUser,
  };
};

export default useApiUser;
