<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted } from "vue";
import { RouterView, useRoute } from "vue-router";
// Premevue
import ProgressSpinner from "primevue/progressspinner";
// Components
import Menubar from "@/components/Menubar.vue";
import Footer from "@/components/Footer.vue";
// Stores
import useAuthStore from "@/stores/auth.store";
// Utils
import debounce from "@/utils/debounce";

// Determine if the current route should be displayed in fullscreen mode
const route = useRoute();
const isFullscreen = computed(() => route.meta.fullscreen === true);

// Controls the global loading state while auth is being initialized
const authStore = useAuthStore();

// A list of DOM events considered indicative of user activity
const ACTIVITY_EVENTS = [
  "mousemove",
  "click",
  "keydown",
  "scroll",
  "touchstart",
] as const;

// Pause the inactivity timer when the user navigates away from the tab, and resume it when the tab becomes visible again
const handleVisibilityChange = () => {
  if (document.visibilityState === "hidden") {
    authStore.cancelResetTimer();
  } else {
    authStore.resetTimer();
  }
};

// Debounce the timer reset to avoid firing excessively on high-frequency
const debouncedResetTimer = debounce(authStore.resetTimer, 1000);

// Register all activity event listeners and the visibility change handler
onMounted(() => {
  ACTIVITY_EVENTS.forEach((event) =>
    window.addEventListener(event, debouncedResetTimer),
  );
  window.addEventListener("visibilitychange", handleVisibilityChange);
});

// Clean up all event listeners and cancel any pending timer when the
onBeforeUnmount(() => {
  ACTIVITY_EVENTS.forEach((event) => {
    window.removeEventListener(event, debouncedResetTimer);
  });
  window.removeEventListener("visibilitychange", handleVisibilityChange);
  authStore.cancelResetTimer();
});
</script>

<template>
  <!-- Loading -->
  <div
    v-if="authStore.isLoading"
    class="flex items-center justify-center h-screen"
  >
    <ProgressSpinner aria-label="Loading" />
  </div>

  <!-- Main container -->
  <template v-else>
    <template v-if="!isFullscreen">
      <Toast />
      <Menubar />
    </template>

    <div :class="isFullscreen ? 'min-h-screen' : 'min-h-screen flex flex-col'">
      <RouterView />
      <Footer v-if="!isFullscreen" />
    </div>
  </template>
</template>
