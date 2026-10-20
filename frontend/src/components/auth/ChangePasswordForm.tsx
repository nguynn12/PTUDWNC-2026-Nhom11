"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { changePassword } from "@/lib/auth/session";
import { applyFieldErrors, type AppError } from "@/lib/errors";
import { changePasswordSchema, type ChangePasswordValues } from "@/lib/validators/auth";
import { useAuthUIStore } from "@/store/auth-ui-store";
import type { UserDto } from "@/types/auth";
import { FormAlert } from "./FormAlert";
import { PasswordField } from "./PasswordField";
import { SubmitButton } from "./SubmitButton";

const SERVER_FIELDS = ["currentPassword", "newPassword"] as const;
const PASSWORD_HINT = "Tối thiểu 8 ký tự, gồm chữ hoa, chữ thường, chữ số và ký tự đặc biệt.";

/** Đổi mật khẩu khi đã đăng nhập (POST /auth/password/change). */
export function ChangePasswordForm() {
  const queryClient = useQueryClient();
  const showToast = useAuthUIStore((s) => s.showToast);
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    setError,
    reset,
    formState: { errors },
  } = useForm<ChangePasswordValues>({
    resolver: zodResolver(changePasswordSchema),
    defaultValues: { currentPassword: "", newPassword: "", confirmPassword: "" },
  });

  const mutation = useMutation<UserDto, AppError, ChangePasswordValues>({
    mutationFn: (v) => changePassword(v.currentPassword, v.newPassword),
    onSuccess: (user) => {
      queryClient.setQueryData(["me"], user);
      reset();
      showToast("success", "Đã đổi mật khẩu. Các thiết bị khác đã được đăng xuất.");
    },
    onError: (error) => {
      if (!applyFieldErrors(error, SERVER_FIELDS, setError)) setServerError(error.message);
    },
  });

  const onSubmit = handleSubmit((values) => {
    setServerError(null);
    mutation.mutate(values);
  });

  return (
    <form onSubmit={onSubmit} noValidate className="grid gap-4">
      {serverError && <FormAlert>{serverError}</FormAlert>}
      <PasswordField
        id="currentPassword"
        label="Mật khẩu hiện tại"
        autoComplete="current-password"
        error={errors.currentPassword}
        {...register("currentPassword")}
      />
      <div className="grid gap-4 sm:grid-cols-2">
        <PasswordField
          id="newPassword"
          label="Mật khẩu mới"
          autoComplete="new-password"
          hint={PASSWORD_HINT}
          error={errors.newPassword}
          {...register("newPassword")}
        />
        <PasswordField
          id="confirmNewPassword"
          label="Nhập lại mật khẩu mới"
          autoComplete="new-password"
          error={errors.confirmPassword}
          {...register("confirmPassword")}
        />
      </div>
      <div className="flex justify-end">
        <SubmitButton className="sm:w-auto" loading={mutation.isPending} loadingText="Đang đổi...">
          Đổi mật khẩu
        </SubmitButton>
      </div>
    </form>
  );
}
