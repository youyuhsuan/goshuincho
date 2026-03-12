<template>
  <div class="oauth-callback">
    <div v-if="isLoading" class="loading-spinner">
      <div class="spinner"></div>
      <h2>Processing Login...</h2>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
// Stores
import useAuthStore from "@/stores/auth.store";

const isLoading = ref<boolean>(true);

const router = useRouter();
const route = useRoute();

const { google } = useAuthStore();

onMounted(async () => {
  const code: string = route.query.code as string;
  const state: string = route.query.state as string;
  const errorParam: string = route.query.error as string;

  try {
    if (errorParam) throw new Error(`OAuth Error: ${errorParam}`);
    if (!code || !state) throw new Error("Missing code or state");

    await google.processGoogleCallback(code, state);
    router.push("/");
  } catch (error) {
    console.error(error);
    router.push("/login?error=oauth_failed");
  } finally {
    isLoading.value = false;
  }
});
</script>
