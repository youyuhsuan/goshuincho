<script setup lang="ts">
import { computed } from "vue";
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
