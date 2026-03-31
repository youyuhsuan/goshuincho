<script setup lang="ts">
import { ref } from "vue";
// Route
import { useRouter } from "vue-router";
// Primevue
import FloatLabel from "primevue/floatlabel";
import Checkbox from "primevue/checkbox";
import type { FormSubmitEvent } from "@primevue/forms";
import { zodResolver } from "@primevue/forms/resolvers/zod";
// Zod
import { z } from "zod";
// Composables
import useMessage from "@/composables/useMessage";
// Utils
import generateFieldIds, { type FieldIds } from "@/utils/generateFieldIds";
import type { LoginRequest } from "@/types/authType";
// Stores
import useAuthStore from "@/stores/auth.store";
// Config
import ROUTE_CONFIGS from "@/config/routeConfig";

const isLoading = ref<boolean>(false);
// Remenber check
const checked = ref<boolean>(false);

const fieldIds = ref<FieldIds>(
  generateFieldIds(["email", "password", "checked"]),
);
const initialValues = ref<LoginRequest>({
  email: "",
  password: "",
});

const { showWarning } = useMessage();
const { login } = useAuthStore();
const router = useRouter();

const resolver = zodResolver(
  z.object({
    email: z.email(),
    password: z.string(),
  }),
);

const onFormSubmit = async (e: FormSubmitEvent) => {
  isLoading.value = true;
  // Prevent the browser's default form submission behavior
  e.originalEvent.preventDefault();

  // Check if all form fields have passed validation
  if (e.valid) {
    try {
      // Extract form values and submit to the login store
      await login(e.values as LoginRequest);
      // Reset all form fields to their initial state
      e.reset();
      router.push(ROUTE_CONFIGS.HOME);
    } catch (error: unknown) {
      if (typeof error === "string") showWarning(error);
    } finally {
      isLoading.value = false;
    }
  }
};
</script>

<template>
  <Form
    v-slot="$form"
    :initialValues="initialValues"
    :resolver="resolver"
    :validateOnValueUpdate="false"
    :validateOnBlur="true"
    class="flex flex-col gap-6.5 w-full"
    @submit="onFormSubmit"
  >
    <!-- Email -->
    <div class="flex flex-col gap-1">
      <FloatLabel>
        <InputText
          :id="fieldIds.email"
          :invalid="$form.email?.invalid"
          name="email"
          type="text"
          autocomplete="email"
          fluid
        />
        <label :for="fieldIds.email">
          {{ $t("auth.login.field.email") }}
        </label>
      </FloatLabel>
      <Message
        v-if="$form.email?.invalid"
        severity="error"
        size="small"
        variant="simple"
      >
        {{ $form.email.error.message }}
      </Message>
    </div>

    <!-- Password -->
    <div class="flex flex-col gap-1">
      <FloatLabel>
        <!--
          :feedback // Disable password strength indicator
          :inputProps // Pass native HTML attributes to input
          toggleMask // Enable show/hide password toggle button
          fluid // Make component width fill parent container
        -->
        <Password
          :id="fieldIds.password"
          :invalid="$form.password?.invalid"
          name="password"
          autocomplete="password"
          :feedback="false"
          :inputProps="{ autocomplete: 'current-password' }"
          toggleMask
          fluid
        />
        <label :for="fieldIds.password">
          {{ $t("auth.login.field.password") }}
        </label>
      </FloatLabel>
      <Message
        v-if="$form.password?.invalid"
        severity="error"
        size="small"
        variant="simple"
      >
        <ul class="my-0 px-4 flex flex-col gap-1">
          <li v-for="(error, index) of $form.password.errors" :key="index">
            {{ error.message }}
          </li>
        </ul>
      </Message>
    </div>

    <div class="flex mb-5 text-xs">
      <!-- Remember me -->
      <div class="flex items-center gap-x-1">
        <Checkbox
          v-model="checked"
          :inputId="fieldIds.checked"
          size="small"
          binary
        />
        <label :for="fieldIds.checked" class="text-gray-600">
          {{ $t("auth.login.rememberMe") }}
        </label>
      </div>

      <!-- Forgot Password -->
      <div class="ml-auto">
        <a href="#" class="text-blue-600 hover:text-blue-800 hover:underline">
          {{ $t("auth.login.forgotPassword") }}
        </a>
      </div>
    </div>

    <!-- Submit Button -->
    <Button
      type="submit"
      severity="secondary"
      :label="$t('auth.login.submit')"
      :loading="isLoading"
      :disabled="$form.valid === false"
    />
  </Form>
</template>
