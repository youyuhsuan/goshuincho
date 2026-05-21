<script lang="ts" setup>
import { ref, type Component } from "vue";
// Components
import SettingLayout from "@/components/setting/SettingLayout.vue";
import PersonalContainer from "@/components/setting/PersonalContainer.vue";
import AppearanceContainer from "@/components/setting/AppearenceContainer.vue";
// Type
import type { SettingView } from "@/types/settingType";

const view = ref<SettingView>("personal");

const settingComponentMap: Record<SettingView, Component> = {
  personal: PersonalContainer,
  appearance: AppearanceContainer,
};
</script>

<template>
  <SettingLayout v-model:view="view">
    <component :is="settingComponentMap[view]" />
  </SettingLayout>
</template>
