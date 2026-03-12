<template>
  <main class="flex flex-1">
    <section
      class="flex flex-1 lg:flex-[2.5] flex-col items-center justify-center px-6 sm:px-8 lg:px-12 py-8"
    >
      <!-- title -->
      <div class="w-full max-w-md mb-6 sm:mb-7 lg:mb-8">
        <h1 class="text-2xl font-bold mb-0.5">{{ authText.title }}</h1>
        <p class="text-sm text-gray-600">{{ authText.description }}</p>
      </div>

      <!-- Form -->
      <section class="w-full max-w-md mb-4">
        <login-form v-if="isLogin" />
        <register-form v-else v-model:isLogin="isLogin" />
      </section>

      <!-- Toggle button -->
      <div class="text-sm mb-6">
        <span class="text-gray-600">{{ authText.question }}</span>
        <button
          class="ml-1 text-blue-600 hover:text-blue-800 hover:underline font-medium transition-colors"
          :aria-label="`Switch to ${isLogin ? 'sign up' : 'sign in'} mode`"
          @click="toggleMode"
        >
          {{ authText.action }}
        </button>
      </div>

      <!-- Divider -->
      <Divider align="center" type="solid" class="w-full max-w-md mb-6">
        <b>or</b>
      </Divider>

      <!-- OAuth authentication button -->
      <google-o-auth-button class="w-full max-w-md" />
    </section>

    <!-- Background image -->
    <section class="hidden lg:flex lg:flex-[3.5] bg-surface-100"></section>
  </main>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";

// Primevue
import Divider from "primevue/divider";

// Copoments
import LoginForm from "@/components/auth/LoginForm.vue";
import RegisterForm from "@/components/auth/RegisterForm.vue";
import GoogleOAuthButton from "@/components/auth/GoogleOAuthButton.vue";

const isLogin = ref<boolean>(true);

// Toggle between login and register modes
const toggleMode = (): void => {
  isLogin.value = !isLogin.value;
};

// UI controller
// Computed property for dynamic text based on current auth mode
const authText = computed(() => ({
  title: isLogin.value ? "Welcome Back" : "Get Started Now",
  description: isLogin.value
    ? "Sign in to your account to continue."
    : "Create an account to get started.",
  question: isLogin.value
    ? "Don't have an account?"
    : "Already have an account?",
  action: isLogin.value ? "Sign up" : "Sign in",
}));
</script>
