"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useCallback } from "react";
import { authApi } from "@/lib/auth/api";
import type { AppError } from "@/lib/errors";
import type { ProfileValues } from "@/lib/validators/auth";
import { useAuthStore } from "@/store/auth-store";
import { useAuthUIStore } from "@/store/auth-ui-store";
import type { ResendConfirmationResponse, UserDto } from "@/types/auth";

/**
 * Mọi thay đổi hồ sơ đi qua đây để header (auth-store) và trang hồ sơ (query ["me"]) luôn dùng
 * cùng một dữ liệu người dùng — đổi tên hay ảnh là header đổi theo ngay.
 */
export function useSyncUser() {
  const queryClient = useQueryClient();
  const setUser = useAuthStore((s) => s.setUser);
  return useCallback(
    (user: UserDto) => {
      queryClient.setQueryData(["me"], user);
      setUser(user);
    },
    [queryClient, setUser],
  );
}

/** FR-AUTH-007: cập nhật tên hiển thị và giới thiệu (PATCH /auth/me). */
export function useUpdateProfile() {
  const syncUser = useSyncUser();
  const showToast = useAuthUIStore((s) => s.showToast);
  return useMutation<UserDto, AppError, ProfileValues>({
    mutationFn: (v) => authApi.updateProfile({ displayName: v.displayName, bio: v.bio }),
    onSuccess: (user) => {
      syncUser(user);
      showToast("success", "Đã cập nhật hồ sơ.");
    },
  });
}

/** Xoá ảnh đại diện (PATCH /auth/me với avatarUrl rỗng). */
export function useRemoveAvatar() {
  const syncUser = useSyncUser();
  const showToast = useAuthUIStore((s) => s.showToast);
  return useMutation<UserDto, AppError, void>({
    mutationFn: () => authApi.removeAvatar(),
    onSuccess: (user) => {
      syncUser(user);
      showToast("success", "Đã xoá ảnh đại diện.");
    },
  });
}

/** Tải ảnh đại diện mới — chỉ dùng khi backend có POST /auth/me/avatar (AVATAR_UPLOAD_ENABLED). */
export function useUploadAvatar() {
  const syncUser = useSyncUser();
  const showToast = useAuthUIStore((s) => s.showToast);
  return useMutation<UserDto, AppError, { file: File; onProgress?: (percent: number) => void }>({
    mutationFn: ({ file, onProgress }) => authApi.uploadAvatar(file, onProgress),
    onSuccess: (user) => {
      syncUser(user);
      showToast("success", "Đã cập nhật ảnh đại diện.");
    },
  });
}

/** FR-AUTH-009: gửi lại email xác nhận (luôn 202 kèm thông điệp chung). */
export function useResendConfirmation() {
  const showToast = useAuthUIStore((s) => s.showToast);
  return useMutation<ResendConfirmationResponse, AppError, string>({
    mutationFn: (email) => authApi.resendConfirmation(email),
    onSuccess: (r) => showToast("success", r.message),
    onError: (error) => showToast("error", error.message),
  });
}
