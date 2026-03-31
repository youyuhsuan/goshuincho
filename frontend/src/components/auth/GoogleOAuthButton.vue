<script setup lang="ts">
import Button from "primevue/button";
// Icon
import IconGoogle from "@/assets/icons/google.svg?component";
// Stores
import useAuthStore from "@/stores/auth.store";

const { oauth } = useAuthStore();
</script>

<template>
  <Button
    type="button"
    :label="$t('auth.oauth.google')"
    @click="oauth.initiateGoogleLogin"
  >
    <template #icon> <IconGoogle class="w-4 h-4" /> </template>
  </Button>
</template>
