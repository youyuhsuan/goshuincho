import { computed, ref } from "vue";
// Pinia
import { defineStore } from "pinia";

const INTRO_DURATION_MS = 3800;

const useLoadingStore = defineStore("loading", () => {
  const count = ref<number>(0);
  const introShown = ref<boolean>(false);

  const isGlobalLoading = computed(() => count.value > 0);

  const start = () => count.value++;
  const stop = () => (count.value = Math.max(0, count.value - 1));

  // Plays the intro animation once per session; resolves immediately on subsequent calls.
  const playIntro = (): Promise<void> => {
    if (introShown.value) return Promise.resolve();
    introShown.value = true;
    start();
    return new Promise((resolve) => {
      setTimeout(() => {
        stop();
        resolve();
      }, INTRO_DURATION_MS);
    });
  };

  return { isGlobalLoading, start, stop, playIntro };
});

export default useLoadingStore;
