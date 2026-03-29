<script lang="ts" setup>
import { computed } from "vue";
// i18n
import { useI18n } from "vue-i18n";
// Primevue
import Divider from "primevue/divider";
// Types
import type { SettingView } from "@/types/settingType";

const { t } = useI18n();

const view = defineModel<SettingView>("view");
const menuItems = computed(() => [
  {
    label: t("settings.menu.profile"),
    command: () => (view.value = "personal"),
  },
  {
    label: t("settings.menu.appearance"),
    command: () => (view.value = "appearance"),
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
        <h1 class="text-xl font-bold">
          {{ $t("settings.title") }}
        </h1>
        <span>{{ $t("settings.description") }}</span>
        <Divider />
      </div>
      <slot></slot>
    </main>
  </div>
</template>
