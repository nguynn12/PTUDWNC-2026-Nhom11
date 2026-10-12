"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { useCallback, useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { ApiError, DEFAULT_ERROR_MESSAGE } from "@/lib/auth/errors";
import { login, loginWithGoogle } from "@/lib/auth/session";
import { useAuthStore } from "@/lib/auth/store";
import { safeCallbackUrl } from "@/lib/auth/utils";
import { loginSchema, type LoginValues } from "@/lib/validators/auth";
import styles from "@/styles/auth.module.css";
import { Field } from "./Field";
import { GoogleSignInButton } from "./GoogleSignInButton";

function errorMessage(error: unknown): string {
  return error instanceof ApiError ? error.message : DEFAULT_ERROR_MESSAGE;
}

export function LoginForm() {
  const router = useRouter();
  const params = useSearchParams();
  const callbackUrl = safeCallbackUrl(params.get("callbackUrl"));
  const status = useAuthStore((s) => s.status);
  const [serverError, setServerError] = useState<string | null>(null);
  const [googlePending, setGooglePending] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginValues>({ resolver: zodResolver(loginSchema) });

  // SRS 5.1: đã đăng nhập thì không ở lại trang login.
  useEffect(() => {
    if (status === "authenticated") router.replace(callbackUrl);
  }, [status, router, callbackUrl]);

  const onSubmit = handleSubmit(async (values) => {
    setServerError(null);
    try {
      await login(values.email, values.password);
    } catch (error) {
      setServerError(errorMessage(error));
    }
  });

  const onGoogleCredential = useCallback(async (idToken: string, nonce: string) => {
    setServerError(null);
    setGooglePending(true);
    try {
      await loginWithGoogle(idToken, nonce);
    } catch (error) {
      setServerError(errorMessage(error));
    } finally {
      setGooglePending(false);
    }
  }, []);

  return (
    <main className={styles.page}>
      <div className={styles.card}>
        <h1 className={styles.title}>Đăng nhập</h1>
        <p className={styles.subtitle}>Chào mừng bạn trở lại Culinary Blog.</p>

        {serverError && (
          <div className={`${styles.alert} ${styles.alertError}`} role="alert">
            {serverError}
          </div>
        )}

        <form onSubmit={onSubmit} noValidate>
          <Field label="Email" type="email" autoComplete="email" error={errors.email} {...register("email")} />
          <Field
            label="Mật khẩu"
            type="password"
            autoComplete="current-password"
            error={errors.password}
            {...register("password")}
          />
          <button type="submit" className={styles.button} disabled={isSubmitting || googlePending}>
            {isSubmitting ? "Đang đăng nhập..." : "Đăng nhập"}
          </button>
        </form>

        <div className={styles.divider}>hoặc</div>
        <GoogleSignInButton onCredential={onGoogleCredential} />
        {googlePending && <p className={styles.hint}>Đang đăng nhập bằng Google...</p>}

        <p className={styles.footer}>
          Chưa có tài khoản? <Link href="/auth/register">Đăng ký</Link>
        </p>
      </div>
    </main>
  );
}
