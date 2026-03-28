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
  { title: "System Mode", icon: "pi pi-desktop", src: "", view: "system" },
  { title: "Light Mode", icon: "pi pi-sun", src: "", view: "light" },
  { title: "Dark Mode", icon: "pi pi-moon", src: "", view: "dark" },
];
</script>

<template>
  <section class="mb-8">
    <div class="mb-6">
      <h2 class="text-md font-semibold mb-0.5">Themes</h2>
      <p>Select or customize your UI theme.</p>
    </div>
    <div class="flex flex-1 gap-4 flex-col md:flex-row">
      <template v-for="card in cardMap" :key="card.title">
        <div class="relative w-full h-[32]">
          <Card
            class="h-full cursor-pointer"
            :class="{
              '!border-2 !border-primary !focus:border-primary-600 ':
                settingStore.currentTheme === card.view,
            }"
            @click="settingStore.changeMode(card.view)"
          >
            <template #header>
              <img
                class="w-full h-full object-cover"
                :alt="card.title"
                :src="card.src"
              />
            </template>
            <template #title>
              <div class="flex items-center">
                <i class="mr-2" :class="card.icon" />
                <span class="text-base lg:text-lg">{{ card.title }}</span>
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
  <!-- <section>
    <div class="mb-6">
      <h2 class="text-md font-semibold mb-0.5">Language</h2>
      <p>Select the language of the platform.</p>
    </div>
  </section> -->
</template>
