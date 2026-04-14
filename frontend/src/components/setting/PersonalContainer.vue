<script lang="ts" setup>
import { computed, ref } from "vue";
// i18n
import { useI18n } from "vue-i18n";
// Primevue
import FileUpload, { type FileUploadSelectEvent } from "primevue/fileupload";
import Dialog from "primevue/dialog";
import { useConfirm } from "primevue/useconfirm";
import type { FormSubmitEvent } from "@primevue/forms";
import { zodResolver } from "@primevue/forms/resolvers/zod";
// Zod
import z from "zod";
// Router
import router from "@/router";
// Components
import Avatar from "@/components/Avatar.vue";
// Composables
import useApiUser from "@/composables/api/useApiUser";
import useMessage from "@/composables/useMessage";
// Stores
import useAuthStore from "@/stores/auth.store";
// Config
import ROUTE_CONFIGS from "@/config/routeConfig";
// Utiles
import compressImage from "@/utils/compressImage";
import generateFieldIds from "@/utils/generateFieldIds";
import type { FieldIds } from "@/utils/generateFieldIds";
// Type
import type { UpdateRequest } from "@/types/userType";

const { t } = useI18n();

const { getUser, updateUser, uploadUserImage, deleteUser } = useApiUser();
const isLoading = ref<boolean>(false);

const authStore = useAuthStore();
const avatarSize = computed(() => {
  const width = window.innerWidth;
  if (width < 1024) return "large"; // mobile, tablet

  return "xlarge"; // desktop
});

// Profile Picture Upload
// Controls profile picture upload dialog visibility
const isVisibleDialog = ref<boolean>(false);

// Reference to PrimeVue FileUpload instance for programmatic control
const vFileUpload = ref<InstanceType<typeof FileUpload> | null>(null);
const selectedFile = ref<File | null>(null);

// Store the first selected file from FileUpload event
const onSelectedFile = (event: FileUploadSelectEvent) => {
  if (event.files.length === 0) return;
  selectedFile.value = event.files[event.files.length - 1];
};

// Compress and upload the selected profile picture
const saveUpload = async () => {
  if (!selectedFile.value || !authStore.user?.id) return;

  isLoading.value = true;
  try {
    // Compress image before upload
    const image = await compressImage(selectedFile.value);
    if (!image) return;

    // Build FormData with compressed file
    const formData = new FormData();
    formData.append("file", image);

    await uploadUserImage(authStore.user?.id, formData);
    authStore.user = (await getUser(authStore.user.id)).data;

    isVisibleDialog.value = false;
  } catch (error: unknown) {
    if (typeof error === "string") showWarning(error);
  } finally {
    isLoading.value = false;
  }
};

// Clear FileUpload state and close the dialog
const cancelUpload = () => {
  (vFileUpload.value as any)?.clear();
  selectedFile.value = null;
  isVisibleDialog.value = false;
};

// Toggles between view and edit mode
const isEdit = ref<boolean>(false);

// Edit Mode
const fieldIds = ref<FieldIds>(generateFieldIds(["name"]));
const initialValues = ref({
  name: authStore.user?.name,
});

const resolver = zodResolver(
  z.object({
    name: z
      .string()
      .min(1, { message: t("settings.profile.validation.name.min") })
      .max(100, { message: t("settings.profile.validation.name.max") }),
  }),
);
const { showWarning, showInfo } = useMessage();

// Handle profile name update form submission
const onFormSubmit = async (e: FormSubmitEvent) => {
  if (!authStore.user) return;
  // Prevent the browser's default form submission behavior
  e.originalEvent.preventDefault();

  // Check if all form fields have passed validation
  if (e.valid) {
    isLoading.value = true;

    try {
      await updateUser(authStore.user.id, e.values as UpdateRequest);
      authStore.user = (await getUser(authStore.user.id)).data;
      isEdit.value = false;
      showInfo(t("settings.profile.dialog.message.updateSuccess"));
    } catch (error: unknown) {
      if (typeof error === "string") showWarning(error);
    } finally {
      isLoading.value = false;
    }
  }
};

// Delete Account
const confirm = useConfirm();
const deleteUserAccount = async () => {
  if (!authStore.user) return;

  confirm.require({
    message: t("settings.profile.deleteAccount.message"),
    header: t("settings.profile.deleteAccount.title"),
    icon: "pi pi-exclamation-triangle",
    rejectProps: {
      label: t("common.cancel"),
      severity: "secondary",
      outlined: true,
    },
    acceptProps: {
      label: t("common.delete"),
      severity: "danger",
    },
    accept: async () => {
      if (!authStore.user) return;

      try {
        await deleteUser(authStore.user!.id);
        // Clear auth state after account deletion
        await authStore.logout();
        router.push(ROUTE_CONFIGS.AUTH);
      } catch (error: unknown) {
        if (typeof error === "string") showWarning(error);
      }
    },
  });
};
</script>

