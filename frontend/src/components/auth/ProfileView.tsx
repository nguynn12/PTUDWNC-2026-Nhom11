"use client";

import { useMutation, useQuery } from "@tanstack/react-query";
import {
  BookOpen,
  CalendarDays,
  ChevronRight,
  Compass,
  FolderTree,
  LayoutDashboard,
  LogOut,
  Mail,
  NotebookPen,
  ShieldCheck,
  Trash2,
  type LucideIcon,
} from "lucide-react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import type { ReactNode } from "react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { authApi } from "@/lib/auth/api";
import { logout } from "@/lib/auth/session";
import type { AppError } from "@/lib/errors";
import { formatDate, formatNumber } from "@/lib/utils";
import { hasRole, useAuthStatus } from "@/store/auth-store";
import { useAuthUIStore } from "@/store/auth-ui-store";
import type { ResendConfirmationResponse, UserDto } from "@/types/auth";
import { AuthErrorState } from "./AuthErrorState";
import { AuthLoading } from "./AuthLoading";
import { ChangePasswordForm } from "./ChangePasswordForm";
import { FormAlert } from "./FormAlert";
import { ProfileForm } from "./ProfileForm";
import { RoleBadge } from "./RoleBadge";
import { UserAvatar } from "./UserAvatar";

interface Shortcut {
  href: string;
  label: string;
  description: string;
  icon: LucideIcon;
}

/** Lối tắt theo vai trò (SRS 2.3: Admin có mọi quyền của Author). */
function shortcutsFor(user: UserDto): Shortcut[] {
  const isAdmin = hasRole(user, "Admin");
  const isAuthor = isAdmin || hasRole(user, "Author");
  const items: Shortcut[] = [];
  if (isAuthor) {
    items.push(
      { href: "/dashboard/recipes/new", label: "Viết công thức mới", description: "Soạn và lưu nháp công thức", icon: NotebookPen },
      { href: "/dashboard/recipes", label: "Công thức của tôi", description: "Bản nháp, đã xuất bản, đã lưu trữ", icon: BookOpen },
      { href: "/dashboard", label: "Bảng điều khiển", description: "Tổng quan hoạt động của bạn", icon: LayoutDashboard },
    );
  }
  if (isAdmin) {
    items.push(
      { href: "/dashboard/categories", label: "Quản lý danh mục", description: "Thêm, sửa, sắp xếp danh mục", icon: FolderTree },
      { href: "/admin/recipes/trash", label: "Thùng rác công thức", description: "Khôi phục hoặc xoá vĩnh viễn", icon: Trash2 },
    );
  }
  items.push({ href: "/recipes", label: "Khám phá công thức", description: "Xem các món mới nhất", icon: Compass });
  return items;
}

const DAY_MS = 24 * 60 * 60 * 1000;

function memberDays(createdAt: string): number {
  const created = new Date(createdAt).getTime();
  return Number.isNaN(created) ? 0 : Math.max(1, Math.ceil((Date.now() - created) / DAY_MS));
}

function EmailStatus({ confirmed }: { confirmed: boolean }) {
  return (
    <Badge
      variant="outline"
      className={
        confirmed ? "border-secondary/30 bg-secondary/10 text-secondary" : "border-amber-300 bg-amber-50 text-amber-800"
      }
    >
      <span className="size-1.5 rounded-full bg-current" />
      {confirmed ? "Đã xác nhận" : "Chưa xác nhận"}
    </Badge>
  );
}

function InfoRow({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div className="flex items-center justify-between gap-3 py-3 first:pt-0 last:pb-0">
      <dt className="text-sm text-muted-foreground">{label}</dt>
      <dd className="flex flex-wrap justify-end gap-1.5 text-right text-sm font-semibold">{children}</dd>
    </div>
  );
}

