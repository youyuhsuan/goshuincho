import { createApp } from "vue";
// Prime
import PrimeVue from "primevue/config";
import ToastService from "primevue/toastservice";
import ConfirmationService from "primevue/confirmationservice";
import Aura from "@primeuix/themes/aura";
import { definePreset } from "@primeuix/themes";
// i18n
import { createI18n } from "vue-i18n";
import en from "@/config/locales/en.json";
import zh from "@/config/locales/zh.json";
// Pinia
import { createPinia } from "pinia";
import piniaPluginPersistedstate from "pinia-plugin-persistedstate";
// Components
import App from "@/App.vue";
import router from "@/router";
// Styles
import "@/assets/main.css";
//
import vCursorHover from "@/directives/cursor";

const app = createApp(App);
const pinia = createPinia();

// i18n setup
const i18n = createI18n({
  legacy: false,
  locale:
    JSON.parse(localStorage.getItem("locale") ?? "{}")?.currentLanguage ?? "en",
  fallbackLocale: "en",
  messages: {
    en: en,
    zh: zh,
  },
});

// Customize existing preset
const MyPreset = definePreset(Aura, {
  semantic: {
    colorScheme: {
      light: {
        primary: {
          50: "#FFF0EE",
          100: "#FFD6D0",
          200: "#FFB3A9",
          300: "#FF8F80",
          400: "#FF7057",
          500: "#FF5841",
          600: "#E04030",
          700: "#C02820",
          800: "#8F1810",
          900: "#5C0808",
          950: "#3A0404",
        },
        stone: {
          50: "#FAF8F6",
          100: "#F3EFEB",
          200: "#E8E2DB",
          300: "#D4CBC1",
          400: "#B0A596",
          500: "#8A7E6F",
          600: "#6B6053",
          700: "#4A4239",
          800: "#302A24",
          900: "#1A1612",
        },
      },
    },
  },
});

app.use(i18n);
app.use(PrimeVue, {
  theme: {
    preset: MyPreset,
    options: {},
  },
});
app.use(ToastService);
app.use(ConfirmationService);
app.use(pinia);
pinia.use(piniaPluginPersistedstate);
app.use(router);

app.directive("cursor-hover", vCursorHover);
app.mount("#app");
