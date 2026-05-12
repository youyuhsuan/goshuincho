import { computed, ref } from "vue";
// Pinia
import { defineStore } from "pinia";

const useLoadingStore = defineStore("loading", () => {
  const count = ref<number>(0);

  const isGlobalLoading = computed(() => count.value > 0);

  const start = () => {
    count.value++;
  };

  const stop = () => {
    count.value = Math.max(0, count.value - 1);
  };

  return { isGlobalLoading, start, stop };
});

export default useLoadingStore;
