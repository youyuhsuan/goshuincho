import axios from "axios";
// Config
import { API_CONFIG, API_ENDPOINTS } from "@/config/apiConfig";
// Utils
import handleError from "@/utils/errorHandler";
// Store
import useAuthStore from "@/stores/auth.store";

export const instance = axios.create({
  baseURL: API_CONFIG.BASE_URL,
  timeout: API_CONFIG.TIMEOUT,
});

export const authInstance = axios.create({
  baseURL: API_CONFIG.BASE_URL,
  timeout: API_CONFIG.TIMEOUT,
  withCredentials: true,
});

// Global response error handling
instance.interceptors.response.use(
  (response) => response,
  async (error) => {
    return Promise.reject(handleError(error));
  },
);

// Attach access token to Authorization header for authenticated requests
authInstance.interceptors.request.use(async (config) => {
  const authStore = useAuthStore();

  config.headers.Authorization = `Bearer ${authStore.accessToken}`;
  return config;
});

let isRefreshing: boolean = false;
let failedQueue: {
  resolve: (value?: unknown) => void;
  reject: (reason?: unknown) => void;
}[] = [];

const processQueue = (error?: unknown) => {
  failedQueue.forEach((prom) => (error ? prom.reject(error) : prom.resolve()));
  failedQueue = [];
};

const SKIP_REFRESH = [{ url: API_ENDPOINTS.AUTH.REFRESH }];

const shouldSkipRefresh = (error: any) =>
  !SKIP_REFRESH.some((skip) => skip.url === error?.config?.url);

authInstance.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (
      error.response?.status === 401 &&
      !error.config._retry &&
      shouldSkipRefresh(error)
    ) {
      error.config._retry = true;

      try {
        // Avoid multiple simultaneous refresh attempts
        try {
          if (!isRefreshing) {
            isRefreshing = true;
            await useAuthStore().refreshAccessToken();
          } else {
            await new Promise((resolve, reject) => {
              failedQueue.push({ resolve, reject });
            });
          }
          processQueue();
        } catch (error: unknown) {
          processQueue(error);
        } finally {
          isRefreshing = false;
        }

        return authInstance(error.config);
      } catch {
        const { default: useAuthStore } = await import("@/stores/auth.store");
        useAuthStore().logout();
      }
    }

    return Promise.reject(handleError(error));
  },
);
