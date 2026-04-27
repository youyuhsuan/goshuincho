import { createRouter, createWebHistory } from "vue-router";
// Components
import HomeView from "@/views/HomeView.vue";
import AuthView from "@/views/AuthView.vue";
import AboutView from "@/views/AboutView.vue";
// Stores
import useAuthStore from "@/stores/auth.store";
// Configs
import ROUTE_CONFIGS from "@/config/routeConfig";

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: ROUTE_CONFIGS.HOME,
      name: "home",
      component: HomeView,
    },
    {
      path: ROUTE_CONFIGS.ABOUTE,
      name: "about",
      component: AboutView,
    },
    {
      path: ROUTE_CONFIGS.AUTH,
      name: "auth",
      component: AuthView,
    },
    {
      path: ROUTE_CONFIGS.OAUTH,
      name: "oAuthCallback",
      component: () => import("@/views/OAuthCallback.vue"),
      meta: {
        fullscreen: true,
      },
      beforeEnter: async (to) => {
        const authStore = useAuthStore();
        authStore.setLoading(true);

        const code = to.query.code as string;
        const state = to.query.state as string;
        const errorParam = to.query.error as string;

        try {
          if (errorParam) throw new Error(`OAuth Error: ${errorParam}`);
          if (!code || !state) throw new Error("Missing code or state");
          if (state !== sessionStorage.getItem("oauth_state"))
            throw new Error("Invalid state parameter");

          sessionStorage.removeItem("oauth_state");
          await authStore.oauth.processGoogleCallback(code);

          authStore.setLoading(false);
          return { path: ROUTE_CONFIGS.HOME };
        } catch (error) {
          console.error(error);
          authStore.setLoading(false);
          return { path: ROUTE_CONFIGS.HOME, hash: "#error=oauth_failed" };
        }
      },
    },
    {
      path: ROUTE_CONFIGS.SETTING,
      name: "settings",
      component: () => import("@/views/SettingsView.vue"),
      meta: {
        requiresAuth: true,
      },
    },
  ],
});

router.beforeEach(async (to, from) => {
  const authStore = useAuthStore();

  // Restore authentication state if tokens exist, but user is not yet authentocated
  if (
    authStore.accessToken &&
    authStore.refreshToken &&
    !authStore.isAuthenticated
  )
    await authStore.initialize();

  // Redirect to auth page if route requires authentication, but user is not logged in
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    return { path: "/auth" };
  }

  // Redirect to home if user is already authenticated and tries to auth pag
  if (to.path === "/auth" && authStore.isAuthenticated) {
    return { path: "/" };
  }
});

export default router;
