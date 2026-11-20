"use client";

import { useEffect, useState, type ReactNode } from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  BookOpen,
  ClipboardList,
  Flame,
  Home,
  LayoutDashboard,
  Menu,
  PlusCircle,
  Tags,
  Trash2,
  Users,
  X,
  type LucideIcon,
} from "lucide-react";
import { Sheet, SheetClose, SheetContent, SheetTitle, SheetTrigger } from "@/components/ui/sheet";
import { initialsOf } from "@/lib/auth/utils";
import { useCurrentUser, useHasRole } from "@/store/auth-store";

const navigation: ReadonlyArray<{
  href: string;
  label: string;
  icon: LucideIcon;
  exact?: boolean;
  adminOnly?: boolean;
}> = [
  { href: "/dashboard", label: "Tổng quan", icon: LayoutDashboard, exact: true },
  { href: "/dashboard/recipes", label: "Công thức của tôi", icon: BookOpen },
  { href: "/dashboard/recipes/new", label: "Tạo công thức mới", icon: PlusCircle },
  { href: "/dashboard/categories", label: "Danh mục món ăn", icon: Tags, adminOnly: true },
  { href: "/admin/recipes/trash", label: "Thùng rác công thức", icon: Trash2, adminOnly: true },
  { href: "/admin/users", label: "Người dùng", icon: Users, adminOnly: true },
  { href: "/admin/system-logs", label: "Nhật ký hệ thống", icon: ClipboardList, adminOnly: true },
  { href: "/", label: "Về trang chủ", icon: Home, exact: true },
];

function ManagementSidebar({ onNavigate }: { onNavigate?: () => void }) {
  const pathname = usePathname();
  const user = useCurrentUser();
  const isAdmin = useHasRole("Admin");
  const items = navigation.filter(item => isAdmin || !item.adminOnly);
  const active = items.findLast(item => pathname === item.href || (!item.exact && pathname.startsWith(item.href + "/")));

  return (
    <>
      <div className="sidebar-brand">
        <div className="sidebar-logo-icon"><Flame size={18} aria-hidden="true" /></div>
        {onNavigate ? (
          <>
            <SheetTitle className="sidebar-brand-title">Culinary Blog</SheetTitle>
            <SheetClose className="sidebar-close" aria-label="Đóng menu quản trị">
              <X size={20} aria-hidden="true" />
            </SheetClose>
          </>
        ) : (
          <div className="sidebar-brand-title">Culinary Blog</div>
        )}
      </div>

      <nav className="sidebar-nav" aria-label="Điều hướng quản trị">
        {items.map(({ href, label, icon: Icon }) => (
          <Link
            key={href}
            href={href}
            className="sidebar-link"
            aria-current={active?.href === href ? (pathname === href ? "page" : "location") : undefined}
            onClick={onNavigate}
          >
            <Icon size={18} aria-hidden="true" />
            {label}
          </Link>
        ))}
      </nav>

      <div className="sidebar-footer">
        <div className="user-profile-widget">
          <div className="user-avatar" aria-hidden="true">{initialsOf(user?.displayName)}</div>
          <div className="min-w-0">
            <div className="user-info-name">{user?.displayName ?? "Người dùng"}</div>
            <div className="user-info-role">{isAdmin ? "Quản trị viên" : "Tác giả"}</div>
          </div>
        </div>
      </div>
    </>
  );
}

function MobileManagementNavigation() {
  const [open, setOpen] = useState(false);

  useEffect(() => {
    const desktop = window.matchMedia("(width >= 64rem)");
    const closeOnDesktop = (event: MediaQueryListEvent) => {
      if (event.matches) setOpen(false);
    };
    desktop.addEventListener("change", closeOnDesktop);
    return () => desktop.removeEventListener("change", closeOnDesktop);
  }, []);

  return (
    <Sheet open={open} onOpenChange={setOpen}>
      <div className="dashboard-toolbar">
        <SheetTrigger className="btn btn-secondary btn-sm" aria-label="Mở menu quản trị">
          <Menu size={20} aria-hidden="true" />
        </SheetTrigger>
      </div>
      <SheetContent side="left" className="dashboard-sidebar-drawer" showCloseButton={false}>
        <ManagementSidebar onNavigate={() => setOpen(false)} />
      </SheetContent>
    </Sheet>
  );
}

export function ManagementShell({ children }: { children: ReactNode }) {
  return (
    <div className="dashboard-wrapper">
      <aside className="dashboard-sidebar"><ManagementSidebar /></aside>
      <div className="dashboard-main">
        <div className="management-container">
          <MobileManagementNavigation />
          {children}
        </div>
      </div>
    </div>
  );
}
