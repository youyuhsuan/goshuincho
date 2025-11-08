<template>
  <Nav />
  <RouterView />
  <Footer />
</template>

<script setup lang="ts">
import { RouterView } from "vue-router";
import Nav from "@/components/Nav.vue";
import Footer from "@/components/Footer.vue";
</script>
