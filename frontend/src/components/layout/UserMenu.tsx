"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useRef } from "react";
import { logout } from "@/lib/auth/session";
import { useAuthStatus, useCurrentUser } from "@/store/auth-store";
import { useAuthUIStore } from "@/store/auth-ui-store";
import styles from "@/styles/user-menu.module.css";
import { UserAvatar } from "@/components/auth/UserAvatar";

export function UserMenu() {
  const router = useRouter();
  const status = useAuthStatus();
  const user = useCurrentUser();
  const open = useAuthUIStore((s) => s.userMenuOpen);
  const setOpen = useAuthUIStore((s) => s.setUserMenuOpen);
  const wrapRef = useRef<HTMLDivElement>(null);

  // Đóng menu khi bấm ra ngoài hoặc nhấn Esc.
  useEffect(() => {
    if (!open) return;
    const onPointerDown = (e: PointerEvent) => {
      if (!wrapRef.current?.contains(e.target as Node)) setOpen(false);
    };
    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") setOpen(false);
    };
    document.addEventListener("pointerdown", onPointerDown);
    document.addEventListener("keydown", onKeyDown);
    return () => {
      document.removeEventListener("pointerdown", onPointerDown);
      document.removeEventListener("keydown", onKeyDown);
    };
  }, [open, setOpen]);

  if (status === "loading") return null;

  if (status !== "authenticated" || !user) {
    return (
      <Link href="/auth/login" className={`btn btn-primary btn-sm ${styles.loginLink}`} aria-label="Đăng nhập">
        <svg className={styles.loginIcon} viewBox="0 0 24 24" width="18" height="18" aria-hidden="true">
          <path
            fill="currentColor"
            d="M12 12a5 5 0 1 0 0-10 5 5 0 0 0 0 10Zm0 2c-4.42 0-8 2.24-8 5v1a1 1 0 0 0 1 1h14a1 1 0 0 0 1-1v-1c0-2.76-3.58-5-8-5Z"
          />
        </svg>
        <span className={styles.loginText}>Đăng nhập</span>
      </Link>
    );
  }

  return (
    <div className={styles.menuWrap} ref={wrapRef}>
      <button
        type="button"
        className={styles.trigger}
        aria-haspopup="menu"
        aria-expanded={open}
        onClick={() => setOpen(!open)}
      >
        <UserAvatar name={user.displayName} src={user.avatarUrl} size="sm" />
        <span className={styles.name}>{user.displayName}</span>
      </button>

      {open && (
        <div className={styles.menu} role="menu">
          <Link href="/profile" role="menuitem" className={styles.menuItem} onClick={() => setOpen(false)}>
            Hồ sơ cá nhân
          </Link>
          <button
            type="button"
            role="menuitem"
            className={styles.menuItem}
            onClick={async () => {
              setOpen(false);
              await logout();
              router.replace("/auth/login");
            }}
          >
            Đăng xuất
          </button>
        </div>
      )}
    </div>
  );
}
