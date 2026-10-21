"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { useForm, useWatch } from "react-hook-form";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { authApi } from "@/lib/auth/api";
import { applyFieldErrors, type AppError } from "@/lib/errors";
import { cn } from "@/lib/utils";
import { profileSchema, type ProfileValues } from "@/lib/validators/auth";
import { useAuthStore } from "@/store/auth-store";
import { useAuthUIStore } from "@/store/auth-ui-store";
import type { UserDto } from "@/types/auth";
import { Field, FieldMessage } from "./Field";
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
    <form onSubmit={onSubmit} noValidate className="grid gap-5">
      {serverError && <FormAlert>{serverError}</FormAlert>}

      <div className="flex flex-col gap-4 sm:flex-row sm:items-start">
        <UserAvatar
          name={displayName || me.displayName}
          src={IMAGE_URL.test(avatarUrl) ? avatarUrl : me.avatarUrl}
          size="lg"
          className="ring-4 ring-primary/10"
        />
        <div className="min-w-0 flex-1">
          <Field
            label="URL ảnh đại diện"
            placeholder="https://..."
            hint="Dán liên kết ảnh (JPG, PNG). Để trống nếu không muốn đổi ảnh."
            error={errors.avatarUrl}
            {...register("avatarUrl")}
          />
        </div>
      </div>

      <div className="grid gap-5 sm:grid-cols-2">
        <Field label="Tên hiển thị" error={errors.displayName} {...register("displayName")} />
        <Field id="email" label="Email" value={me.email} hint="Email đăng nhập, không đổi được." disabled readOnly />
      </div>

      <div className="grid gap-2">
        <Label htmlFor="bio">Giới thiệu</Label>
        <Textarea
          id="bio"
          rows={4}
          placeholder="Vài dòng về bạn và phong cách nấu ăn yêu thích..."
          className="min-h-28"
          aria-invalid={errors.bio ? true : undefined}
          aria-describedby={errors.bio ? "bio-error" : undefined}
          {...register("bio")}
        />
        <div className="flex items-start justify-between gap-3">
          <FieldMessage id="bio" error={errors.bio} />
          <span
            className={cn("ml-auto text-xs text-muted-foreground", bioLength > BIO_MAX && "text-destructive")}
            aria-live="polite"
          >
            {bioLength}/{BIO_MAX}
          </span>
        </div>
      </div>

      <div className="flex flex-col-reverse gap-3 border-t pt-5 sm:flex-row sm:justify-end">
        <Button
          type="button"
          variant="outline"
          size="lg"
          className="h-10 hover:bg-muted hover:text-foreground"
          disabled={!isDirty || saveMutation.isPending}
          onClick={() => {
            reset();
            setServerError(null);
          }}
        >
          Huỷ thay đổi
        </Button>
        <SubmitButton className="sm:w-auto" loading={saveMutation.isPending} loadingText="Đang lưu..." disabled={!isDirty}>
          Lưu thay đổi
        </SubmitButton>
      </div>
    </form>
  );
}
