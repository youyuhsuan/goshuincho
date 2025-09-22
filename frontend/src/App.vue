<script setup lang="ts">
import { RouterLink, RouterView } from "vue-router";
</script>

<template>
  <header class="mx-auto max-w-7xl px-6 lg:px-8 pt-8">
    <nav class="flex items-center justify-between">
      <RouterLink to="/">
        <img
          alt="Vue logo"
          class="logo"
          src="@/assets/logo.svg"
          width="32"
          height="32"
        />
      </RouterLink>
      <div class="flex items-center gap-x-6">
        <RouterLink class="flex group/item" to="/about">About</RouterLink>
        <RouterLink class="flex group/item" to="/auth">Login</RouterLink>
      </div>
    </nav>
  </header>
  <RouterView />
</template>
