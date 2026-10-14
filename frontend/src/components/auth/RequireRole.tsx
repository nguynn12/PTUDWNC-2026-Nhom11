"use client";

import Link from "next/link";
import type { ReactNode } from "react";
import { useHasRole } from "@/store/auth-store";
import type { UserRole } from "@/types/auth";
import styles from "@/styles/auth.module.css";
import { AuthCard } from "./AuthCard";
import { FormAlert } from "./FormAlert";
import { RequireAuth } from "./RequireAuth";

interface RequireRoleProps {
  role: UserRole;
  children: ReactNode;
  /** Nội dung thay thế khi không đủ quyền (mặc định: thông báo và nút về trang chủ). */
  fallback?: ReactNode;
}

/**
 * Chặn theo vai trò phía client (SRS 2.3, 5.1: /dashboard/categories chỉ Admin). Đây là lớp giao diện;
 * quyền thật vẫn do backend kiểm tra bằng policy Admin/Author.
 */
export function RequireRole({ role, children, fallback }: RequireRoleProps) {
  const allowed = useHasRole(role);

  return (
    <RequireAuth>
      {allowed
        ? children
        : (fallback ?? (
            <section className={styles.page}>
              <AuthCard title="Không có quyền truy cập">
                <FormAlert variant="warning">
                  Trang này chỉ dành cho {role === "Admin" ? "Quản trị viên" : "Tác giả"}.
                </FormAlert>
                <Link href="/" className={styles.button}>
                  Về trang chủ
                </Link>
              </AuthCard>
            </section>
          ))}
    </RequireAuth>
  );
}
