"use client";

import { usePathname, useRouter } from "next/navigation";
import { useEffect, type ReactNode } from "react";
import { useAuthStatus } from "@/store/auth-store";
import styles from "@/styles/auth.module.css";
import { AuthLoading } from "./AuthLoading";

/** Bảo vệ route phía client (SRS 5.1: /profile, /dashboard/* bắt buộc đăng nhập). */
export function RequireAuth({ children }: { children: ReactNode }) {
  const status = useAuthStatus();
  const router = useRouter();
  const pathname = usePathname();

  useEffect(() => {
    if (status === "unauthenticated") {
      router.replace(`/auth/login?callbackUrl=${encodeURIComponent(pathname)}`);
    }
  }, [status, router, pathname]);

  if (status !== "authenticated") {
    return (
      <section className={styles.page}>
        <AuthLoading message="Đang kiểm tra phiên đăng nhập..." />
      </section>
    );
  }
  return <>{children}</>;
}
