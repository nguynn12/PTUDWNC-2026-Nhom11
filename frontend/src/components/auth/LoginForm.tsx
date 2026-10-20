"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { useCallback, useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { getErrorMessage } from "@/lib/errors";
import { login, loginWithGoogle } from "@/lib/auth/session";
import { safeCallbackUrl } from "@/lib/auth/utils";
import { loginSchema, type LoginValues } from "@/lib/validators/auth";
import { useAuthStatus } from "@/store/auth-store";
import { Loader2 } from "lucide-react";
import { Separator } from "@/components/ui/separator";
import { AuthCard } from "./AuthCard";
import { Field } from "./Field";
import { FormAlert } from "./FormAlert";
import { GoogleSignInButton } from "./GoogleSignInButton";
import { PasswordField } from "./PasswordField";
import { SubmitButton } from "./SubmitButton";

export function LoginForm() {
  const router = useRouter();
  const params = useSearchParams();
  const callbackUrl = safeCallbackUrl(params.get("callbackUrl"));
  const status = useAuthStatus();
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
      setServerError(getErrorMessage(error));
    }
  });

  const onGoogleCredential = useCallback(async (idToken: string, nonce: string) => {
    setServerError(null);
    setGooglePending(true);
    try {
      await loginWithGoogle(idToken, nonce);
    } catch (error) {
      setServerError(getErrorMessage(error));
    } finally {
      setGooglePending(false);
    }
  }, []);

  return (
    <AuthCard
      title="Đăng nhập"
      subtitle="Chào mừng bạn trở lại Culinary Blog."
      footer={
        <>
          Chưa có tài khoản? <Link href="/auth/register">Đăng ký</Link>
        </>
      }
    >
      {serverError && <FormAlert>{serverError}</FormAlert>}

      <form onSubmit={onSubmit} noValidate className="grid gap-4">
        <Field label="Email" type="email" autoComplete="email" error={errors.email} {...register("email")} />
        <PasswordField
          label="Mật khẩu"
          autoComplete="current-password"
          error={errors.password}
          {...register("password")}
        />
        <SubmitButton loading={isSubmitting} loadingText="Đang đăng nhập..." disabled={googlePending}>
          Đăng nhập
        </SubmitButton>
      </form>

      <div className="flex items-center gap-3 text-xs text-muted-foreground uppercase">
        <Separator className="flex-1" />
        hoặc
        <Separator className="flex-1" />
      </div>
      <GoogleSignInButton onCredential={onGoogleCredential} />
      {googlePending && (
        <p className="flex items-center justify-center gap-2 text-sm text-muted-foreground" role="status">
          <Loader2 className="size-4 animate-spin" aria-hidden="true" /> Đang đăng nhập bằng Google...
        </p>
      )}
    </AuthCard>
  );
}
