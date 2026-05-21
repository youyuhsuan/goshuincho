<template>
  <main class="flex-1 py-16 mx-auto">
    <h1 class="text-xl font-bold text-surface-900 dark:text-surface-0 mb-6">
      About Me
    </h1>
    <!-- Hero Section -->
    <div class="flex flex-col md:flex-row items-center gap-12">
      <div class="relative shrink-0">
        <div class="w-52 h-52 rounded-2xl overflow-hidden">
          <Image
            :src="profileImage"
            alt="You Yu-Hsuan"
            class="w-full h-full object-cover"
          />
        </div>
      </div>

      <div class="flex flex-col text-center md:text-left">
        <p class="text-sm font-medium text-primary uppercase tracking-widest">
          Frontend Developer
        </p>
        <h2
          class="text-5xl font-bold text-surface-900 dark:text-surface-0 leading-tight mb-3"
        >
          You Yu-Hsuan
        </h2>
        <p class="text-surface-500 text-md max-w-md leading-relaxed">
          Designer-turned Frontend Engineer | Creating interfaces that connect
        </p>

        <div class="flex gap-4 mt-2 justify-center md:justify-start">
          <a
            href="tel:0933305155"
            class="flex items-center gap-2 text-sm font-medium text-surface-600 hover:text-primary transition-colors"
          >
            <i class="pi pi-phone text-lg" />
            0933-305-155
          </a>
          <a
            href="mailto:stellayou0118@gmail.com"
            class="flex items-center gap-2 text-sm font-medium text-surface-600 hover:text-primary transition-colors"
          >
            <i class="pi pi-envelope text-lg" />
            stellayou0118@gmail.com
          </a>
          <a
            href="https://github.com/youyuhsuan"
            target="_blank"
            class="flex items-center gap-2 text-sm font-medium text-surface-600 hover:text-primary transition-colors"
          >
            <i class="pi pi-github text-lg" />
            GitHub
          </a>
          <a
            href="https://www.linkedin.com/in/youyuhsuan/"
            target="_blank"
            class="flex items-center gap-2 text-sm font-medium text-surface-600 hover:text-primary transition-colors"
          >
            <i class="pi pi-linkedin text-lg" />
            LinkedIn
          </a>
        </div>
      </div>
    </div>

    <div class="mt-16">
      <h2
        class="text-sm font-medium text-surface-400 uppercase tracking-widest mb-6"
      >
        Tech Stack
      </h2>
      <div class="flex flex-wrap gap-3">
        <span
          v-for="tech in techs"
          :key="tech"
          class="px-4 py-2 rounded-full text-sm font-medium bg-surface-100 dark:bg-surface-800 text-surface-700 dark:text-surface-200"
        >
          {{ tech }}
        </span>
      </div>
    </div>
  </main>
</template>

<script setup lang="ts">
import profileImage from "@/assets/images/profile.jpg";

const techs = ["Vue.js", "TypeScript", "Tailwind CSS", "Pinia"];
</script>
