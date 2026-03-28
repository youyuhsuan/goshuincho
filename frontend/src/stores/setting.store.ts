import { computed, ref } from "vue";
// Pinia
import { defineStore } from "pinia";
// Type
import type { theme } from "@/types/settingType";

const useSettingStore = defineStore(
  "setting",
  () => {
    const currentTheme = ref<theme>("system");
    const mediaQuery = window.matchMedia("(prefers-color-scheme: dark)");

    const changeMode = (mode: theme) => {
      currentTheme.value = mode;

      const shouldBeDark = () => {
        switch (currentTheme.value) {
          case "light":
            return false;
          case "dark":
            return true;
          default:
            return mediaQuery.matches;
        }
      };

      document.documentElement.classList.toggle("my-app-dark", shouldBeDark());
    };

    return { currentTheme, changeMode };
  },
  {
    persist: {
      key: "currentTheme",
      pick: ["currentTheme"],
      storage: localStorage,
    },
  },
);

export default useSettingStore;
