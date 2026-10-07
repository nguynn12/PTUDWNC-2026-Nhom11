"use client";

import { useEffect } from "react";
import { useAuthUIStore } from "@/store/useAuthUIStore";
import styles from "@/styles/user-menu.module.css";

export function AuthToastHost() {
  const toast = useAuthUIStore((s) => s.toast);
  const clearToast = useAuthUIStore((s) => s.clearToast);

  useEffect(() => {
    if (!toast) return;
    const id = setTimeout(clearToast, 4000);
    return () => clearTimeout(id);
  }, [toast, clearToast]);

  if (!toast) return null;
  return (
    <div role="status" className={`${styles.toast} ${toast.kind === "error" ? styles.toastError : styles.toastSuccess}`}>
      {toast.message}
    </div>
  );
}
