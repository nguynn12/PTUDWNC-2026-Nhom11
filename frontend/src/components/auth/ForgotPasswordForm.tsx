"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { MailCheck } from "lucide-react";
import Link from "next/link";
import { useForm } from "react-hook-form";
import { Button } from "@/components/ui/button";
import { authApi } from "@/lib/auth/api";
import { applyFieldErrors, type AppError } from "@/lib/errors";
import { forgotPasswordSchema, type ForgotPasswordValues } from "@/lib/validators/auth";
import { AuthCard } from "./AuthCard";
import { Field } from "./Field";
import { FormAlert } from "./FormAlert";
import { SubmitButton } from "./SubmitButton";

/** Quên mật khẩu: nhập email để nhận liên kết đặt lại mật khẩu. */
export function ForgotPasswordForm() {
  const {
    register,
    handleSubmit,
    setError,
    getValues,
    formState: { errors },
  } = useForm<ForgotPasswordValues>({ resolver: zodResolver(forgotPasswordSchema) });

  const mutation = useMutation<void, AppError, string>({
    mutationFn: (email) => authApi.forgotPassword(email),
    onError: (error) => {
      applyFieldErrors(error, ["email"] as const, setError);
    },
  });

  const onSubmit = handleSubmit((values) => mutation.mutate(values.email.trim()));

  if (mutation.isSuccess) {
    return (
      <AuthCard
        title="Kiểm tra hộp thư"
        footer={
          <>
            Đã nhớ mật khẩu? <Link href="/auth/login">Đăng nhập</Link>
          </>
        }
      >
        <div className="flex flex-col items-center gap-3 py-2 text-center">
          <span className="flex size-14 items-center justify-center rounded-full bg-secondary/10 text-secondary">
            <MailCheck className="size-7" aria-hidden="true" />
          </span>
          <p className="text-sm text-muted-foreground" role="status">
            Nếu <strong className="text-foreground">{getValues("email")}</strong> đã đăng ký tài khoản, chúng tôi vừa
            gửi một liên kết đặt lại mật khẩu tới địa chỉ này. Liên kết chỉ dùng được một lần.
          </p>
          <p className="text-xs text-muted-foreground">Không thấy email? Hãy xem thư mục Spam hoặc gửi lại.</p>
          <Button variant="outline" className="hover:bg-muted hover:text-foreground" onClick={() => mutation.reset()}>
            Gửi lại với email khác
          </Button>
        </div>
      </AuthCard>
    );
  }

  return (
    <AuthCard
      title="Quên mật khẩu"
      subtitle="Nhập email đã đăng ký, chúng tôi sẽ gửi liên kết để bạn đặt mật khẩu mới."
      footer={
        <>
          Đã nhớ mật khẩu? <Link href="/auth/login">Đăng nhập</Link>
        </>
      }
    >
      {mutation.isError && !mutation.error.errors.email && <FormAlert>{mutation.error.message}</FormAlert>}

      <form onSubmit={onSubmit} noValidate className="grid gap-4">
        <Field label="Email" type="email" autoComplete="email" error={errors.email} {...register("email")} />
        <SubmitButton loading={mutation.isPending} loadingText="Đang gửi...">
          Gửi liên kết đặt lại mật khẩu
        </SubmitButton>
      </form>
    </AuthCard>
  );
}
