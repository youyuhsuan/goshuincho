<script lang="ts" setup>
import { ref } from "vue";
// Primevue
import Avatar from "primevue/avatar";
// Stores
import useAuthStore from "@/stores/auth.store";

const authStore = useAuthStore();
const hasError = ref<boolean>(false);

withDefaults(
  defineProps<{ size?: string; iconClass?: string; avatarClass?: string }>(),
  {
    size: "small",
  },
);
</script>

<template>
  <Avatar
    class="!rounded-full !object-cover !flex !items-center !justify-center"
    :class="[{ '!border-2': !authStore.user?.picture }, avatarClass]"
    :image="hasError ? undefined : authStore.user?.picture"
    :size="size"
    shape="circle"
    @error="hasError = true"
  >
    <template v-if="!authStore.user?.picture || hasError">
      <i
        class="pi pi-user leading-none"
        :class="iconClass"
        style="font-size: inherit"
      />
    </template>
  </Avatar>
</template>
