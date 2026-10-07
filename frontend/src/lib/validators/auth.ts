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

export const profileSchema = z.object({
  displayName: z.string().trim().min(2, "Tên hiển thị từ 2 đến 100 ký tự.").max(100, "Tên hiển thị từ 2 đến 100 ký tự."),
  avatarUrl: z
    .string()
    .trim()
    .max(500, "URL tối đa 500 ký tự.")
    .refine((v) => v === "" || /^https?:\/\//i.test(v), "URL ảnh phải bắt đầu bằng http:// hoặc https://"),
  bio: z.string().max(1000, "Giới thiệu tối đa 1000 ký tự."),
});

export type LoginValues = z.infer<typeof loginSchema>;
export type RegisterValues = z.infer<typeof registerSchema>;
export type ProfileValues = z.infer<typeof profileSchema>;
