<script setup lang="ts">
import { ref } from "vue";
import { useRouter } from "vue-router";
// Primevue
import { zodResolver } from "@primevue/forms/resolvers/zod";
import type { FormSubmitEvent } from "@primevue/forms";
// Zod
import { z } from "zod";
// i18n
import { useI18n } from "vue-i18n";
// Composables
import useApiAuth from "@/composables/api/useApiAuth";
import useAsyncAction from "@/composables/useAsyncAction";
// Schemas
import { emailSchema, passwordSchema } from "@/schemas/authSchemas";
// Utils
import generateFieldIds, { type FieldIds } from "@/utils/generateFieldIds";
// Type
import type { RegisterFormData, RegisterRequest } from "@/types/authType";
// Config
import ROUTE_CONFIGS from "@/config/routeConfig";

const { t } = useI18n();
const router = useRouter();

const fieldIds = ref<FieldIds>(
  generateFieldIds(["name", "password", "confirmPassword"]),
);
const initialValues = ref<RegisterFormData>({
  email: "",
  name: "",
  password: "",
  confirmPassword: "",
});

const { registerUser } = useApiAuth();

const { isLoading, execute } = useAsyncAction((values: RegisterRequest) =>
  registerUser(values),
);

const resolver = zodResolver(
  z
    .object({
      name: z
        .string()
        .min(1, { message: t("auth.register.validation.name.min") })
        .max(100, { message: t("auth.register.validation.name.max") }),
      email: emailSchema(t),
      password: passwordSchema(t),
      confirmPassword: z.string(),
    })
    .refine((data) => data.password === data.confirmPassword, {
      message: t("validation.confirmPassword.match"),
      path: ["confirmPassword"],
    }),
);

const onFormSubmit = async (e: FormSubmitEvent) => {
  e.originalEvent.preventDefault();
  if (!e.valid) return;

  const { confirmPassword: _, ...registerData } = e.values as RegisterFormData;
  const ok = await execute(registerData);
  if (ok) {
    e.reset();
    router.push(ROUTE_CONFIGS.AUTH_LOGIN);
  }
};
</script>

<template>
  <Form
    v-slot="$form"
    :initialValues="initialValues"
    :resolver="resolver"
    class="w-full flex flex-col gap-6.5"
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
        v-if="$form.email?.invalid && $form.email?.value"
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
      :disabled="$form.valid === false || $form.dirty === false || isLoading"
    />
  </Form>
</template>
