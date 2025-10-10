<template>
  <Button type="button" label="Continue with Google" @click="signInWithGoogle">
    <template #icon> <IconGoogle class="w-4 h-4" /> </template>
  </Button>
</template>

<script setup lang="ts">
import Button from "primevue/button";
// Icon
import IconGoogle from "@/assets/icons/google.svg?component";
// Composables
import useOAUth from "@/composables/useOAuth";
const { signInWithGoogle } = useOAUth();
</script>
