<template>
  <div class="oauth-callback">
    <div class="loading-container">
      <div v-if="isLoading" class="loading-spinner">
        <div class="spinner"></div>
        <h2>Processing Login...</h2>
        <p>Please wait while we complete your authentication</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from "vue";
import { useRouter } from "vue-router";
import useOAUth from "@/composables/useOAuth";

const router = useRouter();
const { processGoogleCallback, isLoading } = useOAUth();

const redirectToLogin = () => {
  router.push("/login");
};

onMounted(async () => {
  await processGoogleCallback();
  redirectToLogin();
});
</script>
