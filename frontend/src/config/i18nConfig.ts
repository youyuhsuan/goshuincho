import { createI18n } from "vue-i18n";
import en from "@/config/locales/en.json";
import zh from "@/config/locales/zh.json";

const i18n = createI18n({
  legacy: false,
  locale:
    JSON.parse(localStorage.getItem("locale") ?? "{}")?.currentLanguage ?? "en",
  fallbackLocale: "en",
  messages: { en, zh },
});

export default i18n;
