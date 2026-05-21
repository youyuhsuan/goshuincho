<script setup lang="ts">
import { computed } from "vue";
// Store
import useSettingStore from "@/stores/setting.store";
// Composables
import useMouse from "@/composables/useMouse";

const settingStore = useSettingStore();
const { x, y } = useMouse();

const wrapperStyle = computed(() => ({
  left: `${x.value}px`,
  top: `${y.value}px`,
}));

const dotStyle = computed(() => ({
  width: "1rem",
  height: "1rem",
  background: settingStore.cursor.color,
  transform: `scale(${settingStore.cursor.size})`,
}));
</script>

<template>
  <Teleport to="body">
    <div class="cursor-wrapper" :style="wrapperStyle">
      <!-- cursor dot -->
      <div
        v-if="settingStore.cursor.type === 'dot'"
        class="cursor-dot"
        :style="dotStyle"
      />

      <!-- cursor stamp -->
      <div
        v-else-if="settingStore.cursor.type === 'stamp'"
        class="cursor-stamp"
        :style="dotStyle"
      />
    </div>
  </Teleport>
</template>

<style scoped>
.cursor-wrapper {
  position: fixed;
  pointer-events: none;
  transform: translate(-50%, -50%);
  z-index: 9999;
}

.cursor-dot {
  border-radius: 50%;
  transition: transform 0.4s cubic-bezier(0.34, 1.56, 0.64, 1);
}

.cursor-stamp {
  transition: transform 0.4s cubic-bezier(0.34, 1.56, 0.64, 1);
}
</style>
