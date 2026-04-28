<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import AutoComplete from "primevue/autocomplete";
// Router
import { useRouter } from "vue-router";
// Composables
import useAsyncState from "@/composables/useAsyncState";
import useApiShrines from "@/composables/api/useApiShrines";
import useMessage from "@/composables/useMessage";
// Types
import type { Shrine, SuggestionShrine } from "@/types/shrinesType";
// Utils
import { formatAddress } from "@/utils/formatUI";
// Config
import ROUTE_CONFIGS from "@/config/routeConfig";

const { t } = useI18n();
const router = useRouter();
const { getShrineSuggestions, getFeaturedShrines } = useApiShrines();
const { showWarning } = useMessage();

const keyword = ref<{ shrine: string; location: string }>({
  shrine: "",
  location: "",
});
const location = ref<{ latitude: number; longitude: number } | null>(null);

const getUserLocation = () => {
  if (!navigator.geolocation) {
    showWarning(t("home.location.notSupported"));
    return;
  }
  navigator.geolocation.getCurrentPosition(
    (pos) => {
      const { latitude, longitude } = pos.coords;
      location.value = { latitude, longitude };
      keyword.value.location = t("home.location.currentLocation");
    },
    (error) => {
      console.error(error);
      showWarning(t("home.location.permissionDenied"));
    },
  );
};

const suggestionShrines = ref<SuggestionShrine[]>([]);
const suggest = useAsyncState(() =>
  getShrineSuggestions(keyword.value.shrine).then((r) => r.data),
);

const onSearchShrineSuggestions = async () => {
  if (!keyword.value.shrine) return;
  await suggest.execute();
  suggestionShrines.value = suggest.data.value || [];
};

const featuredTodayShrines = ref<Shrine[]>([]);
const featuredLoad = useAsyncState(() =>
  getFeaturedShrines().then((r) => r.data),
);

onMounted(async () => {
  await featuredLoad.execute();
  featuredTodayShrines.value = featuredLoad.data.value || [];
});
</script>

