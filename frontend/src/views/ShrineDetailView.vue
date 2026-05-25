<script setup lang="ts">
import { ref, onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";
// I18n
import { useI18n } from "vue-i18n";
// Composables
import useApiShrines from "@/composables/api/useApiShrines";
import useAsyncState from "@/composables/useAsyncState";
import useAsyncAction from "@/composables/useAsyncAction";
// Stores
import useAuthStore from "@/stores/auth.store";
// Utils
import { formatAddress } from "@/utils/formatUI";
// Config
import ROUTE_CONFIGS from "@/config/routeConfig";

const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const authStore = useAuthStore();
const { getShrine } = useApiShrines();

const shrineId = route.params.id as string;

const shrineState = useAsyncState(() =>
  getShrine(shrineId).then((r) => r.data),
);

const visitDate = ref<Date | null>(null);
const notes = ref("");
const isVisited = ref(false);
const isWishlisted = ref(false);

const saveRecord = useAsyncAction(
  async () => {
    // TODO: wire to POST /api/users/:id/collections when backend is ready
    await new Promise<void>((resolve) => setTimeout(resolve, 500));
  },
  { successMessage: t("shrines.detail.saveSuccess") },
);

onMounted(() => shrineState.execute());
</script>

<template>
  <main class="w-full pb-32" :aria-label="t('shrines.detail.ariaLabel.page')">
    <!-- Loading -->
    <template v-if="shrineState.isLoading.value">
      <div class="px-6 md:px-16 lg:px-32 pt-10 space-y-6">
        <Skeleton height="1rem" width="8rem" />
        <Skeleton height="4rem" width="20rem" />
        <Skeleton height="0.75rem" width="12rem" />
      </div>
      <div class="mt-10">
        <Skeleton class="w-full" height="26rem" />
      </div>
    </template>

    <!-- Not Found -->
    <template v-else-if="!shrineState.data.value">
      <div
        class="flex flex-col items-center justify-center py-40 text-stone-300"
      >
        <i class="pi pi-inbox text-5xl mb-4 opacity-40" />
        <p class="tracking-widest text-sm uppercase">{{ t("common.empty") }}</p>
        <button
          class="mt-8 text-xs text-stone-400 tracking-[0.2em] uppercase hover:text-primary-500 transition flex items-center gap-2"
          @click="router.push(ROUTE_CONFIGS.SHRINES)"
          v-cursor-hover
        >
          <i class="pi pi-arrow-left text-[10px]" />
          {{ t("shrines.detail.backToExplore") }}
        </button>
      </div>
    </template>

    <!-- Content -->
    <template v-else>
      <!--  Header  -->
      <section class="w-full border-4 border-primary-500 p-0.5">
        <div
          class="border border-primary-500 py-2 flex flex-col gap-2 items-center justify-center"
        >
          <h1
            class="text-xl md:text-4xl tracking-[0.4em] font-medium text-primary-500"
          >
            {{ shrineState.data.value.name }}
          </h1>
          <p class="text-stone-400 text-xs tracking-[0.3em] uppercase">
            {{ formatAddress(shrineState.data.value) }}
          </p>
        </div>
      </section>

      <!-- Hero Image -->
      <section
        class="relative h-72 md:h-[28rem] w-full overflow-hidden bg-stone-100 mb-16"
      >
        <img
          v-if="shrineState.data.value.imageUrl"
          class="w-full h-full object-cover"
          :src="shrineState.data.value.imageUrl"
          :alt="shrineState.data.value.name"
        />
        <div
          v-else
          class="w-full h-full flex items-center justify-center text-stone-200"
        >
          <i class="pi pi-image text-7xl" />
        </div>
        <div
          class="absolute inset-0 bg-gradient-to-t from-black/20 via-transparent to-transparent"
        />

        <!-- Action buttons -->
        <div
          v-if="authStore.isAuthenticated"
          class="absolute bottom-4 right-4 flex items-center gap-3 shrink-0"
        >
          <button
            class="flex items-center gap-2 px-5 py-2 text-xs tracking-widest uppercase border transition-colors"
            :class="
              isWishlisted
                ? 'border-red-400 text-red-500 bg-red-50 dark:bg-red-950/20'
                : 'border-stone-300 text-stone-500 hover:border-red-400 hover:text-red-500 dark:border-stone-700 dark:text-stone-400'
            "
            @click="isWishlisted = !isWishlisted"
            v-cursor-hover
          >
            <i :class="isWishlisted ? 'pi pi-heart-fill' : 'pi pi-heart'" />
            <span>{{ t("shrines.detail.wishlist") }}</span>
          </button>
          <button
            class="flex items-center gap-2 px-5 py-2 text-xs tracking-widest uppercase border transition-colors"
            :class="
              isVisited
                ? 'border-primary-400 text-primary-600 bg-primary-50 dark:bg-primary-950/20'
                : 'border-stone-300 text-stone-500 hover:border-primary-400 hover:text-primary-600 dark:border-stone-700 dark:text-stone-400'
            "
            @click="isVisited = !isVisited"
            v-cursor-hover
          >
            <i :class="isVisited ? 'pi pi-check-circle' : 'pi pi-circle'" />
            <span>{{ t("shrines.detail.visited") }}</span>
          </button>
        </div>
      </section>

      <!-- ── Info  ── -->
      <section class="px-6 md:px-16 lg:px-32">
        <!-- category -->
        <div class="flex flex-wrap gap-2 mb-6">
          <template
            v-for="benefit in shrineState.data.value.benefits"
            :key="benefit"
          >
            <span
              class="bg-primary-200 dark:bg-primary-900/20 text-stone-500 dark:text-primary-400 py-1 px-3 text-xs tracking-wider uppercase"
            >
              {{ benefit }}
            </span>
          </template>
        </div>

        <div class="grid grid-cols-1 lg:grid-cols-3 gap-12">
          <!-- Description -->
          <div class="lg:col-span-2">
            <div class="flex items-center gap-3 mb-6">
              <span class="h-px w-6 bg-primary-400" />
              <span
                class="text-xs tracking-[0.25em] text-primary-500 uppercase font-medium"
              >
                {{ t("shrines.detail.info") }}
              </span>
            </div>
            <p
              v-if="shrineState.data.value.description"
              class="text-stone-600 dark:text-stone-300 leading-[2] tracking-wide text-sm"
            >
              {{ shrineState.data.value.description }}
            </p>
            <p v-else class="text-stone-300 text-sm">—</p>
          </div>

          <!-- Details sidebar -->
          <div
            class="flex flex-col gap-7 text-sm border-l border-stone-100 dark:border-stone-800 pl-8 lg:pl-12"
          >
            <div v-if="shrineState.data.value.openingHours">
              <p
                class="text-[10px] text-stone-400 tracking-[0.3em] uppercase mb-2"
              >
                {{ t("shrines.detail.openingHours") }}
              </p>
              <p class="text-stone-700 dark:text-stone-300 tracking-wide">
                {{ shrineState.data.value.openingHours }}
              </p>
            </div>
            <div v-if="shrineState.data.value.access">
              <p
                class="text-[10px] text-stone-400 tracking-[0.3em] uppercase mb-2"
              >
                {{ t("shrines.detail.access") }}
              </p>
              <p
                class="text-stone-700 dark:text-stone-300 tracking-wide leading-relaxed"
              >
                {{ shrineState.data.value.access }}
              </p>
            </div>
            <div v-if="shrineState.data.value.founded">
              <p
                class="text-[10px] text-stone-400 tracking-[0.3em] uppercase mb-2"
              >
                {{ t("shrines.detail.founded") }}
              </p>
              <p class="text-stone-700 dark:text-stone-300 tracking-wide">
                {{ shrineState.data.value.founded }}
              </p>
            </div>
            <div v-if="shrineState.data.value.enshrineDeity?.length">
              <p
                class="text-[10px] text-stone-400 tracking-[0.3em] uppercase mb-2"
              >
                {{ t("shrines.detail.enshrineDeity") }}
              </p>
              <ul class="flex flex-col gap-1">
                <li
                  v-for="deity in shrineState.data.value.enshrineDeity"
                  :key="deity"
                  class="text-stone-700 dark:text-stone-300 text-sm tracking-wide leading-relaxed"
                >
                  {{ deity }}
                </li>
              </ul>
            </div>
            <div v-if="shrineState.data.value.address">
              <p
                class="text-[10px] text-stone-400 tracking-[0.3em] uppercase mb-2"
              >
                {{ t("shrines.detail.address") }}
              </p>
              <p
                class="text-stone-700 dark:text-stone-300 tracking-wide leading-relaxed text-sm"
              >
                {{ shrineState.data.value.address }}
              </p>
            </div>
            <div v-if="shrineState.data.value.website">
              <p
                class="text-[10px] text-stone-400 tracking-[0.3em] uppercase mb-2"
              >
                {{ t("shrines.detail.website") }}
              </p>
              <a
                :href="shrineState.data.value.website"
                target="_blank"
                rel="noopener noreferrer"
                class="text-primary-500 hover:opacity-70 transition text-xs tracking-wide underline underline-offset-4 break-all"
              >
                {{ shrineState.data.value.website }}
              </a>
            </div>
          </div>
        </div>
      </section>
    </template>
  </main>
</template>
