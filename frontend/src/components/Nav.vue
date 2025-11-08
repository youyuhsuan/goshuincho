<template>
  <header class="mx-auto max-w-7xl px-6 lg:px-8 pt-8">
    <nav class="flex items-center justify-between">
      <RouterLink to="/">
        <IconLogo class="w-5 h-5" />
      </RouterLink>
      <div class="flex items-center gap-x-6">
        <RouterLink class="flex group/item" to="/about">About</RouterLink>
        <RouterLink class="flex group/item" to="/auth">Login</RouterLink>
      </div>
    </nav>
  </header>
</template>

<script setup lang="ts">
import { RouterLink } from "vue-router";
import IconLogo from "@/assets/icons/logo.svg?component";
</script>
