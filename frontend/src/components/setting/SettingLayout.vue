<script lang="ts" setup>
import { computed } from "vue";
// Primevue
import Divider from "primevue/divider";
// Types
import type { SettingView } from "@/types/settingType";

const view = defineModel<SettingView>("view");
const menuItems = computed(() => [
  {
    label: "Personal",
    command: () => (view.value = "personal"),
  },
  {
    label: "Appearance",
    command: () => (view.value = "appearance"),
  },
  {
    label: "Billings",
    command: () => (view.value = "billings"),
  },
]);
</script>

<template>
  <div class="flex flex-1">
    <!-- Menu -->
    <aside class="shrink-0 self-stretch">
      <Menu :model="menuItems" class="h-full" />
    </aside>
    <!-- Main layout -->
    <main class="flex-1 px-8 py-10">
      <div class="flex flex-col gap-1 mb-6">
        <h1 class="text-xl font-bold text-surface-900 dark:text-surface-0">
          Settings
        </h1>
        <span>Manage your account settings and preference.</span>
        <Divider />
      </div>
      <slot></slot>
    </main>
  </div>
</template>
