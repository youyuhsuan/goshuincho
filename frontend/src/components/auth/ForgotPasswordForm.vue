<script setup lang="ts">
// Primevue
import FloatLabel from "primevue/floatlabel";
import type { FormSubmitEvent } from "@primevue/forms";
import { zodResolver } from "@primevue/forms/resolvers/zod";
// Zod
import { z } from "zod";
// i18n
import { useI18n } from "vue-i18n";
// Composables
import useApiAuth from "@/composables/api/useApiAuth";
import useAsyncAction from "@/composables/useAsyncAction";
// Schemas
import { emailSchema } from "@/schemas/authSchemas";

const { t } = useI18n();
const { forgotPassword } = useApiAuth();

const { isLoading, execute } = useAsyncAction(
  (email: string) => forgotPassword(email),
  { successMessage: t("auth.forgotPassword.successMessage") },
);

const resolver = zodResolver(z.object({ email: emailSchema(t) }));

const onFormSubmit = async (e: FormSubmitEvent) => {
  e.originalEvent.preventDefault();
  if (!e.valid) return;

  await execute(e.values.email as string);
};
</script>

<template>
  <Form
    v-slot="$form"
    :resolver="resolver"
    :validateOnBlur="true"
    :validateOnValueUpdate="false"
    class="w-full flex flex-col gap-6"
    @submit="onFormSubmit"
  >
    <div class="flex flex-col gap-1">
      <FloatLabel>
        <InputText
          id="forgot-email"
          name="email"
          type="text"
          autocomplete="email"
          :invalid="$form.email?.invalid"
          fluid
        />
        <label for="forgot-email">{{
          $t("auth.forgotPassword.field.email")
        }}</label>
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

    <!-- Submit Button -->
    <Button
      type="submit"
      severity="secondary"
      :label="$t('auth.forgotPassword.submit')"
      :loading="isLoading"
      :disabled="$form.valid === false || $form.dirty === false || isLoading"
    />
  </Form>
</template>