/** FR-AUTH-006: trang hồ sơ cá nhân (thông tin tài khoản, lối tắt, chỉnh sửa hồ sơ, bảo mật). */
export function ProfileView() {
  const status = useAuthStatus();
  const router = useRouter();
  const showToast = useAuthUIStore((s) => s.showToast);

  const meQuery = useQuery<UserDto, AppError>({
    queryKey: ["me"],
    queryFn: authApi.me,
    enabled: status === "authenticated",
  });

  const resendMutation = useMutation<ResendConfirmationResponse, AppError, string>({
    mutationFn: (email) => authApi.resendConfirmation(email),
    onSuccess: (r) => showToast("success", r.message),
    onError: (error) => showToast("error", error.message),
  });

  const logoutMutation = useMutation({
    mutationFn: logout,
    onSettled: () => router.replace("/"),
  });

  const me = meQuery.data;

  if (status !== "authenticated" || meQuery.isLoading) {
    return (
      <section className="mx-auto w-full max-w-6xl px-4 py-16">
        <AuthLoading message="Đang tải hồ sơ..." />
      </section>
    );
  }

  if (meQuery.isError || !me) {
    return (
      <section className="mx-auto flex w-full max-w-6xl justify-center px-4 py-16">
        <Card className="w-full max-w-md">
          <CardContent>
            <AuthErrorState
              title="Không tải được hồ sơ"
              message={meQuery.error?.message}
              onRetry={() => void meQuery.refetch()}
            />
          </CardContent>
        </Card>
      </section>
    );
  }

  const resendButton = (
    <Button size="sm" disabled={resendMutation.isPending} onClick={() => resendMutation.mutate(me.email)}>
      <Mail /> {resendMutation.isPending ? "Đang gửi..." : "Gửi lại email xác nhận"}
    </Button>
  );

  return (
    <div className="mx-auto flex w-full max-w-6xl flex-col gap-6 px-4 py-8 sm:py-10">
      {/* Đầu trang: ảnh bìa, ảnh đại diện, tên, vai trò */}
      <Card className="gap-0 overflow-hidden py-0 shadow-sm">
        <div className="relative h-32 bg-gradient-to-r from-primary via-[#963e0b] to-secondary sm:h-40">
          <div className="absolute -top-10 right-10 size-40 rounded-full bg-white/10" />
          <div className="absolute -bottom-16 left-1/3 size-40 rounded-full bg-white/5" />
        </div>
        <div className="flex flex-col gap-4 px-6 pb-6 sm:flex-row sm:items-end sm:gap-6">
          <div className="relative z-10 -mt-12 w-fit rounded-full bg-card p-1.5 shadow-md">
            <UserAvatar name={me.displayName} src={me.avatarUrl} size="xl" />
          </div>
          <div className="min-w-0 flex-1 sm:pb-1">
            <h1 className="font-serif text-2xl font-bold tracking-tight sm:text-3xl">{me.displayName}</h1>
            <p className="truncate text-sm text-muted-foreground">{me.email}</p>
            <div className="mt-2 flex flex-wrap items-center gap-2 text-xs text-muted-foreground">
              {me.roles.map((role) => (
                <RoleBadge key={role} role={role} />
              ))}
              <span className="inline-flex items-center gap-1">
                <CalendarDays className="size-3.5" aria-hidden="true" /> Tham gia {formatDate(me.createdAt)}
              </span>
            </div>
          </div>
          <Button asChild variant="outline" className="hover:bg-muted hover:text-foreground sm:mb-1">
            <a href="#thong-tin-ca-nhan">Chỉnh sửa hồ sơ</a>
          </Button>
        </div>
        <Separator />
        <p
          className={
            me.bio
              ? "px-6 py-5 leading-relaxed whitespace-pre-line"
              : "px-6 py-5 text-sm text-muted-foreground italic"
          }
        >
          {me.bio || "Bạn chưa viết giới thiệu. Thêm vài dòng ở phần Thông tin cá nhân bên dưới nhé."}
        </p>
      </Card>

      {!me.emailConfirmed && (
        <FormAlert variant="warning" title="Email chưa được xác nhận" action={resendButton}>
          Bạn chưa xuất bản được công thức cho đến khi xác nhận email.
        </FormAlert>
      )}

      <div className="grid items-start gap-6 lg:grid-cols-[20rem_1fr]">
        <aside className="grid gap-6">
          <Card className="gap-4">
            <CardHeader>
              <CardTitle>Tài khoản</CardTitle>
            </CardHeader>
            <CardContent>
              <dl className="divide-y divide-dashed">
                <InfoRow label="Trạng thái email">
                  <EmailStatus confirmed={me.emailConfirmed} />
                </InfoRow>
                <InfoRow label="Vai trò">
                  {me.roles.length ? me.roles.map((role) => <RoleBadge key={role} role={role} />) : "Thành viên"}
                </InfoRow>
                <InfoRow label="Ngày tham gia">{formatDate(me.createdAt)}</InfoRow>
                <InfoRow label="Thành viên được">{formatNumber(memberDays(me.createdAt), 0)} ngày</InfoRow>
              </dl>
            </CardContent>
          </Card>

          <Card className="gap-4">
            <CardHeader>
              <CardTitle>Lối tắt</CardTitle>
            </CardHeader>
            <CardContent>
              <nav aria-label="Lối tắt">
                <ul className="grid gap-2">
                  {shortcutsFor(me).map(({ href, label, description, icon: Icon }) => (
                    <li key={href}>
                      <Link
                        href={href}
                        className="group flex items-center gap-3 rounded-lg border p-3 transition-colors hover:border-primary/40 hover:bg-primary/5"
                      >
                        <span className="flex size-9 shrink-0 items-center justify-center rounded-md bg-primary/10 text-primary">
                          <Icon className="size-4" aria-hidden="true" />
                        </span>
                        <span className="min-w-0 flex-1">
                          <span className="block text-sm font-semibold">{label}</span>
                          <span className="block truncate text-xs text-muted-foreground">{description}</span>
                        </span>
                        <ChevronRight
                          className="size-4 text-muted-foreground transition-transform group-hover:translate-x-0.5 group-hover:text-primary"
                          aria-hidden="true"
                        />
                      </Link>
                    </li>
                  ))}
                </ul>
              </nav>
            </CardContent>
          </Card>
        </aside>

        <div className="grid min-w-0 gap-6">
          <Card id="thong-tin-ca-nhan" className="scroll-mt-24">
            <CardHeader>
              <CardTitle className="text-lg">Thông tin cá nhân</CardTitle>
              <CardDescription>Tên, ảnh và giới thiệu được hiển thị trên các công thức bạn đăng.</CardDescription>
            </CardHeader>
            <CardContent>
              <ProfileForm me={me} />
            </CardContent>
          </Card>

          <Card id="doi-mat-khau" className="scroll-mt-24">
            <CardHeader>
              <CardTitle className="text-lg">Đổi mật khẩu</CardTitle>
              <CardDescription>
                Sau khi đổi, bạn vẫn đăng nhập trên trình duyệt này; các thiết bị khác sẽ bị đăng xuất. Tài khoản tạo
                bằng Google chưa có mật khẩu thì dùng{" "}
                <Link href="/auth/forgot-password" className="font-medium text-primary hover:underline">
                  Quên mật khẩu
                </Link>{" "}
                để tạo mật khẩu.
              </CardDescription>
            </CardHeader>
            <CardContent>
              <ChangePasswordForm />
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="text-lg">Bảo mật &amp; phiên đăng nhập</CardTitle>
              <CardDescription>Trạng thái xác nhận email và phiên đăng nhập trên trình duyệt này.</CardDescription>
            </CardHeader>
            <CardContent className="grid gap-4">
              <div className="flex flex-wrap items-center justify-between gap-3">
                <div className="flex min-w-0 flex-1 gap-3">
                  <ShieldCheck className="mt-0.5 size-5 shrink-0 text-secondary" aria-hidden="true" />
                  <div>
                    <p className="text-sm font-semibold">Xác nhận email</p>
                    <p className="text-sm text-muted-foreground">
                      {me.emailConfirmed
                        ? "Email đã được xác nhận, bạn có thể xuất bản công thức."
                        : "Xác nhận email để mở khoá chức năng xuất bản công thức."}
                    </p>
                  </div>
                </div>
                {me.emailConfirmed ? <EmailStatus confirmed /> : resendButton}
              </div>
              <Separator />
              <div className="flex flex-wrap items-center justify-between gap-3">
                <div className="flex min-w-0 flex-1 gap-3">
                  <LogOut className="mt-0.5 size-5 shrink-0 text-muted-foreground" aria-hidden="true" />
                  <div>
                    <p className="text-sm font-semibold">Phiên đăng nhập</p>
                    <p className="text-sm text-muted-foreground">
                      Bạn đang đăng nhập trên trình duyệt này. Phiên tự gia hạn khi bạn còn sử dụng.
                    </p>
                  </div>
                </div>
                <Button
                  variant="outline"
                  size="sm"
                  className="border-destructive/40 text-destructive hover:bg-destructive/10 hover:text-destructive"
                  disabled={logoutMutation.isPending}
                  onClick={() => logoutMutation.mutate()}
                >
                  <LogOut /> {logoutMutation.isPending ? "Đang đăng xuất..." : "Đăng xuất"}
                </Button>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
