<template>
  <div class="oauth-callback">
    <div class="loading-container">
      <div v-if="isLoading" class="loading-spinner">
        <div class="spinner"></div>
        <h2>Processing Login...</h2>
        <p>Please wait while we complete your authentication</p>
      </div>

      <div v-else-if="error" class="error-message">
        <h2>Login Failed</h2>
        <p>{{ error }}</p>
        <button @click="redirectToLogin" class="retry-button">Try Again</button>
      </div>

      <div v-else class="success-message">
        <h2>Login Successful!</h2>
        <p>Redirecting...</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRouter, useRoute } from "vue-router";

// Composables
import useOAuth from "@/composables/useOauth";

const router = useRouter();
const route = useRoute();

const isLoading = ref(true);
const error = ref<string | null>(null);

const { handleOAuthCallback } = useOAuth();

const redirectToLogin = () => {
  router.push("/login");
};

onMounted(async () => {
  try {
    // Extract parameters from URL
    const code = route.query.code as string;
    const state = route.query.state as string;
    const errorParam = route.query.error as string;

    // Handle OAuth errors
    if (errorParam) {
      const errorDescription = route.query.error_description as string;
      throw new Error(`OAuth Error: ${errorDescription || errorParam}`);
    }

    // Validate required parameters
    if (!code || !state) {
      throw new Error("Missing authorization code or state parameter");
    }

    // Process the OAuth callback
    await handleOAuthCallback();
  } catch (error: unknown) {
    console.error("OAuth callback error:", error);
  } finally {
    isLoading.value = false;
  }
});
</script>
