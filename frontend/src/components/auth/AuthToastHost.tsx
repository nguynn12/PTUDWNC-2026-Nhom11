"use client";

import { CircleAlert, CircleCheck, X } from "lucide-react";
import { useEffect } from "react";
import { cn } from "@/lib/utils";
import { useAuthUIStore } from "@/store/auth-ui-store";

/** Thông báo nhanh của module Xác thực (đăng ký thành công, đã lưu hồ sơ...). */
export function AuthToastHost() {
  const toast = useAuthUIStore((s) => s.toast);
  const clearToast = useAuthUIStore((s) => s.clearToast);

  useEffect(() => {
    if (!toast) return;
    const id = setTimeout(clearToast, 4000);
    return () => clearTimeout(id);
  }, [toast, clearToast]);

  if (!toast) return null;
  const isError = toast.kind === "error";
  const Icon = isError ? CircleAlert : CircleCheck;
  return (
    <div
      role={isError ? "alert" : "status"}
      className={cn(
        "fixed right-4 bottom-4 z-[60] flex max-w-sm items-start gap-3 rounded-lg border bg-card p-4 text-sm shadow-lg animate-in fade-in-0 slide-in-from-bottom-4",
        isError ? "border-destructive/40 text-destructive" : "border-secondary/40 text-secondary",
      )}
    >
      <Icon className="mt-0.5 size-4 shrink-0" aria-hidden="true" />
      <p className="flex-1 text-foreground">{toast.message}</p>
      <button
        type="button"
        onClick={clearToast}
        className="text-muted-foreground hover:text-foreground"
        aria-label="Đóng thông báo"
      >
        <X className="size-4" />
      </button>
    </div>
  );
}
