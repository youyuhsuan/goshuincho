<script setup lang="ts">
import { RouterView } from "vue-router";
// Components
import Menubar from "@/components/Menubar.vue";
import Footer from "@/components/Footer.vue";
</script>

<template>
  <Menubar />
  <RouterView />
  <Footer />
</template>