<template>
  <!-- Section header with edit toggle -->
  <div class="flex justify-between mb-6">
    <h2 class="text-md font-semibold">{{ $t("settings.profile.title") }}</h2>
    <Button
      v-if="!isEdit"
      variant="outlined"
      severity="secondary"
      icon="pi pi-pencil"
      size="small"
      :label="$t('common.edit')"
      :aria-label="$t('settings.profile.ariaLabel.edit')"
      @click="isEdit = true"
    >
    </Button>
  </div>

  <!-- View mode container -->
  <section v-if="!isEdit">
    <div class="flex items-center gap-4 mb-12">
      <!-- Avatar -->
      <div class="relative w-fit">
        <Avatar
          :size="avatarSize"
          iconClass="text-4xl"
          avatarClass="!w-16 !h-16"
        />
        <!-- Upload image button -->
        <Button
          class="-bottom-1.5 -right-1.5"
          :pt="{
            root: {
              style: 'position: absolute',
            },
          }"
          icon="pi pi-image"
          rounded
          severity="secondary"
          :aria-label="$t('settings.profile.ariaLabel.uploadImage')"
          size="small"
          @click="isVisibleDialog = true"
        />

        <!-- Upload image dialog -->
        <Dialog
          v-model:visible="isVisibleDialog"
          modal
          :header="$t('settings.profile.dialog.title')"
          :style="{ width: '25rem' }"
        >
          <FileUpload
            ref="vFileUpload"
            accept="image/*"
            @select="onSelectedFile"
          >
            <template #header="{ chooseCallback }">
              <Button
                class="w-100"
                icon="pi pi-images"
                variant="outlined"
                severity="secondary"
                :label="$t('settings.profile.uploadImage')"
                :aria-label="$t('settings.profile.ariaLabel.uploadImage')"
                @click="chooseCallback()"
                autofocus
              />
            </template>

            <template #content="{ uploadedFiles, files, messages }">
              <div class="flex justify-center">
                <div v-if="files.length > 0">
                  <div
                    :key="
                      files[files.length - 1].name +
                      files[files.length - 1].type +
                      files[files.length - 1].size
                    "
                    class="flex flex-col items-center gap-2"
                  >
                    <img
                      class="rounded-full object-cover w-24 h-24 sm:w-32 sm:h-32 lg:w-40 lg:h-40"
                      role="presentation"
                      :alt="files[files.length - 1].name"
                      :src="(files[files.length - 1] as any).objectURL"
                    />
                    <span
                      class="text-sm text-surface-500 text-ellipsis max-w-40 whitespace-nowrap overflow-hidden"
                    >
                      {{ files[files.length - 1].name }}
                    </span>
                  </div>
                </div>

                <Message
                  v-for="message of messages"
                  :key="message"
                  :class="{ 'mb-8': !files.length && !uploadedFiles.length }"
                  severity="error"
                >
                  {{ message }}
                </Message>
              </div>
            </template>

            <template #empty>
              <div class="flex items-center justify-center">
                <Avatar
                  :size="avatarSize"
                  iconClass="text-4xl"
                  avatarClass="!w-24 !h-24 sm:!w-32 sm:!h-32 lg:!w-40 lg:!h-40"
                />
              </div>
            </template>
          </FileUpload>

          <!-- Modal footer -->
          <template #footer v-if="selectedFile">
            <!-- Cancel Button -->
            <Button
              :label="$t('common.cancel')"
              :aria-label="$t('common.cancel')"
              text
              severity="secondary"
              @click="cancelUpload()"
            />
            <!-- Save Button -->
            <Button
              :label="$t('common.save')"
              :aria-label="$t('common.save')"
              variant="outlined"
              severity="secondary"
              @click="saveUpload"
            />
          </template>
        </Dialog>
      </div>

      <!-- User display name -->
      <div class="flex flex-col">
        <span class="text-2xl font-bold tracking-wide">
          {{ authStore.user?.name }}
        </span>
        <span class="text-lg font-light">
          {{ authStore.user?.email }}
        </span>
      </div>
    </div>

    <!-- Delete account -->
    <div class="flex flex-col">
      <h2 class="text-md font-bold text-surface-900 dark:text-surface-0 mb-2">
        {{ $t("settings.profile.deleteAccount.title") }}
      </h2>
      <span class="text-xs mb-5">
        {{ $t("settings.profile.deleteAccount.description") }}
      </span>
      <div>
        <Button
          severity="danger"
          :label="$t('settings.profile.deleteAccount.title')"
          :aria-label="$t('settings.profile.ariaLabel.deleteAccount')"
          @click="deleteUserAccount()"
        />
        <ConfirmDialog></ConfirmDialog>
      </div>
    </div>
  </section>

  <!-- Edit mode container -->
  <section v-else>
    <Form
      v-slot="$form"
      :initialValues="initialValues"
      :resolver="resolver"
      :validateOnValueUpdate="false"
      :validateOnBlur="true"
      class="flex flex-col gap-6.5 w-full mb-4"
      @submit="onFormSubmit"
    >
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
            {{ $t("settings.profile.field.name") }}
          </label>
        </FloatLabel>
        <Message
          v-if="$form.name?.invalid"
          severity="error"
          size="small"
          variant="simple"
        >
          {{ $form.name.error.message }}
        </Message>
      </div>

      <!-- Save Button -->
      <div class="flex justify-end">
        <Button
          type="submit"
          severity="secondary"
          :label="$t('settings.profile.submit')"
          :aria-label="$t('settings.profile.ariaLabel.submit')"
          :loading="isLoading"
          :disabled="$form.valid === false"
        />
      </div>
    </Form>
  </section>
</template>
