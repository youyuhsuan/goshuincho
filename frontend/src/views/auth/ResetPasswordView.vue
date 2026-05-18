<script setup lang="ts">
import { computed, onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";
// Primevue
import { zodResolver } from "@primevue/forms/resolvers/zod";
import type { FormSubmitEvent } from "@primevue/forms";
import FloatLabel from "primevue/floatlabel";
// Zod
import { z } from "zod";
// I18n
import { useI18n } from "vue-i18n";
// Composables
import useApiAuth from "@/composables/api/useApiAuth";
import useAsyncAction from "@/composables/useAsyncAction";
// Schemas
import { passwordSchema } from "@/schemas/authSchemas";
// Configs
import ROUTE_CONFIGS from "@/config/routeConfig";
// Components
import Header from "@/components/auth/Header.vue";

const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const { resetPassword } = useApiAuth();

const token = computed(() => route.query.token as string | undefined);

const { isLoading, execute } = useAsyncAction((newPassword: string) =>
  resetPassword(token.value!, newPassword),
);

const RESTRICT_MS = 3800;

const resolver = zodResolver(
  z
    .object({
      newPassword: passwordSchema(t),
      confirmPassword: z.preprocess((v) => v ?? "", z.string()),
    })
    .refine((data) => data.newPassword === data.confirmPassword, {
      message: t("validation.confirmPassword.match"),
      path: ["confirmPassword"],
    }),
);

const onSubmit = async (e: FormSubmitEvent) => {
  e.originalEvent.preventDefault();
  if (!e.valid) return;

  const ok = await execute(e.values.newPassword as string);
  if (ok) setTimeout(() => router.push(ROUTE_CONFIGS.AUTH_LOGIN), RESTRICT_MS);
};

onMounted(() => {
  if (!token.value) {
    router.replace(ROUTE_CONFIGS.AUTH_FORGOT_PASSWORD);
  }
});
</script>

<template>
  <section>
    <!-- Header -->
    <Header />

    <!-- Form -->
    <Form
      v-slot="$form"
      :resolver="resolver"
      :validateOnBlur="true"
      :validateOnValueUpdate="false"
      class="w-full flex flex-col gap-6 mb-6"
      @submit="onSubmit"
    >
      <div class="flex flex-col gap-1">
        <FloatLabel>
          <Password
            id="new-password"
            name="newPassword"
            :feedback="false"
            :inputProps="{ autocomplete: 'new-password' }"
            :invalid="$form.newPassword?.invalid"
            toggleMask
            fluid
          />
          <label for="new-password">{{
            $t("auth.resetPassword.field.newPassword")
          }}</label>
        </FloatLabel>
        <Message
          v-if="$form.newPassword?.invalid"
          severity="error"
          size="small"
          variant="simple"
        >
          <ul class="my-0 px-4 flex flex-col gap-1">
            <li v-for="(error, i) of $form.newPassword.errors" :key="i">
              {{ error.message }}
            </li>
          </ul>
        </Message>
      </div>

      <div class="flex flex-col gap-1">
        <FloatLabel>
          <Password
            id="confirm-password"
            name="confirmPassword"
            :feedback="false"
            :inputProps="{ autocomplete: 'new-password' }"
            :invalid="$form.confirmPassword?.invalid"
            toggleMask
            fluid
          />
          <label for="confirm-password">{{
            $t("auth.resetPassword.field.confirmPassword")
          }}</label>
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
        :label="$t('auth.resetPassword.submit')"
        :loading="isLoading"
        :disabled="$form.valid === false || $form.dirty === false || isLoading"
        fluid
      />
    </Form>
  </section>
</template>
