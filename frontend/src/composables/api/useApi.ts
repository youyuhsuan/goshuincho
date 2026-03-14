import axios from "axios";
// Config
import { API_CONFIG, API_ENDPOINTS } from "@/config/apiConfig";
// Utils
import handleError from "@/utils/errorHandler";

export const instance = axios.create({
  baseURL: API_CONFIG.BASE_URL,
  timeout: API_CONFIG.TIMEOUT,
});

export const authInstance = axios.create({
  baseURL: API_CONFIG.BASE_URL,
  timeout: API_CONFIG.TIMEOUT,
  withCredentials: true,
});

instance.interceptors.response.use(
  (response) => response,
  async (error) => {
    return Promise.reject(handleError(error));
  },
);

let isRefreshing: boolean = false;
let failedQueue: {
  resolve: (value?: unknown) => void;
  reject: (reason?: any) => void;
}[] = [];

const processQueue = (error?: any) => {
  failedQueue.forEach((prom) => (error ? prom.reject(error) : prom.resolve()));
  failedQueue = [];
};

const SKIP_REFRESH = [
  { url: API_ENDPOINTS.SESSIONS, method: "get" },
  { url: `${API_ENDPOINTS.SESSIONS}/refresh`, method: "post" },
];

const shouldSkipRefresh = (error: any) =>
  SKIP_REFRESH.some(
    (skip) =>
      skip.url === error?.config?.url && skip.method === error?.confi?.method,
  );

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
        try {
          if (!isRefreshing) {
            isRefreshing = true;
            await authInstance.post(`${API_ENDPOINTS.SESSIONS}/refresh`);
          } else {
            await new Promise((resolve, reject) => {
              failedQueue.push({ resolve, reject });
            });
          }
          processQueue();
        } catch (error) {
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
