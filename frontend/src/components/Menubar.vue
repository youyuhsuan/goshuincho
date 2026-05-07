<script setup lang="ts">
// i18n
import { useI18n } from "vue-i18n";

// Components
import DesktopNav from "@/components/DesktopNav.vue";
import MobileNav from "@/components/MobileNav.vue";

const { t } = useI18n();
</script>

<template>
  <header
    :aria-label="t('nav.ariaLabel.main')"
    class="flex items-center justify-between max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-2"
  >
    <!-- Logo -->
    <router-link
      to="/"
      class="font-bold text-xl text-gray-900 dark:text-white hover:opacity-80 transition"
    >
      {{ t("common.title") }}
    </router-link>

    <!-- Desktop Navigation-->
    <DesktopNav class="hidden md:flex" />
    <!-- Moblie Navigation -->
    <MobileNav class="md:hidden" />
  </header>
</template>

<style scoped>
header {
  transition: background-color 0.3s ease;
}
</style>
