<template>
  <Form
    v-slot="$form"
    :initialValues="initialValues"
    :resolver="resolver"
    class="flex flex-col gap-5 w-full"
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

    <!-- Remember me -->
    <div class="mb-5">
      <Checkbox v-model="checked" :inputId="fieldIds.checked" binary />
      <label :for="fieldIds.checked"> Remember Me</label>
    </div>

    <Button type="submit" severity="secondary" label="Sign in" />
  </Form>
</template>

<script setup lang="ts">
import { ref } from "vue";
// Primevue
import FloatLabel from "primevue/floatlabel";
import Checkbox from "primevue/checkbox";
import { zodResolver } from "@primevue/forms/resolvers/zod";
import type { FormSubmitEvent } from "@primevue/forms";
import { z } from "zod";
// Composables
import useApiUser from "@/composables/api/useApiUser";
import useMessage from "@/composables/useMessage";
import useApiErrorHandler from "@/composables/api/useApiErrorHandler";
// Utils
import generateFieldIds, { type FieldIds } from "@/utils/generateFieldIds";
import type { LoginRequest } from "@/types/userType";

const isLoading = ref<boolean>(false);
// TODO:
// Remenber check
const checked = ref<boolean>(false);

const fieldIds = ref<FieldIds>(
  generateFieldIds(["email", "password", "checked"])
);
const initialValues = ref<LoginRequest>({
  email: "",
  password: "",
});

const { loginUser } = useApiUser();
const { handleAndShowError } = useApiErrorHandler();
const { showSuccess } = useMessage();

const resolver = zodResolver(
  z.object({
    email: z
      .email()
      .min(1, { message: "Minimum 1 characters." })
      .max(320, { message: "Maximum 320 characters." }),
    password: z
      .string()
      .min(3, { message: "Minimum 3 characters." })
      .max(8, { message: "Maximum 8 characters." })
      .refine((value) => /[a-z]/.test(value), {
        message: "Must have a lowercase letter.",
      })
      .refine((value) => /[A-Z]/.test(value), {
        message: "Must have an uppercase letter.",
      })
      .refine((value) => /\d/.test(value), {
        message: "Must have a number.",
      }),
  })
);

const onFormSubmit = async (e: FormSubmitEvent) => {
  isLoading.value = true;
  // Prevent the browser's default form submission behavior
  e.originalEvent.preventDefault();

  // Check if all form fields have passed validation
  if (e.valid) {
    try {
      // Extract form values and submit to the login API
      await loginUser(e.values as LoginRequest);
      // Reset all form fields to their initial state
      e.reset();

      // Notify user of successful login
      showSuccess("Login successful");
    } catch (error: unknown) {
      handleAndShowError(error, "Login");
    } finally {
      isLoading.value = false;
    }
  }
};
</script>
