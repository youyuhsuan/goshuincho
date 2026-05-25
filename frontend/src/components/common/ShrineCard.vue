<script lang="ts" setup>
import { ref } from "vue";
// Types
import type { Shrine } from "@/types/shrinesType";
// Utils
import { formatAddress } from "@/utils/formatUI";
// Stores
import useAuthStore from "@/stores/auth.store";

const authStore = useAuthStore();

defineProps<{ shrine: Shrine }>();

const isWishlisted = ref(false);
const isVisited = ref(false);

const toggleWishlist = (e: MouseEvent) => {
  e.stopPropagation();
  isWishlisted.value = !isWishlisted.value;
};

const toggleVisited = (e: MouseEvent) => {
  e.stopPropagation();
  isVisited.value = !isVisited.value;
};
</script>

<template>
  <div class="flex flex-col group">
    <!-- Image -->
    <div
      class="relative h-64 bg-stone-100 overflow-hidden mb-4 border-2 border-stone-200 hover:border-primary-500 hover:shadow-[0_0_0_4px_primary-500] transition-all duration-300"
    >
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

      <!-- Gradient overlay -->
      <div
        class="absolute inset-0 bg-gradient-to-t from-black/10 to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300"
      />
      <!-- Hover action row -->
      <div
        v-if="authStore.isAuthenticated"
        class="absolute bottom-0 left-0 right-0 p-3 flex justify-end opacity-0 group-hover:opacity-100 transition-opacity duration-300"
      >
        <div class="flex gap-2">
          <!-- Wishlist -->
          <button
            class="w-8 h-8 rounded-full bg-white/20 backdrop-blur-sm flex items-center justify-center text-white hover:bg-white/40 transition-colors"
            @click="toggleWishlist"
            v-cursor-hover
          >
            <i
              class="text-sm"
              :class="
                isWishlisted ? 'pi pi-heart-fill text-red-400' : 'pi pi-heart'
              "
            />
          </button>

          <!-- Visited -->
          <button
            class="w-8 h-8 rounded-full bg-white/20 backdrop-blur-sm flex items-center justify-center text-white hover:bg-white/40 transition-colors"
            @click="toggleVisited"
            v-cursor-hover
          >
            <i
              class="text-sm"
              :class="
                isVisited
                  ? 'pi pi-check-circle text-primary-300'
                  : 'pi pi-circle'
              "
            />
          </button>
        </div>
      </div>
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
