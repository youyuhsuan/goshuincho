<template>
  <Button
    type="button"
    label="Continue with Google"
    @click="google.initiateGoogleLogin"
  >
    <template #icon> <IconGoogle class="w-4 h-4" /> </template>
  </Button>
</template>

<script setup lang="ts">
import Button from "primevue/button";
// Icon
import IconGoogle from "@/assets/icons/google.svg?component";
// Stores
import useAuthStore from "@/stores/auth.store";

const { google } = useAuthStore();
</script>
