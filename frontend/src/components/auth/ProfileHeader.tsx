import { CalendarDays, Pencil } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { formatDate } from "@/lib/utils";
import type { UserDto } from "@/types/auth";
import { outlineButtonClass } from "./profile-styles";
import { RoleBadge } from "./RoleBadge";
import { UserAvatar } from "./UserAvatar";

// RoleBadge dùng màu nhấn của site; trên nền sáng của khu vực tài khoản cần chữ đậm hơn để đạt 4.5:1.
const ROLE_TONE = {
  Author: "text-(color:--cb-primary-hover)",
  Admin: "text-(color:--cb-accent-ink)",
} as const;

/** Đầu trang hồ sơ: ảnh đại diện, tên, email, vai trò, ngày tham gia và giới thiệu (hiển thị một lần). */
export function ProfileHeader({ me, onEdit }: { me: UserDto; onEdit: () => void }) {
  const displayName = me.displayName.normalize("NFC");

  return (
    <header className="flex flex-col gap-5 sm:flex-row sm:items-center sm:gap-6">
      <UserAvatar name={displayName} src={me.avatarUrl} size="xl" className="size-20 shrink-0 ring-1 ring-border sm:size-24" />

      <div className="min-w-0 flex-1">
        <h1 className="font-serif text-[1.75rem] leading-tight font-semibold tracking-tight break-words sm:text-3xl">
          {displayName}
        </h1>
        <p className="mt-0.5 truncate text-sm text-muted-foreground">{me.email}</p>

        <div className="mt-2.5 flex flex-wrap items-center gap-x-3 gap-y-2 text-[13px] text-muted-foreground">
          {me.roles.length > 0 ? (
            me.roles.map((role) => <RoleBadge key={role} role={role} className={ROLE_TONE[role]} />)
          ) : (
            <Badge variant="outline" className="font-semibold text-muted-foreground">
              Thành viên
            </Badge>
          )}
          <span className="inline-flex items-center gap-1.5">
            <CalendarDays className="size-3.5" aria-hidden="true" /> Tham gia {formatDate(me.createdAt)}
          </span>
        </div>

        {me.bio && (
          <p className="mt-3 line-clamp-3 max-w-prose text-[15px] leading-relaxed whitespace-pre-line text-foreground/90">
            {me.bio}
          </p>
        )}
      </div>

      <Button type="button" className={`${outlineButtonClass} self-start sm:self-center`} onClick={onEdit}>
        <Pencil aria-hidden="true" /> Chỉnh sửa hồ sơ
      </Button>
    </header>
  );
}
