"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { useForm } from "react-hook-form";
import type { ApiError } from "@/lib/auth/errors";
import type { UserDto } from "@/lib/auth/types";
import { register as registerAccount } from "@/lib/auth/session";
import { registerSchema, type RegisterValues } from "@/lib/validators/auth";
import { useAuthUIStore } from "@/store/useAuthUIStore";
import styles from "@/styles/auth.module.css";
import { Field } from "./Field";

const FIELDS = ["displayName", "email", "password"] as const;

export function RegisterForm() {
  const router = useRouter();
  const showToast = useAuthUIStore((s) => s.showToast);
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<RegisterValues>({ resolver: zodResolver(registerSchema) });

  const mutation = useMutation<UserDto, ApiError, RegisterValues>({
    mutationFn: (v) => registerAccount({ email: v.email, password: v.password, displayName: v.displayName }),
    // FR-AUTH-001: backend trả cặp token ngay khi đăng ký nên người dùng vào thẳng trang hồ sơ.
    onSuccess: () => {
      showToast("success", "Đăng ký thành công! Hãy kiểm tra email để xác nhận tài khoản.");
      router.replace("/profile");
    },
    onError: (error) => {
      if (error.code === "AUTH_EMAIL_EXISTS") {
        setError("email", { message: error.message });
        return;
      }
      // 422: backend trả errors { email: [...], password: [...] } -> gắn vào từng ô.
      let mapped = false;
      for (const field of FIELDS) {
        const messages = error.fieldErrors[field];
        if (messages?.length) {
          setError(field, { message: messages[0] });
          mapped = true;
        }
      }
      if (!mapped) setServerError(error.message);
    },
  });

  const onSubmit = handleSubmit((values) => {
    setServerError(null);
    mutation.mutate(values);
  });

  return (
    <main className={styles.page}>
      <div className={styles.card}>
        <h1 className={styles.title}>Tạo tài khoản</h1>
        <p className={styles.subtitle}>Tham gia cộng đồng chia sẻ công thức nấu ăn.</p>

        {serverError && (
          <div className={`${styles.alert} ${styles.alertError}`} role="alert">
            {serverError}
          </div>
        )}

        <form onSubmit={onSubmit} noValidate>
          <Field label="Tên hiển thị" autoComplete="name" error={errors.displayName} {...register("displayName")} />
          <Field label="Email" type="email" autoComplete="email" error={errors.email} {...register("email")} />
          <Field
            label="Mật khẩu"
            type="password"
            autoComplete="new-password"
            error={errors.password}
            {...register("password")}
          />
          <Field
            label="Nhập lại mật khẩu"
            type="password"
            autoComplete="new-password"
            error={errors.confirmPassword}
            {...register("confirmPassword")}
          />
          <button type="submit" className={styles.button} disabled={mutation.isPending}>
            {mutation.isPending ? "Đang tạo tài khoản..." : "Đăng ký"}
          </button>
        </form>

        <p className={styles.footer}>
          Đã có tài khoản? <Link href="/auth/login">Đăng nhập</Link>
        </p>
      </div>
    </main>
  );
}
