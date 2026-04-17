<script setup lang="ts">
import { ref } from "vue";
// Primevue
import { zodResolver } from "@primevue/forms/resolvers/zod";
import type { FormSubmitEvent } from "@primevue/forms";
// Zod
import { z } from "zod";
// i18n
import { useI18n } from "vue-i18n";
// Composables
import useMessage from "@/composables/useMessage";
import useApiAuth from "@/composables/api/useApiAuth";
// Utils
import generateFieldIds, { type FieldIds } from "@/utils/generateFieldIds";
// Type
import type {
  AuthMode,
  RegisterFormData,
  RegisterRequest,
} from "@/types/authType";

const { t } = useI18n();

const isLoading = ref<boolean>(false);
const fieldIds = ref<FieldIds>(
  generateFieldIds(["name", "password", "confirmPassword"]),
);
const initialValues = ref<RegisterFormData>({
  email: "",
  name: "",
  password: "",
  confirmPassword: "",
});

const authMode = defineModel<AuthMode>("authMode", {
  required: true,
});

const { registerUser } = useApiAuth();
const { showWarning } = useMessage();

const resolver = zodResolver(
  z
    .object({
      name: z
        .string()
        .min(1, { message: t("auth.register.validation.name.min") })
        .max(100, { message: t("auth.register.validation.name.max") }),
      email: z.email(),
      password: z
        .string()
        .min(6, { message: t("auth.register.validation.password.min") })
        .max(500, { message: t("auth.register.validation.password.max") })
        .refine((value) => /[a-z]/.test(value), {
          message: t("auth.register.validation.password.lowercase"),
        })
        .refine((value) => /[A-Z]/.test(value), {
          message: t("auth.register.validation.password.uppercase"),
        })
        .refine((value) => /\d/.test(value), {
          message: t("auth.register.validation.password.number"),
        }),
      confirmPassword: z.string(),
    })
    .refine((data) => data.password === data.confirmPassword, {
      message: t("auth.register.validation.confirmPassword.match"),
      path: ["confirmPassword"],
    }),
);

const onFormSubmit = async (e: FormSubmitEvent) => {
  // Prevent the browser's default form submission behavior
  e.originalEvent.preventDefault();

  // Check if all form fields have passed validation
  if (e.valid) {
    isLoading.value = true;
    try {
      // Extract form values and submit to the login API
      const { confirmPassword: _, ...registerData } =
        e.values as RegisterFormData;
      await registerUser(registerData as RegisterRequest);

      // Reset all form fields to their initial state
      e.reset();

      // Switch to login mode after successful registration
      authMode.value = "login";
    } catch (error: unknown) {
      console.error("Registration error:", error);
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
    class="flex flex-col gap-6.5 w-full"
    :validateOnValueUpdate="false"
    :validateOnBlur="true"
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
          {{ $t("auth.register.field.email") }}
        </label>
      </FloatLabel>
      <Message
        v-if="$form.email?.invalid"
        severity="error"
        size="small"
        variant="simple"
        class="text-xs"
      >
        {{ $form.email.error.message }}
      </Message>
    </div>

    <!-- Name -->
    <div class="flex flex-col gap-1">
      <FloatLabel>
        <InputText
          :id="fieldIds.name"
          :invalid="$form.name?.invalid"
          name="name"
          type="text"
          autocomplete="name"
          fluid
        />
        <label :for="fieldIds.name">
          {{ $t("auth.register.field.name") }}
        </label>
      </FloatLabel>
      <Message
        v-if="$form.name?.invalid"
        severity="error"
        size="small"
        variant="simple"
        class="text-xs"
      >
        {{ $form.name.error.message }}
      </Message>
    </div>

    <!-- Password -->
    <div class="flex flex-col gap-1">
      <FloatLabel>
        <Password
          :id="fieldIds.password"
          :invalid="$form.password?.invalid"
          name="password"
          :feedback="false"
          :inputProps="{ autocomplete: 'new-password' }"
          toggleMask
          fluid
        />
        <label :for="fieldIds.password">
          {{ $t("auth.register.field.password") }}
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

    <!-- Confirm Password -->
    <div class="flex flex-col gap-1 mb-5">
      <FloatLabel>
        <Password
          :id="fieldIds.confirmPassword"
          :invalid="$form.confirmPassword?.invalid"
          name="confirmPassword"
          :feedback="false"
          :inputProps="{ autocomplete: 'new-password' }"
          toggleMask
          fluid
        />
        <label :for="fieldIds.confirmPassword">
          {{ $t("auth.register.field.confirmPassword") }}
        </label>
      </FloatLabel>
      <Message
        v-if="$form.confirmPassword?.invalid"
        severity="error"
        size="small"
        variant="simple"
      >
        {{ $form.confirmPassword.error.message }}
      </Message>
    </div>

    <!-- Submit Button -->
    <Button
      type="submit"
      severity="secondary"
      :label="$t('auth.register.submit')"
      :loading="isLoading"
      :disabled="$form.valid === false"
    />
  </Form>
</template>
