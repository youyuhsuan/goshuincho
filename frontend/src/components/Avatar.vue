<script lang="ts" setup>
// Primevue
import Avatar from "primevue/avatar";
// Stores
import useAuthStore from "@/stores/auth.store";
// Composables
import useProfilePicture from "@/composables/useProfilePicture";

const authStore = useAuthStore();
const { isPictureReady, hasError } = useProfilePicture(
  () => authStore.user?.picture,
);

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
    :image="isPictureReady ? authStore.user?.picture : undefined"
    :size="size"
    shape="circle"
  >
    <template v-if="!isPictureReady || hasError">
      <i
        class="pi pi-user leading-none"
        :class="iconClass"
        style="font-size: inherit"
      />
    </template>
  </Avatar>
</template>
