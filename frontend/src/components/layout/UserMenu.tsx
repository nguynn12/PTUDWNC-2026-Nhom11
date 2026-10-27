"use client";

import { ChevronDown, LayoutDashboard, LogIn, LogOut, NotebookPen, ShieldCheck, UserRound } from "lucide-react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { RoleBadge } from "@/components/auth/RoleBadge";
import { UserAvatar } from "@/components/auth/UserAvatar";
import { Button, buttonVariants } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { logout } from "@/lib/auth/session";
import { cn } from "@/lib/utils";
import { useAuthStatus, useCurrentUser, useHasRole } from "@/store/auth-store";
import { useAuthUIStore } from "@/store/auth-ui-store";

// Mục menu: nền cam nhạt khi rê chuột/focus thay cho màu accent mặc định.
const ITEM = "py-2 focus:bg-primary/10 focus:text-foreground not-data-[variant=destructive]:focus:**:text-foreground";

/** Menu tài khoản trên header (shadcn DropdownMenu + Avatar). Chưa đăng nhập thì hiện nút Đăng nhập. */
export function UserMenu() {
  const router = useRouter();
  const status = useAuthStatus();
  const user = useCurrentUser();
  const isAuthor = useHasRole("Author");
  const open = useAuthUIStore((s) => s.userMenuOpen);
  const setOpen = useAuthUIStore((s) => s.setUserMenuOpen);

  if (status === "loading") {
    return <span className="block size-9 animate-pulse rounded-full bg-muted" aria-hidden="true" />;
  }

  if (status !== "authenticated" || !user) {
    return (
      <Button asChild size="sm" className="h-9 rounded-full px-3 sm:px-4">
        <Link href="/auth/login" aria-label="Đăng nhập">
          <LogIn aria-hidden="true" />
          <span className="hidden sm:inline">Đăng nhập</span>
        </Link>
      </Button>
    );
  }

  const onLogout = async () => {
    await logout();
    router.replace("/auth/login");
  };

  return (
    <DropdownMenu open={open} onOpenChange={(next) => setOpen(next)}>
      <DropdownMenuTrigger
        aria-label="Mở menu tài khoản"
        className={cn(
          buttonVariants({ variant: "ghost" }),
          "h-10 gap-1 rounded-full px-1 hover:bg-muted sm:pr-2 hover:text-foreground data-[popup-open]:bg-muted",
        )}
      >
        <UserAvatar name={user.displayName} src={user.avatarUrl} size="sm" />
        <ChevronDown className="hidden size-4 text-muted-foreground sm:block" aria-hidden="true" />
      </DropdownMenuTrigger>

      <DropdownMenuContent align="end" sideOffset={8} className="w-64 p-1.5">
        <DropdownMenuGroup>
          <DropdownMenuLabel className="flex items-center gap-3 px-2 py-2 font-normal text-foreground">
            <UserAvatar name={user.displayName} src={user.avatarUrl} size="md" />
            <span className="min-w-0 flex-1">
              <span className="block truncate text-sm font-semibold">{user.displayName}</span>
              <span className="block truncate text-xs text-muted-foreground">{user.email}</span>
            </span>
          </DropdownMenuLabel>
          {user.roles.length > 0 && (
            <div className="flex flex-wrap gap-1 px-2 pb-2">
              {user.roles.map((role) => (
                <RoleBadge key={role} role={role} />
              ))}
            </div>
          )}
        </DropdownMenuGroup>
        <DropdownMenuSeparator />
        <DropdownMenuGroup>
          <DropdownMenuItem className={ITEM} render={<Link href="/profile" />}>
            <UserRound /> Hồ sơ cá nhân
          </DropdownMenuItem>
          {isAuthor && (
            <>
              <DropdownMenuItem className={ITEM} render={<Link href="/dashboard/recipes" />}>
                <NotebookPen /> Công thức của tôi
              </DropdownMenuItem>
              <DropdownMenuItem className={ITEM} render={<Link href="/dashboard" />}>
                <LayoutDashboard /> Bảng điều khiển
              </DropdownMenuItem>
            </>
          )}
          {user.roles.includes("Admin") && (
            <DropdownMenuItem className={ITEM} render={<Link href="/dashboard/categories" />}>
              <ShieldCheck /> Quản trị danh mục
            </DropdownMenuItem>
          )}
        </DropdownMenuGroup>
        <DropdownMenuSeparator />
        <DropdownMenuItem variant="destructive" className="py-2" onClick={onLogout}>
          <LogOut /> Đăng xuất
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
