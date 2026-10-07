"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useRef } from "react";
import { logout } from "@/lib/auth/session";
import { useAuthStore } from "@/lib/auth/store";
import { initialsOf } from "@/lib/auth/utils";
import { useAuthUIStore } from "@/store/useAuthUIStore";
import styles from "@/styles/user-menu.module.css";

export function UserMenu() {
  const router = useRouter();
  const status = useAuthStore((s) => s.status);
  const user = useAuthStore((s) => s.user);
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
      <Link href="/auth/login" className={`btn btn-primary btn-sm ${styles.loginLink}`}>
        Đăng nhập
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
        {user.avatarUrl ? (
          // eslint-disable-next-line @next/next/no-img-element
          <img className={styles.avatar} src={user.avatarUrl} alt="" />
        ) : (
          <span className={styles.avatar}>{initialsOf(user.displayName)}</span>
        )}
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
