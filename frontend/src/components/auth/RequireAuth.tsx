"use client";

import { usePathname, useRouter } from "next/navigation";
import { useEffect, type ReactNode } from "react";
import { useAuthStore } from "@/lib/auth/store";
import styles from "@/styles/auth.module.css";

/** Bảo vệ route phía client (SRS 5.1: /profile, /dashboard/* bắt buộc đăng nhập). */
export function RequireAuth({ children }: { children: ReactNode }) {
  const status = useAuthStore((s) => s.status);
  const router = useRouter();
  const pathname = usePathname();

  useEffect(() => {
    if (status === "unauthenticated") {
      router.replace(`/auth/login?callbackUrl=${encodeURIComponent(pathname)}`);
    }
  }, [status, router, pathname]);

  if (status !== "authenticated") {
    return (
      <main className={styles.page}>
        <p>Đang kiểm tra phiên đăng nhập...</p>
      </main>
    );
  }
  return <>{children}</>;
}
