"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { applyFieldErrors, type AppError } from "@/lib/errors";
import { register as registerAccount } from "@/lib/auth/session";
import { registerSchema, type RegisterValues } from "@/lib/validators/auth";
import { useAuthUIStore } from "@/store/auth-ui-store";
import type { UserDto } from "@/types/auth";
import { AuthCard } from "./AuthCard";
import { Field } from "./Field";
import { FormAlert } from "./FormAlert";
import { PasswordField } from "./PasswordField";
import { SubmitButton } from "./SubmitButton";

const SERVER_FIELDS = ["displayName", "email", "password"] as const;
const PASSWORD_HINT = "Tối thiểu 8 ký tự, gồm chữ hoa, chữ thường, chữ số và ký tự đặc biệt.";

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

  const mutation = useMutation<UserDto, AppError, RegisterValues>({
    mutationFn: (v) => registerAccount({ email: v.email, password: v.password, displayName: v.displayName }),
    // FR-AUTH-001: backend trả cặp token ngay khi đăng ký nên người dùng vào thẳng trang hồ sơ.
    onSuccess: () => {
      showToast("success", "Đăng ký thành công! Hãy kiểm tra email để xác nhận tài khoản.");
      router.replace("/profile");
    },
    onError: (error) => {
      if (error.code === "AUTH_EMAIL_EXISTS") {
        setError("email", { type: "server", message: error.message });
        return;
      }
      // 422: gắn lỗi từng trường; không gắn được thì hiện thông báo chung.
      if (!applyFieldErrors(error, SERVER_FIELDS, setError)) setServerError(error.message);
    },
  });

  const onSubmit = handleSubmit((values) => {
    setServerError(null);
    mutation.mutate(values);
  });

  return (
    <AuthCard
      title="Tạo tài khoản"
      subtitle="Tham gia cộng đồng chia sẻ công thức nấu ăn."
      footer={
        <>
          Đã có tài khoản? <Link href="/auth/login">Đăng nhập</Link>
        </>
      }
    >
      {serverError && <FormAlert>{serverError}</FormAlert>}

      <form onSubmit={onSubmit} noValidate className="grid gap-4">
        <Field label="Tên hiển thị" autoComplete="name" error={errors.displayName} {...register("displayName")} />
        <Field label="Email" type="email" autoComplete="email" error={errors.email} {...register("email")} />
        <PasswordField
          label="Mật khẩu"
          autoComplete="new-password"
          hint={PASSWORD_HINT}
          error={errors.password}
          {...register("password")}
        />
        <PasswordField
          label="Nhập lại mật khẩu"
          autoComplete="new-password"
          error={errors.confirmPassword}
          {...register("confirmPassword")}
        />
        <SubmitButton loading={mutation.isPending} loadingText="Đang tạo tài khoản...">
          Đăng ký
        </SubmitButton>
      </form>
    </AuthCard>
  );
}
