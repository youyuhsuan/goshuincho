<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from "vue";
import useSettingStore from "@/stores/setting.store";

const settingStore = useSettingStore();

// Track mouse position
const x = ref<number>(0);
const y = ref<number>(0);
const onMove = (e: MouseEvent) => {
  x.value = e.clientX;
  y.value = e.clientY;
};

// Position the cursor wrapper at the mouse coordinates
const wrapperStyle = computed(() => ({
  left: `${x.value}px`,
  top: `${y.value}px`,
}));

// Apply size via scale and color from store
const dotStyle = computed(() => ({
  width: "1rem",
  height: "1rem",
  background: settingStore.cursor.color,
  transform: `scale(${settingStore.cursor.size})`,
}));

onMounted(() => window.addEventListener("mousemove", onMove));
onUnmounted(() => window.removeEventListener("mousemove", onMove));
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
</style>
