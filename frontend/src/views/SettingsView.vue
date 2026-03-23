<script lang="ts" setup>
import { ref } from "vue";
// Components
import SettingLayout from "@/components/setting/SettingLayout.vue";
// Type
import type { SettingView } from "@/types/settingType";

const view = ref<SettingView>("personal");
</script>

<template>
  <SettingLayout v-model:view="view"> </SettingLayout>
</template>
