<template>
  <Form
    v-slot="$form"
    :initialValues="initialValues"
    :resolver="resolver"
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
        <label :for="fieldIds.email">email</label>
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
        <label :for="fieldIds.name">name</label>
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
        <label :for="fieldIds.password">Password</label>
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
        <label :for="fieldIds.confirmPassword">Confirm Password</label>
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

    <Button
      type="submit"
      severity="secondary"
      label="Sign up"
      :loading="isLoading"
      :disabled="$form.valid === false"
    />
  </Form>
</template>

<script setup lang="ts">
import { ref } from "vue";
// Primevue
import { zodResolver } from "@primevue/forms/resolvers/zod";
import type { FormSubmitEvent } from "@primevue/forms";
import { z } from "zod";
// Composables
import useApiUser from "@/composables/api/useApiUser";
import useMessage from "@/composables/useMessage";
// Utils
import generateFieldIds, { type FieldIds } from "@/utils/generateFieldIds";
// Type
import type { RegisterFormData, RegisterRequest } from "@/types/userType";

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

const isLogin = defineModel<boolean>("isLogin", {
  required: true,
});

const { registerUser } = useApiUser();
const { showWarning } = useMessage();

const resolver = zodResolver(
  z
    .object({
      // TODO: first name + last name = full name
      name: z
        .string()
        .min(1, { message: "Minimum 1 characters." })
        .max(50, { message: "Maximum 50 characters." }),
      email: z
        .email()
        .min(1, { message: "Minimum 1 characters." })
        .max(320, { message: "Maximum 320 characters." }),
      password: z
        .string()
        .min(6, { message: "Minimum 6 characters." })
        .max(500, { message: "Maximum 500 characters." })
        .refine((value) => /[a-z]/.test(value), {
          message: "Must have a lowercase letter.",
        })
        .refine((value) => /[A-Z]/.test(value), {
          message: "Must have an uppercase letter.",
        })
        .refine((value) => /\d/.test(value), {
          message: "Must have a number.",
        }),
      confirmPassword: z.string(),
    })
    .refine((data) => data.password === data.confirmPassword, {
      message: "Passwords do not match.",
      path: ["confirmPassword"],
    }),
);

const onFormSubmit = async (e: FormSubmitEvent) => {
  isLoading.value = true;
  // Prevent the browser's default form submission behavior
  e.originalEvent.preventDefault();

  // Check if all form fields have passed validation
  if (e.valid) {
    try {
      // Extract form values and submit to the login API
      const { confirmPassword: _, ...registerData } =
        e.values as RegisterFormData;
      await registerUser(registerData as RegisterRequest);

      // Reset all form fields to their initial state
      e.reset();

      // Switch to login mode after successful registration
      isLogin.value = true;
    } catch (error: unknown) {
      console.error("Registration error:", error);
      if (typeof error === "string") {
        showWarning(error);
      }
    } finally {
      isLoading.value = false;
    }
  }
};
</script>
