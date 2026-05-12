import { describe, it, expect, beforeEach, vi } from "vitest";
import { setActivePinia, createPinia } from "pinia";
import useAuthStore from "../auth.store";
import useLoadingStore from "../loading.store";

const { mockGetCurrentAuth } = vi.hoisted(() => ({
  mockGetCurrentAuth: vi
    .fn()
    .mockResolvedValue({ data: { id: 1, name: "Test" } }),
}));

vi.mock("@/composables/api/useApiAuth", () => ({
  default: () => ({
    getCurrentAuth: mockGetCurrentAuth,
    loginUser: vi.fn(),
    logoutUser: vi.fn(),
    refreshAccessToken: vi.fn(),
  }),
}));

vi.mock("@/composables/api/useApiOAuth", () => ({
  default: () => ({
    createGoogleAuthorization: vi.fn(),
    createGoogleToken: vi.fn(),
  }),
}));

vi.mock("@/router", () => ({
  default: { push: vi.fn() },
}));

describe("useAuthStore", () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    mockGetCurrentAuth.mockResolvedValue({ data: { id: 1, name: "Test" } });
  });

  describe("initialize()", () => {
    it("starts and stops global loading when tokens exist", async () => {
      const authStore = useAuthStore();
      const loadingStore = useLoadingStore();
      authStore.accessToken = "access";
      authStore.refreshToken = "refresh";

      const startSpy = vi.spyOn(loadingStore, "start");
      const stopSpy = vi.spyOn(loadingStore, "stop");

      await authStore.initialize();

      expect(startSpy).toHaveBeenCalledOnce();
      expect(stopSpy).toHaveBeenCalledOnce();
    });

    it("stops global loading even when no tokens exist", async () => {
      const authStore = useAuthStore();
      const loadingStore = useLoadingStore();

      const startSpy = vi.spyOn(loadingStore, "start");
      const stopSpy = vi.spyOn(loadingStore, "stop");

      await authStore.initialize();

      expect(startSpy).toHaveBeenCalledOnce();
      expect(stopSpy).toHaveBeenCalledOnce();
    });

    it("stops global loading even when API throws", async () => {
      mockGetCurrentAuth.mockRejectedValueOnce(new Error("network error"));

      const authStore = useAuthStore();
      const loadingStore = useLoadingStore();
      authStore.accessToken = "access";
      authStore.refreshToken = "refresh";

      const stopSpy = vi.spyOn(loadingStore, "stop");

      await expect(authStore.initialize()).rejects.toThrow("network error");
      expect(stopSpy).toHaveBeenCalledOnce();
    });

    it("sets isAuthenticated to true on success", async () => {
      const authStore = useAuthStore();
      authStore.accessToken = "access";
      authStore.refreshToken = "refresh";

      await authStore.initialize();

      expect(authStore.isAuthenticated).toBe(true);
    });

    it("leaves isAuthenticated false when no tokens exist", async () => {
      const authStore = useAuthStore();

      await authStore.initialize();

      expect(authStore.isAuthenticated).toBe(false);
    });
  });
});
