<script setup lang="ts">
import { computed, onMounted } from "vue";
import { RouterView, useRoute } from "vue-router";
// Components
import Menubar from "@/components/Menubar.vue";
import Footer from "@/components/Footer.vue";
// Stores
import useAuthSore from "@/stores/auth.store";

const authStore = useAuthSore();
const route = useRoute();

const isFullscreen = computed(() => route.meta.fullscreen === true);

onMounted(() => {
  authStore.checkSession();
});
</script>

<template>
  <template v-if="!isFullscreen">
    <Toast />
    <Menubar />
  </template>

  <div :class="isFullscreen ? 'min-h-screen' : 'min-h-screen flex flex-col'">
    <RouterView />
    <Footer v-if="!isFullscreen" />
  </div>
</template>
