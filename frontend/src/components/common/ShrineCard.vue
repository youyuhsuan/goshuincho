<script lang="ts" setup>
// Types
import type { Shrine } from "@/types/shrinesType";
// Utils
import { formatAddress } from "@/utils/formatUI";

defineProps<{ shrine: Shrine }>();
</script>

<template>
  <div class="group flex flex-col">
    <!-- Image -->
    <div class="relative h-64 bg-stone-100 overflow-hidden mb-4">
      <img
        v-if="shrine.imageUrl"
        class="w-full h-full object-cover transition-transform duration-[600ms] ease-out group-hover:scale-105"
        :src="shrine.imageUrl"
        :alt="shrine.name"
        loading="lazy"
      />
      <div
        v-else
        class="w-full h-full flex items-center justify-center text-stone-300"
      >
        <i class="pi pi-image text-4xl" />
      </div>

      <div
        class="absolute inset-0 bg-gradient-to-t from-black/20 to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300"
      />
    </div>

    <!-- Info -->
    <div class="px-1">
      <p
        class="text-base font-medium text-stone-700 tracking-wide transition-colors group-hover:text-primary-500"
      >
        {{ shrine.name }}
      </p>
      <p class="text-xs text-stone-400 mt-1.5 tracking-wide">
        {{ formatAddress(shrine) }}
      </p>
    </div>
  </div>
</template>
