<script setup lang="ts">
// Components
import FortuneSticks from "@/components/auth/FortuneSticks.vue";
// Route
import { useRoute } from "vue-router";
const route = useRoute();
</script>

<template>
  <main class="flex w-full flex-1">
    <RouterView v-slot="{ Component }">
      <Transition
        mode="out-in"
        enter-active-class="transition-opacity duration-200 ease-in"
        enter-from-class="opacity-0"
        leave-active-class="transition-opacity duration-150 ease-out"
        leave-to-class="opacity-0"
      >
        <component
          :is="Component"
          :key="route.name"
          class="w-full max-w-md lg:max-w-none flex lg:flex-1 flex-col items-center justify-center px-6 sm:px-8 lg:px-10 py-8"
        />
      </Transition>
    </RouterView>

    <section
      class="hidden lg:flex flex-1 items-center justify-center bg-surface-100"
    ></section>
  </main>
</template>
