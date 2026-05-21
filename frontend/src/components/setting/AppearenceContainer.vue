<script setup lang="ts">
import { computed } from "vue";
// i18n
import { useI18n } from "vue-i18n";
// Primevue
import RadioButton from "primevue/radiobutton";
import Card from "primevue/card";
import Select from "primevue/select";
// Store
import useSettingStore from "@/stores/setting.store";
// Type
import type { Language, LanguageOption } from "@/types/settingType";
// Utils
import generateFieldIds, { type FieldIds } from "@/utils/generateFieldIds";
// Constants
import { THEME_CARD_MAP } from "@/constants/style";
import { SUPPORTED_LOCALES } from "@/constants/common";

const settingStore = useSettingStore();

const { t } = useI18n();
// Generate field IDs for accessibility
const fieldIds: FieldIds = generateFieldIds(SUPPORTED_LOCALES);
const languageOption = computed<LanguageOption[]>(() =>
  SUPPORTED_LOCALES.map((code: Language) => ({
    name: t(`settings.appearance.language.options.${code}`),
    code,
  })),
);
</script>

<template>
  <section class="mb-8">
    <!-- Theme Selection -->
    <div class="mb-6">
      <h2 class="text-base font-semibold mb-0.5">
        {{ $t("settings.appearance.theme.title") }}
      </h2>
      <p class="text-sm text-muted-color">
        {{ $t("settings.appearance.theme.description") }}
      </p>
    </div>

    <div class="flex gap-4 flex-col md:flex-row">
      <template v-for="card in THEME_CARD_MAP" :key="card.view">
        <div class="relative w-full">
          <Card
            class="h-full cursor-pointer overflow-hidden transition-shadow"
            :class="{
              '!border-2 !border-primary': settingStore.userTheme === card.view,
            }"
            @click="settingStore.changeThemeMode(card.view)"
          >
            <template #header>
              <div class="w-full h-32 md:h-40 bg-surface-100">
                <img
                  v-if="card.src"
                  class="w-full h-full object-cover"
                  :src="card.src"
                  :alt="$t(`settings.appearance.theme.options.${card.view}`)"
                />
              </div>
            </template>
            <template #title>
              <div class="flex items-center">
                <i class="mr-2 shrink-0" :class="card.icon" />
                <span class="text-sm md:text-base lg:text-lg truncate">
                  {{ $t(`settings.appearance.theme.options.${card.view}`) }}
                </span>
              </div>
            </template>
          </Card>
          <RadioButton
            class="!absolute !right-2.5 !bottom-2.5"
            v-model="settingStore.userTheme"
            :value="card.view"
          />
        </div>
      </template>
    </div>
  </section>

  <!-- Language Selection -->
  <section>
    <div class="mb-6">
      <h2 class="text-base font-semibold mb-0.5">
        {{ $t("settings.appearance.language.title") }}
      </h2>
      <p class="text-sm text-muted-color">
        {{ $t("settings.appearance.language.description") }}
      </p>
    </div>

    <FloatLabel class="w-full max-w-xs">
      <Select
        v-model="settingStore.currentLanguage"
        :inputId="fieldIds.language"
        :options="languageOption"
        optionLabel="name"
        optionValue="code"
        class="w-full"
        checkmark
        @change="settingStore.changeLanguage()"
        :placeholder="$t('settings.appearance.language.placeholder')"
      />
    </FloatLabel>
  </section>
</template>
