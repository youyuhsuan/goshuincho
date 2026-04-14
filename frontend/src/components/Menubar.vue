<script setup lang="ts">
import { computed, ref, watch } from "vue";
// Route
import { useRoute } from "vue-router";
import router from "@/router";
// i18n
import { useI18n } from "vue-i18n";
// Primevue
import Menubar from "primevue/menubar";
import Popover from "primevue/popover";
import Divider from "primevue/divider";
import "primeicons/primeicons.css";
import type { MenuItem } from "primevue/menuitem";
// Components
import Avatar from "@/components/Avatar.vue";
// Stores
import useAuthStore from "@/stores/auth.store";
// Configs
import ROUTE_CONFIGS from "@/config/routeConfig";

const authStore = useAuthStore();

const { t } = useI18n();

const route = useRoute();
const items = computed<MenuItem[]>(() => [
  {
    label: t("nav.about"),
    command: () => router.push(ROUTE_CONFIGS.ABOUTE),
    route: ROUTE_CONFIGS.ABOUTE,
  },
]);

watch(
  () => route.path,
  (newPath) => {
    items.value.forEach((item) => {
      item.class = newPath === item.route ? "active-menu-item" : "";
    });
  },
  { immediate: true },
);

// Control overlay panel
const vPopover = ref<InstanceType<typeof Popover> | null>(null);
let hideTimerId: ReturnType<typeof setTimeout> | null = null;

const showPopover = (event: MouseEvent) => {
  if (!vPopover.value) return;

  cancelHidePopover();
  vPopover.value?.toggle(event);
};

const hidePopover = () => {
  if (!vPopover.value) return;

  hideTimerId = setTimeout(() => vPopover.value?.hide(), 150);
};

const cancelHidePopover = () => {
  if (hideTimerId) clearTimeout(hideTimerId);
  hideTimerId = null;
};

// Logout action
const logout = async () => {
  vPopover.value?.hide();
  await authStore.logout();
};
</script>

<template>
  <header>
    <Menubar :model="items">
      <template #start>
        <a href="/" :aria-label="t('nav.ariaLabel.home')">
          <svg
            width="35"
            height="40"
            viewBox="0 0 35 40"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
            class="h-8"
          >
            <path
              d="M25.87 18.05L23.16 17.45L25.27 20.46V29.78L32.49 23.76V13.53L29.18 14.73L25.87 18.04V18.05ZM25.27 35.49L29.18 31.58V27.67L25.27 30.98V35.49ZM20.16 17.14H20.03H20.17H20.16ZM30.1 5.19L34.89 4.81L33.08 12.33L24.1 15.67L30.08 5.2L30.1 5.19ZM5.72 14.74L2.41 13.54V23.77L9.63 29.79V20.47L11.74 17.46L9.03 18.06L5.72 14.75V14.74ZM9.63 30.98L5.72 27.67V31.58L9.63 35.49V30.98ZM4.8 5.2L10.78 15.67L1.81 12.33L0 4.81L4.79 5.19L4.8 5.2ZM24.37 21.05V34.59L22.56 37.29L20.46 39.4H14.44L12.34 37.29L10.53 34.59V21.05L12.42 18.23L17.45 26.8L22.48 18.23L24.37 21.05ZM22.85 0L22.57 0.69L17.45 13.08L12.33 0.69L12.05 0H22.85Z"
              fill="var(--p-primary-color)"
            />
            <path
              d="M30.69 4.21L24.37 4.81L22.57 0.69L22.86 0H26.48L30.69 4.21ZM23.75 5.67L22.66 3.08L18.05 14.24V17.14H19.7H20.03H20.16H20.2L24.1 15.7L30.11 5.19L23.75 5.67ZM4.21002 4.21L10.53 4.81L12.33 0.69L12.05 0H8.43002L4.22002 4.21H4.21002ZM21.9 17.4L20.6 18.2H14.3L13 17.4L12.4 18.2L12.42 18.23L17.45 26.8L22.48 18.23L22.5 18.2L21.9 17.4ZM4.79002 5.19L10.8 15.7L14.7 17.14H14.74H15.2H16.85V14.24L12.24 3.09L11.15 5.68L4.79002 5.2V5.19Z"
              fill="var(--p-text-color)"
            />
          </svg>
          <span class="sr-only">{{ t("nav.home") }}</span>
        </a>
      </template>

      <template #item="{ item, props }">
        <a
          v-ripple
          :href="item.url"
          v-bind="props.action"
          :class="item.class"
          :aria-label="
            typeof item.label === 'function' ? item.label() : item.label
          "
        >
          <span>{{ item.label }}</span>
        </a>
      </template>

      <template #end>
        <!-- Authentication Button -->
        <Button
          v-if="!authStore.isAuthenticated"
          icon="pi pi-user"
          variant="text"
          class="p-button p-button-text flex items-center gap-2 px-3 py-2 rounded-md hover:bg-surface-100 transition-colors"
          :label="authStore.isAuthenticated ? t('nav.signOut') : t('nav.login')"
          :aria-label="
            authStore.isAuthenticated
              ? t('nav.ariaLabel.signOut')
              : t('nav.ariaLabel.login')
          "
          @click="router.push(ROUTE_CONFIGS.AUTH)"
        >
        </Button>
        <!-- Avatar trigger -->
        <div
          v-else
          @click="showPopover"
          @mouseenter="showPopover"
          @mouseleave="hidePopover"
        >
          <Avatar />
        </div>

        <!-- User Popover -->
        <Popover
          ref="vPopover"
          @mouseenter="cancelHidePopover"
          @mouseleave="hidePopover"
        >
          <div class="flex flex-col w-[12.5rem] py-1 px-0.5">
            <!-- User info -->
            <div
              role="img"
              :aria-label="t('nav.ariaLabel.userInfo')"
              class="flex flex-col items-center cursor-pointer"
              @click="router.push(ROUTE_CONFIGS.SETTING)"
            >
              <Avatar size="xlarge" iconClass="text-4xl" />
              <div aria-hidden="true" class="font-medium">
                {{ authStore.user?.name }}
              </div>
            </div>

            <Divider />

            <!-- Action menu -->
            <ul class="list-none p-0 m-0 flex flex-col gap-4">
              <!-- Settings Button -->
              <li>
                <Button
                  :label="t('nav.settings')"
                  :aria-label="t('nav.ariaLabel.settings')"
                  variant="text"
                  icon="pi pi-cog"
                  class="w-full flex gap-2 cursor-pointer text-sm hover:text-slate-600"
                  @click="router.push(ROUTE_CONFIGS.SETTING)"
                >
                </Button>
              </li>

              <!-- Sign Out Button -->
              <li>
                <Button
                  :label="t('nav.signOut')"
                  :aria-label="t('nav.ariaLabel.signOut')"
                  variant="text"
                  icon="pi pi-sign-out"
                  class="w-full flex gap-2 cursor-pointer text-sm hover:text-slate-700"
                  @click="logout"
                >
                </Button>
              </li>
            </ul>
          </div>
        </Popover>
      </template>
    </Menubar>
  </header>
</template>
