"use client";

import { CircleAlert, ImageUp, Loader2, Trash2 } from "lucide-react";
import { useEffect, useRef, useState, type ChangeEvent } from "react";
import { Button } from "@/components/ui/button";
import { useRemoveAvatar, useUploadAvatar } from "@/hooks/useProfileMutations";
import {
  AVATAR_ACCEPT,
  AVATAR_MAX_BYTES,
  AVATAR_UPLOAD_ENABLED,
  formatFileSize,
  validateAvatarFile,
} from "@/lib/auth/avatar";
import { ConfirmActionDialog } from "./ConfirmActionDialog";
import { outlineButtonClass, primaryButtonClass } from "./profile-styles";
import { UserAvatar } from "./UserAvatar";

interface AvatarEditorProps {
  name: string;
  avatarUrl: string | null;
  /** Mặc định theo cờ môi trường; truyền tường minh khi cần (ví dụ kiểm thử). */
  canUpload?: boolean;
}

interface Selection {
  file: File;
  previewUrl: string;
}

/**
 * Quản lý ảnh đại diện (thay cho ô nhập URL): xem ảnh hiện tại hoặc ảnh mặc định, chọn ảnh từ thiết bị,
 * xem trước rồi mới xác nhận, tải lên, xoá. Ảnh được lưu ngay khi xác nhận (không chờ nút "Lưu thay đổi"
 * của form), nên khu vực này tách khỏi form và có thông báo trạng thái riêng.
 */
