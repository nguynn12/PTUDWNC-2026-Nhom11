"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { useForm, useWatch } from "react-hook-form";
import { authApi } from "@/lib/auth/api";
import { applyFieldErrors, type AppError } from "@/lib/errors";
import { profileSchema, type ProfileValues } from "@/lib/validators/auth";
import { useAuthStore } from "@/store/auth-store";
import { useAuthUIStore } from "@/store/auth-ui-store";
import type { UserDto } from "@/types/auth";
import styles from "@/styles/auth.module.css";
import { Field } from "./Field";
import { FormAlert } from "./FormAlert";
import { SubmitButton } from "./SubmitButton";
import { UserAvatar } from "./UserAvatar";

const SERVER_FIELDS = ["displayName", "avatarUrl", "bio"] as const;
const BIO_MAX = 1000;
const IMAGE_URL = /^https?:\/\/\S+$/i;

/** FR-AUTH-007: cập nhật tên hiển thị, ảnh đại diện, giới thiệu của người đang đăng nhập. */
export function ProfileForm({ me }: { me: UserDto }) {
  const setUser = useAuthStore((s) => s.setUser);
  const queryClient = useQueryClient();
  const showToast = useAuthUIStore((s) => s.showToast);
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    setError,
    reset,
    control,
    formState: { errors, isDirty },
  } = useForm<ProfileValues>({
    resolver: zodResolver(profileSchema),
    // Khi dữ liệu từ server đổi, form tự reset về giá trị này.
    values: { displayName: me.displayName, avatarUrl: me.avatarUrl ?? "", bio: me.bio ?? "" },
  });

  const [displayName, rawAvatarUrl, bio] = useWatch({ control, name: ["displayName", "avatarUrl", "bio"] });
  const avatarUrl = (rawAvatarUrl ?? "").trim();
  const bioLength = (bio ?? "").length;

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

  const onSubmit = handleSubmit((values) => {
    setServerError(null);
    saveMutation.mutate(values);
  });

  return (
    <form onSubmit={onSubmit} noValidate>
      {serverError && <FormAlert>{serverError}</FormAlert>}

      <div className={styles.avatarField}>
        <UserAvatar
          name={displayName || me.displayName}
          src={IMAGE_URL.test(avatarUrl) ? avatarUrl : me.avatarUrl}
          size="lg"
        />
        <div className={styles.avatarFieldInput}>
          <Field
            label="URL ảnh đại diện"
            placeholder="https://..."
            hint="Dán liên kết ảnh (JPG, PNG). Để trống nếu không muốn đổi ảnh."
            error={errors.avatarUrl}
            {...register("avatarUrl")}
          />
        </div>
      </div>

      <div className={styles.formGrid}>
        <Field label="Tên hiển thị" error={errors.displayName} {...register("displayName")} />
        <Field id="email" label="Email" value={me.email} hint="Email đăng nhập, không đổi được." disabled readOnly />
      </div>

      <div className={styles.field}>
        <label className={styles.label} htmlFor="bio">
          Giới thiệu
        </label>
        <textarea
          id="bio"
          rows={4}
          placeholder="Vài dòng về bạn và phong cách nấu ăn yêu thích..."
          className={`${styles.input} ${styles.textarea} ${errors.bio ? styles.inputError : ""}`}
          aria-invalid={errors.bio ? true : undefined}
          aria-describedby="bio-count"
          {...register("bio")}
        />
        <div className={styles.fieldFoot}>
          {errors.bio ? <span className={styles.error}>{errors.bio.message}</span> : <span />}
          <span id="bio-count" className={`${styles.fieldHint} ${bioLength > BIO_MAX ? styles.error : ""}`}>
            {bioLength}/{BIO_MAX}
          </span>
        </div>
      </div>

      <div className={styles.formActions}>
        <button
          type="button"
          className={`${styles.button} ${styles.buttonGhost}`}
          disabled={!isDirty || saveMutation.isPending}
          onClick={() => {
            reset();
            setServerError(null);
          }}
        >
          Huỷ thay đổi
        </button>
        <SubmitButton
          className={styles.buttonAuto}
          loading={saveMutation.isPending}
          loadingText="Đang lưu..."
          disabled={!isDirty}
        >
          Lưu thay đổi
        </SubmitButton>
      </div>
    </form>
  );
}
