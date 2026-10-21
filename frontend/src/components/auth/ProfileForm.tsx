"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { Check, Loader2, Lock } from "lucide-react";
import { useEffect, useState } from "react";
import { useForm, useWatch } from "react-hook-form";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { useUpdateProfile } from "@/hooks/useProfileMutations";
import { applyFieldErrors } from "@/lib/errors";
import { cn } from "@/lib/utils";
import { PROFILE_LIMITS, profileSchema, type ProfileValues } from "@/lib/validators/auth";
import type { UserDto } from "@/types/auth";
import { AvatarEditor } from "./AvatarEditor";
import { ConfirmActionDialog } from "./ConfirmActionDialog";
import { Field, FieldMessage } from "./Field";
import { FormAlert } from "./FormAlert";
import { controlClass, outlineButtonClass, primaryButtonClass } from "./profile-styles";
import { SubmitButton } from "./SubmitButton";

const SERVER_FIELDS = ["displayName", "bio"] as const;

const toValues = (user: UserDto): ProfileValues => ({
  // NFC: tên lưu dạng tổ hợp (e + dấu rời) hiển thị và so sánh như tên thường.
  displayName: user.displayName.normalize("NFC"),
  bio: user.bio ?? "",
});

const timeFormat = new Intl.DateTimeFormat("vi-VN", { hour: "2-digit", minute: "2-digit" });

interface ProfileFormProps {
  me: UserDto;
  /** Báo cho trang biết form đang có thay đổi chưa lưu (để chặn rời trang / đăng xuất). */
  onDirtyChange?: (dirty: boolean) => void;
}

/** FR-AUTH-007: ảnh đại diện, tên hiển thị, email (chỉ đọc) và giới thiệu của người đang đăng nhập. */
export function ProfileForm({ me, onDirtyChange }: ProfileFormProps) {
  const save = useUpdateProfile();
  const [serverError, setServerError] = useState<string | null>(null);
  const [savedAt, setSavedAt] = useState<Date | null>(null);
  const [confirmDiscard, setConfirmDiscard] = useState(false);

  const {
    register,
    handleSubmit,
    setError,
    reset,
    control,
    formState: { errors, isDirty },
  } = useForm<ProfileValues>({
    resolver: zodResolver(profileSchema),
    mode: "onTouched", // báo lỗi ngay khi rời ô, không đợi bấm Lưu
    defaultValues: toValues(me),
  });

  const bio = useWatch({ control, name: "bio" }) ?? "";
  const saving = save.isPending;

  useEffect(() => {
    onDirtyChange?.(isDirty);
  }, [isDirty, onDirtyChange]);
  useEffect(() => () => onDirtyChange?.(false), [onDirtyChange]);

  const onSubmit = handleSubmit((values) => {
    setServerError(null);
    save.mutate(values, {
      // Chỉ báo thành công và đặt lại mốc "đã lưu" sau khi máy chủ xác nhận.
      onSuccess: (user) => {
        reset(toValues(user));
        setSavedAt(new Date());
      },
      // Lỗi: giữ nguyên mọi thứ người dùng đã nhập.
      onError: (error) => {
        if (!applyFieldErrors(error, SERVER_FIELDS, setError)) setServerError(error.message);
      },
    });
  });

  const bioOver = bio.length - PROFILE_LIMITS.bioMax;

  return (
    <div className="grid gap-8">
      <AvatarEditor name={me.displayName.normalize("NFC")} avatarUrl={me.avatarUrl} />

      <form onSubmit={onSubmit} noValidate className="grid gap-6">
        {serverError && (
          <FormAlert title="Chưa lưu được hồ sơ">
            {serverError} Những gì bạn đã nhập vẫn được giữ lại.
          </FormAlert>
        )}

        <Field
          id="displayName"
          label="Tên hiển thị"
          autoComplete="nickname"
          hint={`Hiển thị trên các công thức bạn đăng. Từ ${PROFILE_LIMITS.displayNameMin} đến ${PROFILE_LIMITS.displayNameMax} ký tự.`}
          className={controlClass}
          error={errors.displayName}
          {...register("displayName")}
        />

        <div className="grid gap-2">
          <Label htmlFor="email">
            Email
            <span className="inline-flex items-center gap-1 rounded-full bg-muted px-2 py-1 text-xs font-medium text-muted-foreground">
              <Lock className="size-3" aria-hidden="true" /> Chỉ đọc
            </span>
          </Label>
          <Input
            id="email"
            type="email"
            value={me.email}
            readOnly
            aria-readonly="true"
            aria-describedby="email-hint"
            className={cn(controlClass, "cursor-not-allowed border-dashed bg-muted/60 text-muted-foreground")}
          />
          <p id="email-hint" className="text-xs text-muted-foreground">
            Email dùng để đăng nhập và nhận thông báo, nên không thể thay đổi.
          </p>
        </div>

        <div className="grid gap-2">
          <Label htmlFor="bio">Giới thiệu</Label>
          <Textarea
            id="bio"
            rows={5}
            placeholder="Vài dòng về bạn và phong cách nấu ăn yêu thích..."
            className="min-h-32 resize-y leading-relaxed"
            aria-invalid={errors.bio || bioOver > 0 ? true : undefined}
            aria-describedby={errors.bio ? "bio-error bio-count" : "bio-count"}
            {...register("bio")}
          />
          <div className="flex items-start justify-between gap-3">
            <FieldMessage id="bio" error={errors.bio} />
            <span
              id="bio-count"
              className={cn("ml-auto text-xs text-muted-foreground tabular-nums", bioOver > 0 && "font-medium text-destructive")}
            >
              {bio.length}/{PROFILE_LIMITS.bioMax}
              {bioOver > 0 && ` · vượt ${bioOver} ký tự`}
            </span>
          </div>
        </div>

        <div className="flex flex-col gap-4 border-t border-border pt-6 sm:flex-row sm:items-center sm:justify-between">
          <p role="status" aria-live="polite" className="flex min-h-5 items-center gap-2 text-[13px] text-muted-foreground">
            {saving ? (
              <>
                <Loader2 className="size-4 animate-spin" aria-hidden="true" /> Đang lưu...
              </>
            ) : isDirty ? (
              <>
                <span className="size-2 rounded-full bg-amber-500" aria-hidden="true" /> Có thay đổi chưa lưu
              </>
            ) : savedAt ? (
              <>
                <Check className="size-4 text-(--cb-accent-ink)" aria-hidden="true" /> Đã lưu lúc {timeFormat.format(savedAt)}
              </>
            ) : null}
          </p>

          <div className="flex flex-col-reverse gap-3 sm:flex-row">
            <Button
              type="button"
              className={outlineButtonClass}
              disabled={!isDirty || saving}
              onClick={() => setConfirmDiscard(true)}
            >
              Huỷ thay đổi
            </Button>
            <SubmitButton
              className={cn(primaryButtonClass, "sm:w-auto")}
              loading={saving}
              loadingText="Đang lưu..."
              disabled={!isDirty}
            >
              Lưu thay đổi
            </SubmitButton>
          </div>
        </div>
      </form>

      <ConfirmActionDialog
        open={confirmDiscard}
        onOpenChange={setConfirmDiscard}
        title="Bỏ các thay đổi chưa lưu?"
        description="Tên hiển thị và giới thiệu sẽ trở về nội dung đã lưu gần nhất. Việc này không thể hoàn tác."
        confirmLabel="Bỏ thay đổi"
        cancelLabel="Tiếp tục chỉnh sửa"
        onConfirm={() => {
          reset();
          setServerError(null);
        }}
      />
    </div>
  );
}
