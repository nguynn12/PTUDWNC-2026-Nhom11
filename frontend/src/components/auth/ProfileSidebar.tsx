"use client";

import {
  BookOpen,
  Compass,
  FolderTree,
  LayoutDashboard,
  Menu,
  NotebookPen,
  ShieldCheck,
  Trash2,
  UserRound,
  type LucideIcon,
} from "lucide-react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { useState } from "react";
import { cn } from "@/lib/utils";
import { hasRole } from "@/store/auth-store";
import type { UserDto } from "@/types/auth";

export type ProfileSection = "profile" | "security";

/** id của vùng nội dung ứng với từng mục (cũng là hash trên URL). */
export const SECTION_ANCHORS: Record<ProfileSection, string> = {
  profile: "thong-tin-ca-nhan",
  security: "bao-mat",
};

type NavItem =
  | { kind: "section"; id: ProfileSection; label: string; icon: LucideIcon }
  | { kind: "link"; href: string; label: string; icon: LucideIcon };

interface NavGroup {
  label: string;
  items: NavItem[];
}

/**
 * Nhóm điều hướng theo vai trò (SRS 2.3: Admin có mọi quyền của Author). Route và quyền giữ nguyên như lối tắt cũ;
 * mục "Công thức đã lưu" chưa có chức năng nên chưa đưa vào.
 */
function navGroupsFor(user: UserDto): NavGroup[] {
  const isAdmin = hasRole(user, "Admin");
  const isAuthor = isAdmin || hasRole(user, "Author");

  const groups: NavGroup[] = [
    {
      label: "Tài khoản",
      items: [
        { kind: "section", id: "profile", label: "Hồ sơ cá nhân", icon: UserRound },
        { kind: "section", id: "security", label: "Bảo mật tài khoản", icon: ShieldCheck },
      ],
    },
  ];

  if (isAuthor) {
    groups.push({
      label: "Công thức của tôi",
      items: [
        { kind: "link", href: "/dashboard/recipes/new", label: "Viết công thức mới", icon: NotebookPen },
        { kind: "link", href: "/dashboard/recipes", label: "Công thức của tôi", icon: BookOpen },
      ],
    });
  }

  const explore: NavItem[] = [{ kind: "link", href: "/recipes", label: "Khám phá công thức", icon: Compass }];
  if (isAuthor) explore.push({ kind: "link", href: "/dashboard", label: "Bảng điều khiển", icon: LayoutDashboard });
  groups.push({ label: "Khám phá", items: explore });

  if (isAdmin) {
    groups.push({
      label: "Quản trị",
      items: [
        { kind: "link", href: "/dashboard/categories", label: "Quản lý danh mục", icon: FolderTree },
        { kind: "link", href: "/admin/recipes/trash", label: "Thùng rác công thức", icon: Trash2 },
      ],
    });
  }
  return groups;
}

// Vùng chạm tối thiểu 44px; mục đang chọn nền cam kem nhạt + biểu tượng cam đất.
const ITEM_CLASS =
  "flex min-h-11 items-center gap-3 rounded-lg px-3 text-sm font-medium text-foreground/85 transition-colors hover:bg-accent hover:text-foreground";
const ITEM_ACTIVE_CLASS = "bg-(--cb-primary-tint) font-semibold text-foreground hover:bg-(--cb-primary-tint) [&_svg]:text-primary";

interface ProfileSidebarProps {
  user: UserDto;
  activeSection: ProfileSection;
  onSelectSection: (section: ProfileSection) => void;
}

/** Menu tài khoản có nhóm và biểu tượng. Desktop: cột cố định bên trái; mobile: thu gọn thành một nút. */
export function ProfileSidebar({ user, activeSection, onSelectSection }: ProfileSidebarProps) {
  const pathname = usePathname();
  const [open, setOpen] = useState(false);
  const groups = navGroupsFor(user);
  const activeLabel = groups[0].items.find((i) => i.kind === "section" && i.id === activeSection)?.label ?? "Hồ sơ cá nhân";

  return (
    <div className="lg:sticky lg:top-[calc(var(--header-height)+1.5rem)]">
      <button
        type="button"
        className="flex min-h-11 w-full items-center justify-between gap-3 rounded-lg border border-border bg-card px-4 text-sm font-semibold lg:hidden"
        aria-expanded={open}
        aria-controls="account-nav"
        onClick={() => setOpen((v) => !v)}
      >
        <span className="flex items-center gap-2">
          <Menu className="size-4 text-primary" aria-hidden="true" />
          Menu tài khoản
        </span>
        <span className="truncate font-normal text-muted-foreground">{activeLabel}</span>
      </button>

      <nav
        id="account-nav"
        aria-label="Menu tài khoản"
        className={cn("mt-2 lg:mt-0 lg:block", open ? "block" : "hidden")}
      >
        <div className="grid gap-5 rounded-lg border border-border bg-card p-2 lg:border-0 lg:bg-transparent lg:p-0">
          {groups.map((group) => (
            <div key={group.label} role="group" aria-labelledby={`nav-${group.label}`} className="grid gap-1">
              <p
                id={`nav-${group.label}`}
                className="px-3 pb-1 text-xs font-semibold tracking-wide text-muted-foreground uppercase"
              >
                {group.label}
              </p>
              <ul className="grid gap-0.5">
                {group.items.map((item) => {
                  const Icon = item.icon;
                  if (item.kind === "section") {
                    const active = item.id === activeSection;
                    return (
                      <li key={item.id}>
                        <a
                          href={`#${SECTION_ANCHORS[item.id]}`}
                          aria-current={active ? "page" : undefined}
                          className={cn(ITEM_CLASS, active && ITEM_ACTIVE_CLASS)}
                          onClick={(event) => {
                            event.preventDefault();
                            setOpen(false);
                            onSelectSection(item.id);
                          }}
                        >
                          <Icon className="size-[18px] shrink-0 text-muted-foreground" aria-hidden="true" />
                          {item.label}
                        </a>
                      </li>
                    );
                  }
                  const active = pathname === item.href;
                  return (
                    <li key={item.href}>
                      <Link
                        href={item.href}
                        aria-current={active ? "page" : undefined}
                        className={cn(ITEM_CLASS, active && ITEM_ACTIVE_CLASS)}
                        onClick={() => setOpen(false)}
                      >
                        <Icon className="size-[18px] shrink-0 text-muted-foreground" aria-hidden="true" />
                        {item.label}
                      </Link>
                    </li>
                  );
                })}
              </ul>
            </div>
          ))}
        </div>
      </nav>
    </div>
  );
}
