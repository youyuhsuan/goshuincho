<script setup lang="ts">
import { computed, ref } from "vue";
// Route
import router from "@/router";
// i18n
import { useI18n } from "vue-i18n";
// Primevue
import Popover from "primevue/popover";
import Divider from "primevue/divider";
import Button from "primevue/button";
import "primeicons/primeicons.css";
import type { MenuItem } from "primevue/menuitem";
// Components
import Avatar from "@/components/Avatar.vue";
import Hamburger from "@/components/Hamburger.vue";
import ThemeToggle from "@/components/ThemeToggle.vue";
// Stores
import useAuthStore from "@/stores/auth.store";
// Configs
import ROUTE_CONFIGS from "@/config/routeConfig";

const authStore = useAuthStore();

const { t } = useI18n();

const vAuthPopover = ref<InstanceType<typeof Popover> | null>(null);

// User action menu items, isplayed in user profile popover (desktop) and mobile menu
const desktopMenuItems = computed<MenuItem[]>(() => [
  {
    label: t("nav.about"),
    icon: "pi pi-home",
    route: ROUTE_CONFIGS.ABOUT,
    command: () => router.push(ROUTE_CONFIGS.ABOUT),
    visible: true,
  },
]);
const userMenuItems = computed<MenuItem[]>(() => [
  {
    label: t("nav.settings"),
    icon: "pi pi-cog",
    command: () => {
      vAuthPopover.value?.hide();
      router.push(ROUTE_CONFIGS.SETTING);
    },
    visible: authStore.isAuthenticated,
  },
  {
    label: t("nav.signOut"),
    icon: "pi pi-sign-out",
    command: logout,
    visible: authStore.isAuthenticated,
  },
]);

// Combined menu items for mobile navigation
const allMenuItems = computed<MenuItem[]>(() => [
  ...desktopMenuItems.value,
  ...userMenuItems.value,
]);

// Navigate to authentication page
const goToAuth = () => {
  router.push(ROUTE_CONFIGS.AUTH).catch((error) => {
    console.warn("Navigation failed:", error);
  });
};

const isMobileMenuOpen = ref<boolean>(false);

// Log user out and clean up UI state
const logout = async () => {
  vAuthPopover.value?.hide();
  isMobileMenuOpen.value = false;
  await authStore.logout();
};

// Show user profile popover, Cancels any pending hide timers to prevent menu flickering
const showPopover = (event: MouseEvent) => {
  if (!vAuthPopover.value) return;
  cancelHidePopover();
  vAuthPopover.value?.toggle(event);
};

// Hide user profile popover with delay, Uses setTimeout to allow user to move mouse to popover without it closing
let hideTimerId: ReturnType<typeof setTimeout> | null = null;
const hidePopover = () => {
  if (!vAuthPopover.value) return;
  hideTimerId = setTimeout(() => vAuthPopover.value?.hide(), 150);
};

// Cancel pending popover hide operation, Called when user hovers back over the trigger element
const cancelHidePopover = () => {
  if (hideTimerId) clearTimeout(hideTimerId);
  hideTimerId = null;
};

// Close mobile navigation menu
const closeMobilePopover = () => {
  isMobileMenuOpen.value = false;
};

