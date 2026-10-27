"use client";

import { ShieldAlert } from "lucide-react";
import Link from "next/link";
import type { ReactNode } from "react";
import { Button } from "@/components/ui/button";
import { useHasRole } from "@/store/auth-store";
import type { UserRole } from "@/types/auth";
import { AuthCard } from "./AuthCard";
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
            <section className="mx-auto flex w-full max-w-6xl justify-center px-4 py-16">
              <AuthCard>
                <div className="flex flex-col items-center gap-3 py-4 text-center">
                  <span className="flex size-12 items-center justify-center rounded-full bg-amber-100 text-amber-700">
                    <ShieldAlert className="size-6" aria-hidden="true" />
                  </span>
                  <h1 className="font-serif text-xl font-bold">Không có quyền truy cập</h1>
                  <p className="text-sm text-muted-foreground">
                    Trang này chỉ dành cho {role === "Admin" ? "Quản trị viên" : "Tác giả"}.
                  </p>
                  <Button asChild className="mt-2">
                    <Link href="/">Về trang chủ</Link>
                  </Button>
                </div>
              </AuthCard>
            </section>
          ))}
    </RequireAuth>
  );
}