<template>
  <main class="w-full bg-stone-50" aria-label="homepage">
    <!-- Hero -->
    <section
      class="relative w-full overflow-hidden"
      style="height: min(70dvh, 640px)"
      :aria-label="t('home.ariaLabel.heroSection')"
    >
      <!-- Background layers (dual-tone depth) -->
      <div class="absolute inset-0">
        <!-- Base warm wash -->
        <div
          class="absolute inset-0 bg-gradient-to-b from-stone-100 via-primary-50/60 to-stone-100"
        />
        <!-- Centered radial warmth -->
        <div
          class="absolute inset-0 bg-[radial-gradient(ellipse_60%_50%_at_50%_45%,_theme(colors.primary.100/0.19),_transparent)]"
        />
      </div>

      <!-- Ambient glow blobs -->
      <div
        class="absolute top-1/4 right-[12%] w-72 h-72 rounded-full bg-primary-300/10 blur-3xl pointer-events-none"
      />
      <div
        class="absolute bottom-1/4 left-[8%] w-56 h-56 rounded-full bg-primary-200/15 blur-2xl pointer-events-none"
      />

      <!-- Hero content -->
      <div
        class="relative flex flex-col items-center justify-center h-full gap-3 px-4"
      >
        <h1
          class="text-4xl md:text-6xl font-light tracking-[0.15em] text-stone-700 text-center"
        >
          {{ t("common.title") }}
        </h1>

        <!-- Decorative mon-style divider -->
        <div class="flex items-center gap-3 my-2">
          <span class="h-px w-8 bg-stone-300" />
          <span class="w-1 h-1 rounded-full bg-primary-400" />
          <span class="h-px w-8 bg-stone-300" />
        </div>

        <p class="text-sm md:text-base text-stone-400 mb-8 tracking-wider">
          {{ t("home.hero.subTitle") }}
        </p>

        <!-- Search bar -->
        <InputGroup
          class="!w-full !max-w-[560px] !rounded-full !border !border-stone-200/80 !bg-white/85 !backdrop-blur-sm !shadow-[0_4px_24px_-8px_rgba(0,0,0,0.06)] !overflow-hidden transition-all duration-300 hover:!shadow-[0_8px_32px_-8px_rgba(255,88,65,0.12)] focus-within:!border-primary-300"
        >
          <!-- Location input -->
          <div class="relative flex items-center flex-1 min-w-0">
            <input
              v-model="keyword.location"
              class="w-full px-5 py-3.5 outline-none text-sm bg-transparent text-stone-700 placeholder:text-stone-300"
              type="text"
              :placeholder="t('home.placeholder.location')"
            />
            <Button
              class="!absolute !right-1 !w-9 !h-9 hover:!bg-primary-50 hover:!text-primary-500 !transition-colors"
              icon="pi pi-map-marker"
              severity="secondary"
              variant="text"
              rounded
              :aria-label="t('home.ariaLabel.getCurrentLocation')"
              @click="getUserLocation"
            />
          </div>

          <!-- Divider -->
          <div class="py-2.5">
            <div class="h-full border-r border-stone-200" />
          </div>

          <!-- Shrine autocomplete -->
          <AutoComplete
            v-model="keyword.shrine"
            :pt="{
              pcInputText: {
                root: {
                  class:
                    '!border-none !shadow-none !text-sm !bg-transparent !px-5 !py-3.5 !text-stone-700 placeholder:!text-stone-300',
                },
              },
            }"
            optionLabel="name"
            :placeholder="t('home.placeholder.shrine')"
            :suggestions="suggestionShrines"
            @complete="onSearchShrineSuggestions"
          />

          <!-- Search button -->
          <Button
            class="!rounded-none !px-6 !bg-primary-500 hover:!bg-primary-600 !border-none !transition-colors"
            :loading="suggest.isLoading.value"
            icon="pi pi-search"
            :aria-label="t('home.ariaLabel.searchShrine')"
          />
        </InputGroup>
      </div>
    </section>

    <!-- Shrine list -->
    <div class="px-6 md:px-16 lg:px-32">
      <section
        class="my-16 md:my-20"
        :aria-label="t('home.ariaLabel.featuredtodaySection')"
      >
        <!-- Section header -->
        <div class="mb-10 flex items-end justify-between flex-wrap gap-4">
          <div>
            <div class="flex items-center gap-3 mb-3">
              <span class="h-px w-6 bg-primary-400" />
              <span
                class="text-xs tracking-[0.25em] text-primary-500 uppercase font-medium"
              >
                {{ t("home.featuredToday.title") }}
              </span>
            </div>
            <h2
              class="text-2xl md:text-3xl font-light tracking-wider text-stone-700 mb-2"
            >
              {{ t("home.featuredToday.title") }}
            </h2>
            <p class="text-sm text-stone-400 tracking-wide">
              {{ t("home.featuredToday.description") }}
            </p>
          </div>
        </div>

        <!-- Loading skeleton -->
        <div
          v-if="featuredLoad.isLoading.value"
          class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-12"
        >
          <div v-for="n in 3" :key="n" class="flex flex-col">
            <Skeleton class="mb-4 !rounded-lg" height="17rem" />
            <Skeleton class="mb-2 !rounded" height="1.25rem" width="60%" />
            <Skeleton class="!rounded" height="0.875rem" width="40%" />
          </div>
        </div>

        <!-- Shrine cards -->
        <div
          v-else
          class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-12"
        >
          <!-- Empty state -->
          <div
            v-if="featuredTodayShrines.length === 0"
            class="col-span-full flex flex-col items-center justify-center py-24 text-stone-300"
          >
            <i class="pi pi-inbox text-5xl mb-4 opacity-50" />
            <p class="tracking-wider text-sm">{{ t("common.empty") }}</p>
          </div>

          <article
            v-for="shrine in featuredTodayShrines"
            :key="shrine.id"
            class="flex flex-col cursor-pointer group outline-none focus-visible:ring-2 focus-visible:ring-primary-400 focus-visible:ring-offset-4 rounded-lg"
            tabindex="0"
            @click="router.push(ROUTE_CONFIGS.SHRINES + `/${shrine.id}`)"
            @keydown.enter="
              router.push(ROUTE_CONFIGS.SHRINES + `/${shrine.id}`)
            "
          >
            <!-- Image -->
            <div
              class="relative h-64 bg-stone-100 rounded-lg overflow-hidden mb-4"
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

              <!-- Hover overlay -->
              <div
                class="absolute inset-0 bg-gradient-to-t from-black/20 to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300"
              />
            </div>

            <!-- Info -->
            <div class="px-1">
              <h3
                class="text-base font-medium text-stone-700 tracking-wide transition-colors group-hover:text-primary-500"
              >
                {{ shrine.name }}
              </h3>
              <p class="text-xs text-stone-400 mt-1.5 tracking-wide">
                {{ formatAddress(shrine) }}
              </p>
            </div>
          </article>
        </div>
      </section>
    </div>
  </main>
</template>
