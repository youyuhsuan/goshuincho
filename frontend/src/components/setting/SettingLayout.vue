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
    icon: "pi pi-user",
    label: t("settings.menu.profile"),
    view: "personal",
    command: () => (view.value = "personal"),
  },
  {
    icon: "pi pi-palette",
    label: t("settings.menu.appearance"),
    view: "appearance",
    command: () => (view.value = "appearance"),
  },
]);
</script>

<template>
  <div class="flex flex-1">
    <!-- Menu -->
    <aside
      class="shrink-0 self-stretch border-r border-solid border-surface-300"
    >
      <ul class="list-none p-0 m-0 w-48">
        <li
          v-for="item in menuItems"
          :key="item.label"
          class="px-4 py-3 cursor-pointer hover:bg-surface-100 transition-colors"
          :class="{
            'text-primary font-medium bg-surface-50': view === item.view,
          }"
          @click="item.command"
        >
          <i v-if="item.icon" :class="[item.icon, 'mr-2']" />
          {{ item.label }}
        </li>
      </ul>
    </aside>

    <!-- Main layout -->
    <main class="flex-1 px-8 py-10">
      <div class="flex flex-col gap-y-1.5 mb-6">
        <h1 class="text-3xl font-bold tracking-wide">
          {{ $t("settings.title") }}
        </h1>
        <span>{{ $t("settings.description") }}</span>
        <Divider />
      </div>
      <slot></slot>
    </main>
  </div>
</template>
