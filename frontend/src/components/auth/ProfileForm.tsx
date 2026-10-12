"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { authApi } from "@/lib/auth/api";
import type { ApiError } from "@/lib/auth/errors";
import type { UserDto } from "@/lib/auth/types";
import { useAuthStore } from "@/lib/auth/store";
import { initialsOf } from "@/lib/auth/utils";
import { profileSchema, type ProfileValues } from "@/lib/validators/auth";
import { useAuthUIStore } from "@/store/useAuthUIStore";
import styles from "@/styles/auth.module.css";
import { Field } from "./Field";

export function ProfileForm() {
  const status = useAuthStore((s) => s.status);
  const setUser = useAuthStore((s) => s.setUser);
  const queryClient = useQueryClient();
  const showToast = useAuthUIStore((s) => s.showToast);
  const [serverError, setServerError] = useState<string | null>(null);

  const meQuery = useQuery<UserDto, ApiError>({
    queryKey: ["me"],
    queryFn: authApi.me,
    enabled: status === "authenticated",
  });
  const me = meQuery.data;

  const {
    register,
    handleSubmit,
    formState: { errors, isDirty },
  } = useForm<ProfileValues>({
    resolver: zodResolver(profileSchema),
    // Khi dữ liệu từ server đến (hoặc đổi), form tự reset về giá trị này.
    values: me
      ? { displayName: me.displayName, avatarUrl: me.avatarUrl ?? "", bio: me.bio ?? "" }
      : undefined,
  });

  const saveMutation = useMutation<UserDto, ApiError, ProfileValues>({
    // Backend: field null = giữ nguyên giá trị cũ (không xóa được ảnh qua endpoint này).
    mutationFn: (v) =>
      authApi.updateProfile({ displayName: v.displayName, avatarUrl: v.avatarUrl || null, bio: v.bio }),
    onSuccess: (user) => {
      queryClient.setQueryData(["me"], user);
      setUser(user); // header (UserMenu) đổi tên/ảnh ngay
      showToast("success", "Đã cập nhật hồ sơ.");
    },
    onError: (error) => setServerError(error.message),
  });

  const resendMutation = useMutation<{ message: string }, ApiError, string>({
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
      <main className={styles.page}>
        <p>Đang tải hồ sơ...</p>
      </main>
    );
  }

  if (meQuery.isError || !me) {
    return (
      <main className={styles.page}>
        <div className={`${styles.card} ${styles.wide}`}>
          <div className={`${styles.alert} ${styles.alertError}`} role="alert">
            {meQuery.error?.message ?? "Không tải được hồ sơ."}
          </div>
          <button type="button" className={styles.button} onClick={() => meQuery.refetch()}>
            Thử lại
          </button>
        </div>
      </main>
    );
  }

  return (
    <main className={styles.page}>
      <div className={`${styles.card} ${styles.wide}`}>
        <div className={styles.profileHead}>
          {me.avatarUrl ? (
            // eslint-disable-next-line @next/next/no-img-element
            <img className={styles.avatar} src={me.avatarUrl} alt="" />
          ) : (
            <div className={styles.avatar}>{initialsOf(me.displayName)}</div>
          )}
          <div>
            <h1 className={styles.title}>{me.displayName}</h1>
            <p className={styles.meta}>
              {me.roles.map((role) => (
                <span key={role} className={styles.badge}>
                  {role}
                </span>
              ))}
              Tham gia {new Date(me.createdAt).toLocaleDateString("vi-VN")}
            </p>
          </div>
        </div>

        {!me.emailConfirmed && (
          <div className={`${styles.alert} ${styles.alertWarning}`} role="status">
            <span>Email chưa được xác nhận.</span>
            <button
              type="button"
              className={`${styles.button} ${styles.buttonSmall}`}
              disabled={resendMutation.isPending}
              onClick={() => resendMutation.mutate(me.email)}
            >
              Gửi lại email xác nhận
            </button>
          </div>
        )}
        {serverError && (
          <div className={`${styles.alert} ${styles.alertError}`} role="alert">
            {serverError}
          </div>
        )}

        <form onSubmit={onSubmit} noValidate>
          <Field id="email" label="Email" value={me.email} disabled readOnly />
          <Field label="Tên hiển thị" error={errors.displayName} {...register("displayName")} />
          <Field label="URL ảnh đại diện" placeholder="https://..." error={errors.avatarUrl} {...register("avatarUrl")} />
          <div className={styles.field}>
            <label className={styles.label} htmlFor="bio">
              Giới thiệu
            </label>
            <textarea
              id="bio"
              className={`${styles.input} ${styles.textarea} ${errors.bio ? styles.inputError : ""}`}
              {...register("bio")}
            />
            {errors.bio && <span className={styles.error}>{errors.bio.message}</span>}
          </div>
          <button type="submit" className={styles.button} disabled={!isDirty || saveMutation.isPending}>
            {saveMutation.isPending ? "Đang lưu..." : "Lưu thay đổi"}
          </button>
        </form>
      </div>
    </main>
  );
}
