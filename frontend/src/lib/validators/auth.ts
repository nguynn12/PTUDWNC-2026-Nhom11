import { z } from "zod";

const email = z.string().trim().min(1, "Vui lòng nhập email.").email("Email không hợp lệ.");

const newPassword = z
  .string()
  .min(8, "Mật khẩu tối thiểu 8 ký tự.")
  .regex(/[A-Z]/, "Cần ít nhất 1 chữ hoa.")
  .regex(/[a-z]/, "Cần ít nhất 1 chữ thường.")
  .regex(/\d/, "Cần ít nhất 1 chữ số.")
  .regex(/[^A-Za-z0-9]/, "Cần ít nhất 1 ký tự đặc biệt.");

export const loginSchema = z.object({
  email,
  password: z.string().min(1, "Vui lòng nhập mật khẩu."),
});

export const registerSchema = z
  .object({
    displayName: z.string().trim().min(1, "Vui lòng nhập tên hiển thị.").max(100, "Tối đa 100 ký tự."),
    email,
    password: newPassword,
    confirmPassword: z.string().min(1, "Vui lòng nhập lại mật khẩu."),
  })
  .refine((d) => d.password === d.confirmPassword, {
    path: ["confirmPassword"],
    message: "Mật khẩu nhập lại không khớp.",
  });

/** Giới hạn hồ sơ — khớp UpdateProfileCommandValidator của backend (SRS FR-AUTH-007). */
export const PROFILE_LIMITS = { displayNameMin: 2, displayNameMax: 100, bioMax: 1000 } as const;

const DISPLAY_NAME_MESSAGE = `Tên hiển thị từ ${PROFILE_LIMITS.displayNameMin} đến ${PROFILE_LIMITS.displayNameMax} ký tự.`;

/** Form hồ sơ chỉ gồm tên và giới thiệu; ảnh đại diện được quản lý riêng (AvatarEditor). */
export const profileSchema = z.object({
  displayName: z
    .string()
    .trim()
    .min(PROFILE_LIMITS.displayNameMin, DISPLAY_NAME_MESSAGE)
    .max(PROFILE_LIMITS.displayNameMax, DISPLAY_NAME_MESSAGE),
  bio: z.string().max(PROFILE_LIMITS.bioMax, `Giới thiệu tối đa ${PROFILE_LIMITS.bioMax} ký tự.`),
});

export const forgotPasswordSchema = z.object({ email });

export const resetPasswordSchema = z
  .object({
    newPassword,
    confirmPassword: z.string().min(1, "Vui lòng nhập lại mật khẩu."),
  })
  .refine((d) => d.newPassword === d.confirmPassword, {
    path: ["confirmPassword"],
    message: "Mật khẩu nhập lại không khớp.",
  });

export const changePasswordSchema = z
  .object({
    currentPassword: z.string().min(1, "Vui lòng nhập mật khẩu hiện tại."),
    newPassword,
    confirmPassword: z.string().min(1, "Vui lòng nhập lại mật khẩu mới."),
  })
  .refine((d) => d.newPassword === d.confirmPassword, {
    path: ["confirmPassword"],
    message: "Mật khẩu nhập lại không khớp.",
  })
  .refine((d) => d.newPassword !== d.currentPassword, {
    path: ["newPassword"],
    message: "Mật khẩu mới phải khác mật khẩu hiện tại.",
  });

export type LoginValues = z.infer<typeof loginSchema>;
export type RegisterValues = z.infer<typeof registerSchema>;
export type ProfileValues = z.infer<typeof profileSchema>;
export type ForgotPasswordValues = z.infer<typeof forgotPasswordSchema>;
export type ResetPasswordValues = z.infer<typeof resetPasswordSchema>;
export type ChangePasswordValues = z.infer<typeof changePasswordSchema>;
