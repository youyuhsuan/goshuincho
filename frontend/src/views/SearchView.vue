<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
// I18n
import { useI18n } from "vue-i18n";
// Components
import ShrineCard from "@/components/common/ShrineCard.vue";
// Composables
import useAsyncPaginatedState from "@/composables/useAsyncPaginatedState";
import useApiShrines from "@/composables/api/useApiShrines";
// Types
import type { SearchShrinesParams } from "@/types/shrinesType";
// Config
import ROUTE_CONFIGS from "@/config/routeConfig";

const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const { getShrines } = useApiShrines();

const hasSearchQuery = computed(
  () => !!(route.query.shrine || route.query.latitude || route.query.longitude),
);

const shrine = useAsyncPaginatedState(
  async (page?: number) => await getShrines(buildParams(page)),
);

const buildParams = (page: number = 1): SearchShrinesParams => ({
  page,
  pageSize: 3,
  shrine: (route.query.shrine as string) || undefined,
  latitude: route.query.latitude ? Number(route.query.latitude) : undefined,
  longitude: route.query.longitude ? Number(route.query.longitude) : undefined,
});

const sentinelRef = ref<HTMLElement | null>(null);
const pagination = shrine.pagination;
const isLoadingMore = shrine.isLoadingMore;

const loadMore = async () => {
  if (!shrine.pagination.value?.hasNextPage) return;
  const nextPage = (shrine.pagination.value.currentPage ?? 1) + 1;
  await shrine.executeMore(nextPage);
};

onMounted(async () => {
  await shrine.execute();

  const observer = new IntersectionObserver(
    (entries) => {
      if (entries[0].isIntersecting) loadMore();
    },
    { threshold: 0.1 },
  );

  if (sentinelRef.value) observer.observe(sentinelRef.value);
  onUnmounted(() => observer.disconnect());
});
</script>

<template>
  <main
    class="w-full px-6 md:px-16 lg:px-32 pt-10 pb-24"
    :aria-label="t('shrines.ariaLabel.page')"
  >
    <!-- Section header -->
    <div class="mb-10">
      <div class="flex items-center gap-3 mb-3">
        <span class="h-px w-6 bg-primary-400" />
        <span
          class="text-xs tracking-[0.25em] text-primary-500 uppercase font-medium"
        >
          {{ t("shrines.pageTitle") }}
        </span>
      </div>
      <h1
        class="text-2xl md:text-3xl font-light tracking-wider text-stone-700 mb-2"
      >
        {{
          route.query.shrine
            ? t("shrines.search.resultsFor", { query: route.query.shrine })
            : t("shrines.pageTitle")
        }}
      </h1>
    </div>

    <!-- Loading skeleton -->
    <div
      v-if="shrine.isLoading.value"
      class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-12"
    >
      <div v-for="n in 6" :key="n" class="flex flex-col">
        <Skeleton class="mb-4" height="17rem" />
        <Skeleton class="mb-2" height="1.25rem" width="60%" />
        <Skeleton height="0.875rem" width="40%" />
      </div>
    </div>

    <!-- Shrine cards -->
    <div
      v-else
      class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-12"
    >
      <!-- Empty state -->
      <div
        v-if="!shrine.data.value?.length"
        class="col-span-full flex flex-col items-center justify-center py-24 text-stone-300"
      >
        <i class="pi pi-inbox text-5xl mb-4 opacity-50" />
        <p class="tracking-wider text-sm">
          {{
            hasSearchQuery ? t("shrines.search.noResults") : t("common.empty")
          }}
        </p>
      </div>

      <article
        v-for="shrine in shrine.data.value ?? []"
        :key="shrine.id"
        class="cursor-pointer outline-none focus-visible:ring-2 focus-visible:ring-primary-400 focus-visible:ring-offset-4"
        tabindex="0"
        :aria-label="t('shrines.ariaLabel.shrineCard', { name: shrine.name })"
        @click="router.push(`${ROUTE_CONFIGS.SEARCH}/${shrine.id}`)"
        @keydown.enter="router.push(`${ROUTE_CONFIGS.SEARCH}/${shrine.id}`)"
        v-cursor-hover
      >
        <ShrineCard :shrine="shrine" />
      </article>
    </div>

    <!-- Load more -->
    <div
      v-if="!shrine.isLoading.value && pagination?.hasNextPage"
      class="flex justify-center mt-16"
    >
      <button
        class="px-8 py-3 text-sm tracking-widest uppercase text-primary-500 border border-primary-300 rounded-full hover:bg-primary-50 transition-colors duration-200 disabled:opacity-40"
        :disabled="isLoadingMore"
        @click="loadMore"
      >
        <span v-if="isLoadingMore">
          <i class="pi pi-spinner pi-spin mr-2" />
        </span>
        {{ t("common.loadMore") }}
      </button>
    </div>

    <div ref="sentinelRef" class="h-1" aria-hidden="true" />
  </main>
</template>
