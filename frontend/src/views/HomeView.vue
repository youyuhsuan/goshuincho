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
// Images
import TwelvePetalFlower from "@/assets/images/twelvePetalFlower.svg?component";

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
  <main class="w-full bg-stone-50 relative" aria-label="homepage">
    <!-- Hero -->
    <section
      class="hero-section w-full z-10"
      style="height: min(70dvh, 640px)"
      :aria-label="t('home.ariaLabel.heroSection')"
    >
      <!-- Hero content -->
      <div
        class="flex flex-col justify-center h-full gap-3 px-6 md:px-16 lg:px-32 z-10"
      >
        <!-- Title brush-reveal -->
        <h1
          class="title-reveal text-8xl md:text-6xl font-light tracking-0.5 text-stone-700"
          v-cursor-hover
        >
          {{ t("common.title") }}
        </h1>

        <p
          class="subtitle-reveal text-sm md:text-base text-stone-400 tracking-wider"
        >
          {{ t("home.hero.subTitle") }}
        </p>

        <!-- Search bar -->
        <div class="search-reveal w-full">
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
      </div>
    </section>

    <!-- Goshuin background stamp -->
    <TwelvePetalFlower
      class="twelve-petal-flower-1"
      aria-hidden="true"
      v-cursor-stamp
    />
    <TwelvePetalFlower
      class="twelve-petal-flower-2"
      aria-hidden="true"
      v-cursor-stamp
    />

    <!-- Shrine list -->
    <div class="px-6 md:px-16 lg:px-32">
      <section
        class="my-16 md:my-20"
        :aria-label="t('home.ariaLabel.featuredtodaySection')"
      >
        <!-- Section header -->
        <div class="mb-10 flex items-end justify-between flex-wrap gap-4">
          <div class="relative">
            <div class="relative z-10">
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
            v-cursor-hover
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

<style scoped>
.twelve-petal-flower-1 {
  position: absolute;
  display: block;
  width: 10rem;
  height: 10rem;
  color: var(--p-primary-500);
  top: 25%;
  right: 0%;
  translate: -50% -50%;
  opacity: 0;
  animation:
    stamp-fade-in 1.6s ease-out 0.5s forwards,
    shrine-float 4s ease-in-out 0.5s infinite,
    continuous-rotate-1 20s linear 0.5s infinite;
}

.twelve-petal-flower-2 {
  position: absolute;
  display: block;
  width: 30rem;
  height: 30rem;
  color: var(--p-primary-500);
  top: 45%;
  right: 0%;
  translate: -50% -50%;
  opacity: 0;
  animation:
    stamp-fade-in 2s ease-out 0.9s forwards,
    shrine-float 5s ease-in-out 0.9s infinite,
    continuous-rotate-2 20s linear 0.9s infinite;
}

@keyframes stamp-fade-in {
  from { opacity: 0; }
  to   { opacity: 1; }
}

@keyframes continuous-rotate-1 {
  from {
    transform: rotate(-90deg);
  }
  to {
    transform: rotate(270deg);
  }
}

@keyframes continuous-rotate-2 {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}

@keyframes shrine-float {
  0%,
  100% {
    transform: translateY(0px) rotate(0deg);
  }
  50% {
    transform: translateY(-20px) rotate(180deg);
  }
}

.title-reveal {
  animation: brush-reveal 0.9s ease-out 1.1s both;
}

.divider-reveal {
  animation: brush-reveal 0.7s ease-out 1.28s both;
}

.subtitle-reveal {
  animation: brush-reveal 0.7s ease-out 1.42s both;
}

.search-reveal {
  animation: brush-reveal 0.8s ease-out 1.58s both;
}

@keyframes brush-reveal {
  0% {
    opacity: 0;
    transform: translateY(13px);
    filter: blur(5px);
  }
  100% {
    opacity: 1;
    transform: translateY(0);
    filter: blur(0);
  }
}
</style>
