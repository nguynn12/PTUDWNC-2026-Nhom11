"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { authApi } from "@/lib/auth/api";
import { applyFieldErrors, type AppError } from "@/lib/errors";
import { profileSchema, type ProfileValues } from "@/lib/validators/auth";
import { useAuthStatus, useAuthStore } from "@/store/auth-store";
import { useAuthUIStore } from "@/store/auth-ui-store";
import type { ResendConfirmationResponse, UserDto } from "@/types/auth";
import styles from "@/styles/auth.module.css";
import { AuthCard } from "./AuthCard";
import { AuthErrorState } from "./AuthErrorState";
import { AuthLoading } from "./AuthLoading";
import { Field } from "./Field";
import { FormAlert } from "./FormAlert";
import { RoleBadge } from "./RoleBadge";
import { SubmitButton } from "./SubmitButton";
import { UserAvatar } from "./UserAvatar";

const SERVER_FIELDS = ["displayName", "avatarUrl", "bio"] as const;

/** FR-AUTH-006/007: xem và cập nhật hồ sơ của người đang đăng nhập. */
export function ProfileForm() {
  const status = useAuthStatus();
  const setUser = useAuthStore((s) => s.setUser);
  const queryClient = useQueryClient();
  const showToast = useAuthUIStore((s) => s.showToast);
  const [serverError, setServerError] = useState<string | null>(null);

  const meQuery = useQuery<UserDto, AppError>({
    queryKey: ["me"],
    queryFn: authApi.me,
    enabled: status === "authenticated",
  });
  const me = meQuery.data;

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isDirty },
  } = useForm<ProfileValues>({
    resolver: zodResolver(profileSchema),
    // Khi dữ liệu từ server đến (hoặc đổi), form tự reset về giá trị này.
    values: me ? { displayName: me.displayName, avatarUrl: me.avatarUrl ?? "", bio: me.bio ?? "" } : undefined,
  });

  const saveMutation = useMutation<UserDto, AppError, ProfileValues>({
    // Backend: field null = giữ nguyên giá trị cũ (không xóa được ảnh qua endpoint này).
    mutationFn: (v) => authApi.updateProfile({ displayName: v.displayName, avatarUrl: v.avatarUrl || null, bio: v.bio }),
    onSuccess: (user) => {
      queryClient.setQueryData(["me"], user);
      setUser(user); // header (UserMenu) đổi tên/ảnh ngay
      showToast("success", "Đã cập nhật hồ sơ.");
    },
    onError: (error) => {
      if (!applyFieldErrors(error, SERVER_FIELDS, setError)) setServerError(error.message);
    },
  });

  const resendMutation = useMutation<ResendConfirmationResponse, AppError, string>({
    mutationFn: (email) => authApi.resendConfirmation(email),
    onSuccess: (r) => showToast("success", r.message),
    onError: (error) => showToast("error", error.message),
  });

  const onSubmit = handleSubmit((values) => {
    setServerError(null);
    saveMutation.mutate(values);
  });

  if (status !== "authenticated" || meQuery.isLoading) {
    return (
      <AuthCard wide>
        <AuthLoading message="Đang tải hồ sơ..." />
      </AuthCard>
    );
  }

  if (meQuery.isError || !me) {
    return (
      <AuthCard wide>
        <AuthErrorState
          title="Không tải được hồ sơ"
          message={meQuery.error?.message}
          onRetry={() => void meQuery.refetch()}
        />
      </AuthCard>
    );
  }

  return (
    <AuthCard wide>
      <div className={styles.profileHead}>
        <UserAvatar name={me.displayName} src={me.avatarUrl} size="lg" />
        <div>
          <h1 className={styles.title}>{me.displayName}</h1>
          <p className={styles.meta}>
            {me.roles.map((role) => (
              <RoleBadge key={role} role={role} />
            ))}
            <span>Tham gia {new Date(me.createdAt).toLocaleDateString("vi-VN")}</span>
          </p>
        </div>
      </div>

      {!me.emailConfirmed && (
        <FormAlert
          variant="warning"
          action={
            <button
              type="button"
              className={`${styles.button} ${styles.buttonSmall}`}
              disabled={resendMutation.isPending}
              onClick={() => resendMutation.mutate(me.email)}
            >
              Gửi lại email xác nhận
            </button>
          }
        >
          Email chưa được xác nhận nên bạn chưa xuất bản được công thức.
        </FormAlert>
      )}
      {serverError && <FormAlert>{serverError}</FormAlert>}

      <form onSubmit={onSubmit} noValidate>
        <Field id="email" label="Email" value={me.email} disabled readOnly />
        <Field label="Tên hiển thị" error={errors.displayName} {...register("displayName")} />
        <Field
          label="URL ảnh đại diện"
          placeholder="https://..."
          hint="Để trống nếu không muốn đổi ảnh."
          error={errors.avatarUrl}
          {...register("avatarUrl")}
        />
        <div className={styles.field}>
          <label className={styles.label} htmlFor="bio">
            Giới thiệu
          </label>
          <textarea
            id="bio"
            className={`${styles.input} ${styles.textarea} ${errors.bio ? styles.inputError : ""}`}
            aria-invalid={errors.bio ? true : undefined}
            {...register("bio")}
          />
          {errors.bio && <span className={styles.error}>{errors.bio.message}</span>}
        </div>
        <SubmitButton loading={saveMutation.isPending} loadingText="Đang lưu..." disabled={!isDirty}>
          Lưu thay đổi
        </SubmitButton>
      </form>
    </AuthCard>
  );
}
