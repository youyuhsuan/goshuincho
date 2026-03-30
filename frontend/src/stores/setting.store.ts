import { computed, ref } from "vue";
// Pinia
import { defineStore } from "pinia";
// i18n
import { useI18n, type Locale } from "vue-i18n";
// Type
import type { theme } from "@/types/settingType";

const useSettingStore = defineStore(
  "setting",
  () => {
    // Theme
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

    // i18n
    const { locale } = useI18n();
    const currentLanguage = ref<Locale>(locale.value);

    const changeLanguage = () => (locale.value = currentLanguage.value);

    return { currentTheme, changeMode, currentLanguage, changeLanguage };
  },
  {
    persist: [
      {
        key: "theme",
        pick: ["currentTheme"],
        storage: localStorage,
      },
      {
        key: "locale",
        pick: ["currentLanguage"],
        storage: localStorage,
      },
    ],
  },
);

export default useSettingStore;
