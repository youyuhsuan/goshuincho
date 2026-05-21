import { z } from "zod";

export const emailSchema = (t: (key: string) => string) =>
  z.string().email({ message: t("validation.email.invalid") });

export const passwordSchema = (t: (key: string) => string) =>
  z.preprocess(
    (v) => v ?? "",
    z
      .string()
      .min(6, { message: t("validation.password.min") })
      .max(500, { message: t("validation.password.max") })
      .refine((v) => /[a-z]/.test(v), { message: t("validation.password.lowercase") })
      .refine((v) => /[A-Z]/.test(v), { message: t("validation.password.uppercase") })
      .refine((v) => /\d/.test(v), { message: t("validation.password.number") }),
  );