// Execute menu item command and close mobile menu, Automatically closes the menu after user selects an item
const handleMobileMenuItemClick = (item: MenuItem) => {
  item.command?.({ originalEvent: new Event("click"), item });
  closeMobilePopover();
};
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
    <nav
      class="hidden md:flex items-center gap-4"
      :aria-label="t('nav.ariaLabel.desktop')"
    >
      <!-- Menu Items -->
      <router-link
        v-for="(item, index) in desktopMenuItems"
        :key="index"
        :to="item.route"
        class="text-sm px-4 text-gray-700 dark:text-gray-300 hover:text-gray-600 dark:hover:text-slate-800 transition"
        :aria-label="item.label"
        v-ripple
      >
        {{ item.label }}
      </router-link>

      <!-- Theme Toggle -->
      <ThemeToggle />

      <!-- Auth: Login Button or Avatar -->
      <template v-if="!authStore.isAuthenticated">
        <Button
          icon="pi pi-user"
          variant="text"
          :label="t('nav.login')"
          :aria-label="t('nav.ariaLabel.login')"
          class="flex items-center gap-2 px-3 py-2 rounded-md hover:bg-surface-100 transition-colors"
          @click="goToAuth"
        />
      </template>
      <template v-else>
        <div
          @click="showPopover"
          @mouseenter="showPopover"
          @mouseleave="hidePopover"
          role="button"
          :aria-label="t('nav.ariaLabel.userMenu')"
          class="cursor-pointer"
        >
          <Avatar />
        </div>

        <!-- User Popover (Desktop) -->
        <Popover
          ref="vAuthPopover"
          @mouseenter="cancelHidePopover"
          @mouseleave="hidePopover"
        >
          <div class="flex flex-col w-[12.5rem] py-1 px-0.5">
            <!-- User Info -->
            <div
              class="flex flex-col items-center cursor-pointer hover:opacity-80 transition p-2"
              @click="router.push(ROUTE_CONFIGS.SETTING)"
            >
              <Avatar size="xlarge" iconClass="text-4xl" />
              <div class="font-medium text-sm mt-2">
                {{ authStore.user?.name }}
              </div>
            </div>

            <!-- Divider -->
            <Divider class="my-2" />

            <!-- Action Menu -->
            <ul class="list-none p-0 m-0 flex flex-col">
              <li v-for="(item, index) in userMenuItems" :key="index">
                <Button
                  v-if="item.visible"
                  :label="String(item.label)"
                  :icon="item.icon"
                  variant="text"
                  class="w-full !justify-start text-sm hover:bg-surface-100 transition-colors"
                  @click="item.command()"
                />
              </li>
            </ul>
          </div>
        </Popover>
      </template>
    </nav>

    <!-- Moblie Navigation -->
    <nav
      class="md:hidden flex items-center gap-2"
      :aria-label="t('nav.ariaLabel.mobile')"
    >
      <!-- Theme Toggle -->
      <ThemeToggle />

      <!-- Hamburger Menu -->
      <Hamburger
        v-model:isMobileMenuOpen="isMobileMenuOpen"
        :aria-label="t('nav.ariaLabel.toggleMenu')"
      />

      <!-- Mobile Popover -->
      <Teleport to="body" v-if="isMobileMenuOpen">
        <!-- Background Overlay -->
        <transition
          enter-active-class="transition duration-200"
          enter-from-class="opacity-0"
          enter-to-class="opacity-100"
          leave-active-class="transition duration-150"
          leave-from-class="opacity-100"
          leave-to-class="opacity-0"
        >
          <div
            v-show="isMobileMenuOpen"
            class="fixed inset-0 z-40 bg-black/50"
            @click="closeMobilePopover"
          />
        </transition>

        <!-- Popover Content -->
        <transition
          enter-active-class="transition duration-200"
          enter-from-class="opacity-0 scale-95"
          enter-to-class="opacity-100 scale-100"
          leave-active-class="transition duration-150"
          leave-from-class="opacity-100 scale-100"
          leave-to-class="opacity-0 scale-95"
        >
          <div
            v-show="isMobileMenuOpen"
            class="fixed top-14 right-4 w-64 bg-white dark:bg-slate-800 rounded-lg shadow-xl border border-gray-200 dark:border-slate-700 z-50 p-4"
            @click.stop
          >
            <!-- Auth: Login Button or User Info -->
            <template v-if="!authStore.isAuthenticated">
              <Button
                :label="t('nav.login')"
                :aria-label="t('nav.ariaLabel.login')"
                variant="text"
                class="w-full !justify-start text-sm hover:bg-surface-100 transition-colors"
                @click="goToAuth"
              />
            </template>
            <template v-else>
              <div
                class="flex flex-col items-center cursor-pointer hover:opacity-80 transition p-2"
                @click="router.push(ROUTE_CONFIGS.SETTING)"
              >
                <Avatar size="xlarge" iconClass="text-4xl" />
                <div class="font-medium text-sm mt-2">
                  {{ authStore.user?.name }}
                </div>
              </div>

              <!-- Divider -->
              <Divider class="my-2" />
            </template>

            <!-- Menu Items -->
            <ul class="list-none p-0 m-0 flex flex-col gap-0">
              <li v-for="(item, index) in allMenuItems" :key="index">
                <Button
                  v-if="item.visible"
                  :label="String(item.label)"
                  variant="text"
                  class="w-full !justify-start text-sm hover:bg-surface-100 transition-colors"
                  @click="handleMobileMenuItemClick(item)"
                />
              </li>
            </ul>
          </div>
        </transition>
      </Teleport>
    </nav>
  </header>
</template>

<style scoped>
/* Optional: Add smooth transitions */
header {
  transition: background-color 0.3s ease;
}
</style>
