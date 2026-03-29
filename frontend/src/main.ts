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

const app = createApp(App);
const pinia = createPinia();

// i18n setup
const i18n = createI18n({
  locale: "en",
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
          50: "{indigo.50}",
          100: "{indigo.100}",
          200: "{indigo.200}",
          300: "{indigo.300}",
          400: "{indigo.400}",
          500: "{indigo.500}",
          600: "{indigo.600}",
          700: "{indigo.700}",
          800: "{indigo.800}",
          900: "{indigo.900}",
          950: "{indigo.950}",
        },
      },
    },
  },
});

app.use(router);
app.use(i18n);
app.use(PrimeVue, {
  theme: {
    preset: MyPreset,
    options: {
      darkModeSelector: ".my-app-dark",
    },
  },
});
app.use(ToastService);
app.use(ConfirmationService);
app.use(pinia);
pinia.use(piniaPluginPersistedstate);

app.mount("#app");
