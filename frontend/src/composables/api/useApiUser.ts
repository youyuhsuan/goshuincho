// Composables
import { authInstance } from "@/composables/api/useApi";
// Config
import { API_ENDPOINTS } from "@/config/apiConfig";
// Types
import type { UpdateRequest, User } from "@/types/userType";

const useApiUser = () => {
  // Get current authenticated user info
  const getUser = (userId: string) =>
    authInstance.get<User>(`${API_ENDPOINTS.USER}/${userId}`);

  const updateUser = (userId: string, payload: Partial<UpdateRequest>) =>
    authInstance.patch(`${API_ENDPOINTS.USER}/${userId}`, payload);

  const uploadUserImage = (userId: string, formData: FormData) =>
    authInstance.post<string>(
      `${API_ENDPOINTS.USER}/${userId}/picture`,
      formData,
    );

  const deleteUser = (userId: string) =>
    authInstance.delete(`${API_ENDPOINTS.USER}/${userId}`);

  return {
    getUser,
    updateUser,
    uploadUserImage,
    deleteUser,
  };
};

export default useApiUser;
