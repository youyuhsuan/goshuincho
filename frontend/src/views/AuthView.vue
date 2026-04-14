<script setup lang="ts">
import { ref } from "vue";
// Primevue
import Divider from "primevue/divider";
// Copoments
import LoginForm from "@/components/auth/LoginForm.vue";
import RegisterForm from "@/components/auth/RegisterForm.vue";
import GoogleOAuthButton from "@/components/auth/GoogleOAuthButton.vue";
// Type
import type { AuthMode } from "@/types/authType";

const authMode = ref<AuthMode>("login");

// Toggle between login and register modes
const toggleMode = () => {
  authMode.value = authMode.value === "login" ? "register" : "login";
};
</script>

<template>
  <main class="flex flex-1">
    <div
      class="flex flex-1 lg:flex-[2.5] flex-col items-center justify-center px-6 sm:px-8 lg:px-12 py-8"
    >
      <!-- title -->
      <div class="w-full max-w-md mb-6 sm:mb-7 lg:mb-8">
        <h1 class="text-2xl font-bold mb-0.5">
          {{ $t(`auth.${authMode}.title`) }}
        </h1>
        <p class="text-sm text-gray-600">
          {{ $t(`auth.${authMode}.description`) }}
        </p>
      </div>

      <!-- Form -->
      <section class="w-full max-w-md mb-4">
        <login-form v-if="authMode === 'login'" />
        <register-form v-else v-model:authMode="authMode" />
      </section>

      <!-- Toggle button -->
      <div class="text-sm mb-6">
        <span class="text-gray-600">
          {{ $t(`auth.${authMode}.question`) }}
        </span>
        <button
          class="ml-1 text-primary-600 hover:text-primary-800 hover:underline font-medium transition-colors"
          :aria-label="$t(`auth.${authMode}.ariaLabel`)"
          @click="toggleMode"
        >
          {{ $t(`auth.${authMode}.action`) }}
        </button>
      </div>

      <!-- Divider -->
      <Divider align="center" type="solid" class="w-full max-w-md mb-6">
        <b> {{ $t("common.divider") }} </b>
      </Divider>

      <!-- OAuth authentication button -->
      <google-o-auth-button class="w-full max-w-md" />
    </div>

    <!-- Background image -->
    <section class="hidden lg:flex lg:flex-[3.5] bg-surface-100"></section>
  </main>
</template>
