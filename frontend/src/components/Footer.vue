<template>
  <footer class="mx-auto max-w-7xl px-6 lg:px-8 mt-2 w-full sm:mt-32 lg:mt-40">
    <!-- Social media links -->
    <div class="flex justify-end space-x-6">
      <a
        href="https://github.com/youyuhsuan"
        class="text-gray-600 hover:text-gray-900 text-sm font-medium transition-colors duration-200 flex items-center gap-2"
        target="_blank"
        rel="noopener noreferrer"
        aria-label="GitHub"
      >
        <IconGithubMark class="w-4 h-4" />
        <span>Github</span>
      </a>
      <a
        href="https://www.linkedin.com/in/youyuhsuan/"
        class="text-gray-600 hover:text-gray-900 text-sm font-medium transition-colors duration-200 flex items-center gap-2"
        target="_blank"
        rel="noopener noreferrer"
        aria-label="LinkedIn"
      >
        <Image :src="IconLinkIn" class="w-4 h-4" alt="LinkedIn icon" />
        <span>LinkedIn</span>
      </a>
    </div>

    <!-- Copyright -->
    <div class="flex flex-wrap items-end justify-between gap-x-6">
      <span class="text-gray-600 text-sm block mb-4">
        ©2025 YOU YU-HSUAN
      </span>
    </div>
  </footer>
</template>

<script setup lang="ts">
import Image from "primevue/image";
import IconGithubMark from "@/assets/icons/github.svg?component";
import IconLinkIn from "@/assets/images/linkin.png";
</script>
