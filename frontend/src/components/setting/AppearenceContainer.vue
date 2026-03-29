<!-- eslint-disable @typescript-eslint/no-unused-expressions -->
<script setup lang="ts">
import RadioButton from "primevue/radiobutton";
import Card from "primevue/card";
// Store
import useSettingStore from "@/stores/setting.store";
// Type
import type { CardItem } from "@/types/settingType";

const settingStore = useSettingStore();

const cardMap: CardItem[] = [
  { icon: "pi pi-desktop", src: "", view: "system" },
  { icon: "pi pi-sun", src: "", view: "light" },
  { icon: "pi pi-moon", src: "", view: "dark" },
];
</script>

<template>
  <section class="mb-8">
    <!-- Theme Selection -->
    <div class="mb-6">
      <h2 class="text-md font-semibold mb-0.5">
        {{ $t("settings.appearance.theme.title") }}
      </h2>
      <p>{{ $t("settings.appearance.theme.description") }}</p>
    </div>
    <div class="flex flex-1 gap-4 flex-col md:flex-row">
      <template v-for="card in cardMap" :key="card.view">
        <div class="relative w-full h-[32]">
          <Card
            class="h-full cursor-pointer overflow-hidden"
            :class="{
              '!border-2 !border-primary !focus:border-primary-600':
                settingStore.currentTheme === card.view,
            }"
            @click="settingStore.changeMode(card.view)"
          >
            <template #header>
              <div class="w-full h-[10rem] bg-black bg-opacity-50">
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
                <i class="mr-2" :class="card.icon" />
                <span class="text-base lg:text-lg"
                  >{{ $t(`settings.appearance.theme.options.${card.view}`) }}
                </span>
              </div>
            </template>
          </Card>
          <RadioButton
            class="!absolute !right-2.5 !bottom-2.5"
            v-model="settingStore.currentTheme"
            :value="card.view"
          />
        </div>
      </template>
    </div>
  </section>

  <!-- Language Selection -->
  <section>
    <div class="mb-6">
      <h2 class="text-md font-semibold mb-0.5">
        {{ $t("settings.appearance.language.title") }}
      </h2>
      <p>{{ $t("settings.appearance.language.description") }}</p>
    </div>
  </section>
</template>
