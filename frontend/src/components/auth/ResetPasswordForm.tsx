"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { useForm } from "react-hook-form";
import { Button } from "@/components/ui/button";
import { authApi } from "@/lib/auth/api";
import { applyFieldErrors, type AppError } from "@/lib/errors";
import { resetPasswordSchema, type ResetPasswordValues } from "@/lib/validators/auth";
import { AuthCard } from "./AuthCard";
import { Field } from "./Field";
import { FormAlert } from "./FormAlert";
import { PasswordField } from "./PasswordField";
import { SubmitButton } from "./SubmitButton";

const PASSWORD_HINT = "Tối thiểu 8 ký tự, gồm chữ hoa, chữ thường, chữ số và ký tự đặc biệt.";

/** Trang đích của liên kết trong email quên mật khẩu (/auth/reset-password?email=...&token=...). */
export function ResetPasswordForm() {
  const params = useSearchParams();
  const email = params.get("email");
  const token = params.get("token");

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<ResetPasswordValues>({ resolver: zodResolver(resetPasswordSchema) });

  const mutation = useMutation<void, AppError, ResetPasswordValues>({
    mutationFn: (v) => authApi.resetPassword({ email: email!, token: token!, newPassword: v.newPassword }),
    onError: (error) => {
      applyFieldErrors(error, ["newPassword"] as const, setError);
    },
  });

  if (!email || !token) {
    return (
      <AuthCard title="Đặt lại mật khẩu">
        <FormAlert title="Liên kết không hợp lệ">
          Liên kết đặt lại mật khẩu thiếu thông tin. Hãy mở đúng liên kết trong email hoặc{" "}
          <Link href="/auth/forgot-password">yêu cầu liên kết mới</Link>.
        </FormAlert>
      </AuthCard>
    );
  }

  if (mutation.isSuccess) {
    return (
      <AuthCard title="Đặt lại mật khẩu">
        <FormAlert variant="success" title="Đã đổi mật khẩu">
          Mật khẩu mới đã được lưu. Các phiên đăng nhập cũ trên thiết bị khác đã bị đăng xuất.
        </FormAlert>
        <Button asChild size="lg" className="h-10 w-full">
          <Link href="/auth/login">Đăng nhập bằng mật khẩu mới</Link>
        </Button>
      </AuthCard>
    );
  }

  const tokenInvalid = mutation.error?.code === "AUTH_RESET_TOKEN_INVALID";

  return (
    <AuthCard
      title="Đặt mật khẩu mới"
      subtitle="Nhập mật khẩu mới cho tài khoản của bạn."
      footer={
        <>
          Nhớ mật khẩu rồi? <Link href="/auth/login">Đăng nhập</Link>
        </>
      }
    >
      {mutation.isError && !mutation.error.errors.newPassword && (
        <FormAlert>
          {mutation.error.message}
          {tokenInvalid && (
            <>
              {" "}
              <Link href="/auth/forgot-password">Yêu cầu liên kết mới</Link>.
            </>
          )}
        </FormAlert>
      )}

      <form onSubmit={handleSubmit((values) => mutation.mutate(values))} noValidate className="grid gap-4">
        <Field id="reset-email" label="Email" value={email} disabled readOnly />
        <PasswordField
          label="Mật khẩu mới"
          autoComplete="new-password"
          hint={PASSWORD_HINT}
          error={errors.newPassword}
          {...register("newPassword")}
        />
        <PasswordField
          label="Nhập lại mật khẩu mới"
          autoComplete="new-password"
          error={errors.confirmPassword}
          {...register("confirmPassword")}
        />
        <SubmitButton loading={mutation.isPending} loadingText="Đang lưu...">
          Đặt lại mật khẩu
        </SubmitButton>
      </form>
    </AuthCard>
  );
}
