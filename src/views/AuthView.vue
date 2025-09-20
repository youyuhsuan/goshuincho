<template>
  <main class="flex flex-col items-center min-h-screen p-6">
    <!-- title -->
    <div class="mb-8">
      <h1 class="text-3xl font-bold text-center">{{ authText.title }}</h1>
    </div>

    <!-- Form -->
    <section class="w-full max-w-md">
      <login-form v-if="isLogin" class="mb-4" />
      <register-form v-else class="mb-4" />
    </section>

    <!-- Toggle button -->
    <div class="text-center text-sm mb-6">
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
  </main>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'

// Primevue
import Divider from 'primevue/divider'

// Copoments
import LoginForm from '@/components/auth/LoginForm.vue'
import RegisterForm from '@/components/auth/RegisterForm.vue'
import GoogleOAuthButton from '@/components/auth/GoogleOAuthButton.vue'

const isLogin = ref<boolean>(true)

// Toggle between login and register modes
const toggleMode = (): void => {
  isLogin.value = !isLogin.value
}

// UI controller
// Computed property for dynamic text based on current auth mode
const authText = computed(() => ({
  title: isLogin.value ? 'Sign in' : 'Sign up',
  question: isLogin.value ? "Don't have an account?" : 'Already have an account?',
  action: isLogin.value ? 'Sign up' : 'Sign in',
}))
</script>
