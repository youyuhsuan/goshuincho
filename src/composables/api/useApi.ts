import axios from "axios";
// Config
import { API_CONFIG } from "@/config/apiConfig";

const instance = axios.create({
  baseURL: API_CONFIG.BASE_URL,
  timeout: API_CONFIG.TIMEOUT,
});

instance.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) {
      // await refreshToken()
      return instance(error.config); // Retry original request
    }
  }
);

export default instance;
