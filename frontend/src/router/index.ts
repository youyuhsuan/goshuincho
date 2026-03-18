import { createRouter, createWebHistory } from "vue-router";
// Components
import HomeView from "@/views/HomeView.vue";
import AuthView from "@/views/AuthView.vue";
import OAuthCallback from "@/views/OAuthCallback.vue";
// Stores
import useAuthStore from "@/stores/auth.store";

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: "/",
      name: "home",
      component: HomeView,
    },
    {
      path: "/about",
      name: "about",
      // route level code-splitting
      // this generates a separate chunk (About.[hash].js) for this route
      // which is lazy-loaded when the route is visited.
      component: () => import("../views/AboutView.vue"),
    },
    {
      path: "/auth",
      name: "auth",
      component: AuthView,
    },
    {
      path: "/oauth/callback",
      name: "oAuthCallback",
      component: OAuthCallback,
      meta: {
        fullscreen: true,
      },
    },
    {
      path: "/user",
      name: "user",
      component: AuthView,
      meta: {
        requiresAuth: true,
      },
    },
  ],
});

router.beforeEach(async (to, from) => {
  const { accessToken, refreshToken, isAuthenticated, initialize } =
    useAuthStore();

  if (accessToken && refreshToken) await initialize();
  if (to.meta.requiresAuth && !isAuthenticated) return { path: "/auth" };
});

export default router;
