"use client";

import { useMutation } from "@tanstack/react-query";
import { CircleCheck, KeyRound, LogOut, MailWarning, ShieldCheck, TriangleAlert } from "lucide-react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState, type ReactNode } from "react";
import { Button } from "@/components/ui/button";
import { logout } from "@/lib/auth/session";
import type { UserDto } from "@/types/auth";
import { ChangePasswordForm } from "./ChangePasswordForm";
import { ConfirmActionDialog } from "./ConfirmActionDialog";
import { outlineButtonClass } from "./profile-styles";
import { ResendConfirmationButton } from "./ResendConfirmationButton";

/** Trạng thái xác nhận email: luôn có biểu tượng + chữ, không chỉ dựa vào màu. */
export function EmailStatus({ confirmed }: { confirmed: boolean }) {
  return confirmed ? (
    <span className="inline-flex items-center gap-1.5 rounded-full bg-(--cb-accent)/12 px-2.5 py-1 text-[13px] font-semibold text-(color:--cb-accent-ink)">
      <CircleCheck className="size-4" aria-hidden="true" /> Đã xác nhận
    </span>
  ) : (
    <span className="inline-flex items-center gap-1.5 rounded-full bg-amber-100 px-2.5 py-1 text-[13px] font-semibold text-amber-900">
      <TriangleAlert className="size-4" aria-hidden="true" /> Chưa xác nhận
    </span>
  );
}

function SecurityBlock({
  id,
  icon,
  title,
  description,
  badge,
  action,
  children,
}: {
  id?: string;
  icon: ReactNode;
  title: string;
  description: ReactNode;
  /** Trạng thái hiển thị cạnh tiêu đề. */
  badge?: ReactNode;
  action?: ReactNode;
  children?: ReactNode;
}) {
  const headingId = `${id ?? title}-heading`;
  return (
    <section id={id} aria-labelledby={headingId} className="scroll-mt-24 py-7 first:pt-0 last:pb-0">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
        <div className="flex min-w-0 gap-3.5">
          <span className="flex size-10 shrink-0 items-center justify-center rounded-full bg-(--cb-primary-tint) text-primary">
            {icon}
          </span>
          <div className="min-w-0">
            <div className="flex flex-wrap items-center gap-x-3 gap-y-1.5">
              <h3 id={headingId} className="text-base leading-snug font-semibold">
                {title}
              </h3>
              {badge}
            </div>
            <p className="mt-1 text-sm leading-relaxed text-muted-foreground">{description}</p>
          </div>
        </div>
        {action && <div className="shrink-0 pl-[3.375rem] sm:pl-0">{action}</div>}
      </div>
      {children && <div className="mt-6 sm:pl-[3.375rem]">{children}</div>}
    </section>
  );
}

interface SecuritySectionProps {
  me: UserDto;
  /** Form hồ sơ đang có thay đổi chưa lưu: đăng xuất sẽ làm mất chúng nên cần xác nhận. */
  hasUnsavedChanges: boolean;
}

/** Bảo mật & phiên đăng nhập: xác nhận email, đổi mật khẩu, đăng xuất. */
export function SecuritySection({ me, hasUnsavedChanges }: SecuritySectionProps) {
  const router = useRouter();
  const [confirmLogout, setConfirmLogout] = useState(false);

  // Giữ nguyên quy trình đăng xuất hiện tại: gọi logout() rồi về trang chủ, kể cả khi API lỗi.
  const logoutMutation = useMutation({
    mutationFn: logout,
    onSettled: () => router.replace("/"),
  });

  const requestLogout = () => (hasUnsavedChanges ? setConfirmLogout(true) : logoutMutation.mutate());

  return (
    <div className="divide-y divide-border">
      <SecurityBlock
        icon={me.emailConfirmed ? <ShieldCheck className="size-5" aria-hidden="true" /> : <MailWarning className="size-5" aria-hidden="true" />}
        title="Xác nhận email"
        description={
          me.emailConfirmed
            ? "Email của bạn đã được xác nhận, bạn có thể xuất bản công thức."
            : "Xác nhận email để mở khoá chức năng xuất bản công thức. Chúng tôi đã gửi liên kết xác nhận tới hộp thư của bạn."
        }
        badge={<EmailStatus confirmed={me.emailConfirmed} />}
        action={me.emailConfirmed ? undefined : <ResendConfirmationButton email={me.email} />}
      />

      <SecurityBlock
        id="doi-mat-khau"
        icon={<KeyRound className="size-5" aria-hidden="true" />}
        title="Mật khẩu"
        description={
          <>
            Sau khi đổi, bạn vẫn đăng nhập trên trình duyệt này; các thiết bị khác sẽ bị đăng xuất. Tài khoản tạo bằng
            Google chưa có mật khẩu thì dùng{" "}
            <Link href="/auth/forgot-password" className="font-medium text-(color:--cb-primary-hover) underline underline-offset-2">
              Quên mật khẩu
            </Link>{" "}
            để tạo mật khẩu.
          </>
        }
      >
        <ChangePasswordForm />
      </SecurityBlock>

      <SecurityBlock
        icon={<LogOut className="size-5" aria-hidden="true" />}
        title="Phiên đăng nhập"
        description="Bạn đang đăng nhập trên trình duyệt này. Phiên tự gia hạn khi bạn còn sử dụng."
        action={
          <Button
            type="button"
            className={outlineButtonClass}
            disabled={logoutMutation.isPending}
            aria-busy={logoutMutation.isPending || undefined}
            onClick={requestLogout}
          >
            <LogOut aria-hidden="true" /> {logoutMutation.isPending ? "Đang đăng xuất..." : "Đăng xuất"}
          </Button>
        }
      />

      <ConfirmActionDialog
        open={confirmLogout}
        onOpenChange={setConfirmLogout}
        title="Đăng xuất và bỏ thay đổi?"
        description="Hồ sơ của bạn có thay đổi chưa lưu. Nếu đăng xuất bây giờ, những thay đổi này sẽ bị mất."
        confirmLabel="Đăng xuất"
        cancelLabel="Ở lại trang"
        onConfirm={() => logoutMutation.mutate()}
      />
    </div>
  );
}