export function AvatarEditor({ name, avatarUrl, canUpload = AVATAR_UPLOAD_ENABLED }: AvatarEditorProps) {
  const inputRef = useRef<HTMLInputElement>(null);
  const previewUrlRef = useRef<string | null>(null);
  const [selection, setSelection] = useState<Selection | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [progress, setProgress] = useState(0);
  const [confirmRemove, setConfirmRemove] = useState(false);

  const upload = useUploadAvatar();
  const remove = useRemoveAvatar();
  const uploading = upload.isPending;
  const removing = remove.isPending;
  const busy = uploading || removing;
  const hasAvatar = Boolean(avatarUrl);

  // Giải phóng URL xem trước khi rời trang để không rò bộ nhớ.
  useEffect(
    () => () => {
      if (previewUrlRef.current) URL.revokeObjectURL(previewUrlRef.current);
    },
    [],
  );

  function discardSelection() {
    if (previewUrlRef.current) URL.revokeObjectURL(previewUrlRef.current);
    previewUrlRef.current = null;
    setSelection(null);
    setProgress(0);
  }

  async function onFileChange(event: ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0];
    event.target.value = ""; // cho phép chọn lại đúng tệp vừa chọn
    if (!file) return;

    setError(null);
    const problem = await validateAvatarFile(file);
    if (problem) {
      setError(problem);
      return;
    }
    if (previewUrlRef.current) URL.revokeObjectURL(previewUrlRef.current);
    const previewUrl = URL.createObjectURL(file);
    previewUrlRef.current = previewUrl;
    setSelection({ file, previewUrl });
  }

  function confirmUpload() {
    if (!selection) return;
    setError(null);
    setProgress(0);
    upload.mutate(
      { file: selection.file, onProgress: setProgress },
      {
        onSuccess: discardSelection,
        onError: (e) => setError(e.message),
      },
    );
  }

  function confirmRemoval() {
    setError(null);
    remove.mutate(undefined, { onError: (e) => setError(e.message) });
  }

  const removeDescription = canUpload
    ? "Tài khoản sẽ hiển thị chữ cái đầu của tên bạn thay cho ảnh. Bạn có thể chọn ảnh khác bất cứ lúc nào."
    : "Tài khoản sẽ hiển thị chữ cái đầu của tên bạn thay cho ảnh. Hiện chưa thể tải ảnh mới từ thiết bị, nên ảnh này sẽ không khôi phục được.";

  return (
    <div role="group" aria-labelledby="avatar-title" aria-busy={busy || undefined} className="flex flex-col gap-4 sm:flex-row sm:items-center sm:gap-6">
      {selection ? (
        // eslint-disable-next-line @next/next/no-img-element -- blob: URL cục bộ, next/image không áp dụng được
        <img
          src={selection.previewUrl}
          alt="Xem trước ảnh đại diện mới"
          className="size-24 shrink-0 rounded-full object-cover ring-1 ring-border"
          onError={() => {
            discardSelection();
            setError("Không đọc được ảnh này. Vui lòng chọn ảnh khác.");
          }}
        />
      ) : (
        <UserAvatar name={name} src={avatarUrl} size="xl" className="shrink-0 ring-1 ring-border" />
      )}

      <div className="grid min-w-0 flex-1 gap-3">
        <div className="grid gap-1">
          <p id="avatar-title" className="text-sm leading-none font-medium">
            Ảnh đại diện
          </p>
          <p id="avatar-hint" className="text-[13px] leading-snug text-muted-foreground">
            {selection
              ? `${selection.file.name} · ${formatFileSize(selection.file.size)}`
              : canUpload
                ? `JPG, PNG hoặc WebP, tối đa ${formatFileSize(AVATAR_MAX_BYTES)}.`
                : "Chưa hỗ trợ tải ảnh từ thiết bị trên máy chủ hiện tại."}
          </p>
        </div>

        {uploading && (
          <div className="grid gap-1.5" role="status">
            <div
              role="progressbar"
              aria-label="Tiến độ tải ảnh lên"
              aria-valuemin={0}
              aria-valuemax={100}
              aria-valuenow={progress}
              className="h-1.5 w-full max-w-xs overflow-hidden rounded-full bg-muted"
            >
              <div className="h-full rounded-full bg-primary transition-[width]" style={{ width: `${progress}%` }} />
            </div>
            <p className="text-[13px] text-muted-foreground">Đang tải ảnh lên… {progress}%</p>
          </div>
        )}

        <div className="flex flex-wrap gap-2">
          {canUpload && (
            <input
              ref={inputRef}
              type="file"
              accept={AVATAR_ACCEPT}
              className="sr-only"
              tabIndex={-1}
              aria-hidden="true"
              onChange={onFileChange}
            />
          )}

          {selection ? (
            <>
              <Button type="button" className={primaryButtonClass} disabled={uploading} onClick={confirmUpload}>
                {uploading && <Loader2 className="animate-spin" aria-hidden="true" />}
                {uploading ? "Đang tải lên..." : "Dùng ảnh này"}
              </Button>
              <Button
                type="button"
                className={outlineButtonClass}
                disabled={uploading}
                onClick={() => inputRef.current?.click()}
              >
                Chọn ảnh khác
              </Button>
              <Button type="button" className={outlineButtonClass} disabled={uploading} onClick={discardSelection}>
                Huỷ
              </Button>
            </>
          ) : (
            <>
              {canUpload && (
                <Button
                  type="button"
                  className={outlineButtonClass}
                  disabled={busy}
                  aria-describedby="avatar-hint"
                  onClick={() => inputRef.current?.click()}
                >
                  <ImageUp aria-hidden="true" /> {hasAvatar ? "Đổi ảnh" : "Chọn ảnh"}
                </Button>
              )}
              {hasAvatar && (
                <Button type="button" className={outlineButtonClass} disabled={busy} onClick={() => setConfirmRemove(true)}>
                  {removing ? <Loader2 className="animate-spin" aria-hidden="true" /> : <Trash2 aria-hidden="true" />}
                  {removing ? "Đang xoá..." : "Xoá ảnh"}
                </Button>
              )}
            </>
          )}
        </div>

        {error && (
          <p role="alert" className="flex items-start gap-1.5 text-sm text-destructive">
            <CircleAlert className="mt-0.5 size-4 shrink-0" aria-hidden="true" />
            <span>{error}</span>
          </p>
        )}
      </div>

      <ConfirmActionDialog
        open={confirmRemove}
        onOpenChange={setConfirmRemove}
        title="Xoá ảnh đại diện?"
        description={removeDescription}
        confirmLabel="Xoá ảnh"
        cancelLabel="Giữ ảnh"
        tone="danger"
        onConfirm={confirmRemoval}
      />
    </div>
  );
}
