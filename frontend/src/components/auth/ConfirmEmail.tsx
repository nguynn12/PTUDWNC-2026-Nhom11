"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { useEffect, useRef } from "react";
import { authApi } from "@/lib/auth/api";
import type { ApiError } from "@/lib/auth/errors";
import { refreshSession } from "@/lib/auth/session";
import { useAuthStore } from "@/lib/auth/store";
import styles from "@/styles/auth.module.css";

export function ConfirmEmail() {
  const params = useSearchParams();
  const userId = params.get("userId");
  const token = params.get("token");
  const started = useRef(false);
  const queryClient = useQueryClient();

  const { mutate, isPending, isSuccess, isError, error } = useMutation<void, ApiError, void>({
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
    <main className={styles.page}>
      <div className={styles.card}>
        <h1 className={styles.title}>Xác nhận email</h1>
        {(!userId || !token) && (
          <div className={`${styles.alert} ${styles.alertError}`}>Liên kết xác nhận không hợp lệ.</div>
        )}
        {isPending && <p>Đang xác nhận...</p>}
        {isSuccess && (
          <div className={`${styles.alert} ${styles.alertSuccess}`}>
            Email đã được xác nhận. <Link href="/auth/login">Đăng nhập</Link>
          </div>
        )}
        {isError && (
          <div className={`${styles.alert} ${styles.alertError}`}>
            {error.message} Link có thể đã dùng hoặc hết hạn; vào Trang cá nhân để gửi lại email xác nhận.
          </div>
        )}
      </div>
    </main>
  );
}
