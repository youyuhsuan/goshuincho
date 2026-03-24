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

// Determine if the current route should be displayed in fullscreen mode
const route = useRoute();
const isFullscreen = computed(() => route.meta.fullscreen === true);

// Controls the global loading state while auth is being initialized
const authStore = useAuthStore();

const ACTIVITY_EVENTS = [
  "mousemove",
  "click",
  "keydown",
  "scroll",
  "touchstart",
];

const handleVisibilityChange = () => {
  if (document.visibilityState === "hidden") {
    authStore.cancelIntervalTimer();
    authStore.cancelResetTimer();
  } else {
    authStore.startIntervalTimer();
    authStore.resetTimer();
  }
};

onMounted(() => {
  authStore.startIntervalTimer();
  ACTIVITY_EVENTS.forEach((event) =>
    window.addEventListener(event, authStore.resetTimer),
  );
  window.addEventListener("visibilitychange", handleVisibilityChange);
});

onBeforeUnmount(() => {
  ACTIVITY_EVENTS.forEach((event) => {
    window.removeEventListener(event, authStore.resetTimer);
  });
  window.removeEventListener("visibilitychange", handleVisibilityChange);

  authStore.cancelIntervalTimer();
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
