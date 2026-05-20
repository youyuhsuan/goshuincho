<script setup lang="ts">
import Image from "primevue/image";
import IconGithubMark from "@/assets/icons/github.svg?component";
import IconLinkIn from "@/assets/images/linkin.png";
</script>

<template>
  <footer class="w-full mt-24 sm:mt-32">
    <!-- Subtle top border — thin brush stroke feel -->
    <div class="mx-auto max-w-7xl px-6 lg:px-8">
      <!-- Footer content -->
      <div
        class="flex flex-col sm:flex-row items-center justify-between gap-6 pb-10"
      >
        <!-- Copyright -->
        <p class="text-xs text-stone-400 tracking-wider order-2 sm:order-1">
          &copy;2026 YOU YU-HSUAN
          <span class="mx-1.5 text-stone-300">&middot;</span>
          All rights reserved.
        </p>

        <!-- Social links -->
        <div class="flex items-center gap-5 order-1 sm:order-2">
          <a
            href="https://github.com/youyuhsuan"
            class="group flex items-center gap-1.5 text-stone-400 hover:text-stone-600 transition-colors duration-300"
            target="_blank"
            rel="noopener noreferrer"
            aria-label="GitHub"
          >
            <IconGithubMark class="w-4 h-4" />
            <span class="text-xs tracking-wide">Github</span>
          </a>

          <span class="w-px h-3 bg-stone-200" />

          <a
            href="https://www.linkedin.com/in/youyuhsuan/"
            class="group flex items-center gap-1.5 text-stone-400 hover:text-stone-600 transition-colors duration-300"
            target="_blank"
            rel="noopener noreferrer"
            aria-label="LinkedIn"
          >
            <Image :src="IconLinkIn" class="w-4 h-4" alt="LinkedIn icon" />
            <span class="text-xs tracking-wide">LinkedIn</span>
          </a>
        </div>
      </div>
    </div>
  </footer>
</template>
