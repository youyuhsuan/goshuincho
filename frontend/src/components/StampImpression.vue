<script setup lang="ts">
import { ref, watch } from "vue";
// Store
import useSettingStore from "@/stores/setting.store";
// Composables
import useMouse from "@/composables/useMouse";
// Type
import type { Impression } from "@/types/commonType";

const settingStore = useSettingStore();
const { x, y } = useMouse();

const impressions = ref<Impression[]>([]);

watch(
  () => settingStore.cursor.clickId,
  (id) => {
    impressions.value.push({ id, x: x.value, y: y.value });
  },
);

const remove = (id: number) => {
  impressions.value = impressions.value.filter((i) => i.id !== id);
};
</script>

<template>
  <Teleport to="body">
    <div
      v-for="imp in impressions"
      :key="imp.id"
      class="stamp-impression"
      :style="{ left: `${imp.x}px`, top: `${imp.y}px` }"
      @animationend="remove(imp.id)"
    />
  </Teleport>
</template>

<style scoped>
.stamp-impression {
  position: fixed;
  width: 2rem;
  height: 2rem;
  background: var(--p-primary-500);
  pointer-events: none;
  z-index: 9998;
  animation: stamp-press 1.1s cubic-bezier(0.22, 1, 0.36, 1) forwards;
}

@keyframes stamp-press {
  0% {
    transform: translate(-50%, -50%) scale(1.25);
    opacity: 0;
    clip-path: inset(0 0 100% 0);
  }
  30% {
    transform: translate(-50%, -50%) scale(0.9);
    opacity: 0.7;
    clip-path: inset(0 0 0% 0);
  }
  55% {
    transform: translate(-50%, -50%) scale(1.04);
    opacity: 0.55;
  }
  100% {
    transform: translate(-50%, -50%) scale(1.1);
    opacity: 0;
  }
}
</style>
