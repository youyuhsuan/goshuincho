<script setup lang="ts">
import { RouterView } from "vue-router";
// Components
import Menubar from "@/components/Menubar.vue";
import Footer from "@/components/Footer.vue";
</script>

<template>
  <Toast />
  <Menubar />
  <div class="min-h-screen flex flex-col">
    <RouterView />
    <Footer />
  </div>
</template>
