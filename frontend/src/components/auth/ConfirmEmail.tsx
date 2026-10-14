"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { useEffect, useRef } from "react";
import { authApi } from "@/lib/auth/api";
import type { AppError } from "@/lib/errors";
import { refreshSession } from "@/lib/auth/session";
import { useAuthStore } from "@/store/auth-store";
import { AuthCard } from "./AuthCard";
import { AuthLoading } from "./AuthLoading";
import { FormAlert } from "./FormAlert";

/** FR-AUTH-008: trang đích của liên kết trong email xác nhận. */
export function ConfirmEmail() {
  const params = useSearchParams();
  const userId = params.get("userId");
  const token = params.get("token");
  const started = useRef(false);
  const queryClient = useQueryClient();

  const { mutate, isPending, isSuccess, isError, error } = useMutation<void, AppError, void>({
    mutationFn: () => authApi.confirmEmail(userId!, token!),
    // Đang đăng nhập: lấy token mới để claim email_verified = true (chính sách VerifiedAuthor).
    onSuccess: async () => {
      if (useAuthStore.getState().status === "authenticated") {
        await refreshSession();
        await queryClient.invalidateQueries({ queryKey: ["me"] });
      }
    },
  });

  // Strict Mode chạy effect hai lần ở dev; token xác nhận chỉ dùng được một lần nên phải chặn.
  useEffect(() => {
    if (userId && token && !started.current) {
      started.current = true;
      mutate();
    }
  }, [userId, token, mutate]);

  return (
    <AuthCard title="Xác nhận email">
      {(!userId || !token) && <FormAlert>Liên kết xác nhận không hợp lệ.</FormAlert>}
      {isPending && <AuthLoading message="Đang xác nhận email..." />}
      {isSuccess && (
        <FormAlert variant="success">
          Email đã được xác nhận. <Link href="/auth/login">Đăng nhập</Link>
        </FormAlert>
      )}
      {isError && (
        <FormAlert>
          {error.message} Liên kết có thể đã dùng hoặc hết hạn; vào <Link href="/profile">Hồ sơ cá nhân</Link> để gửi
          lại email xác nhận.
        </FormAlert>
      )}
    </AuthCard>
  );
}
