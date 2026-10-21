import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";
import type { UserRole } from "@/types/auth";

const ROLES: Record<UserRole, { label: string; className: string }> = {
  Admin: { label: "Quản trị viên", className: "bg-secondary/15 text-secondary" },
  Author: { label: "Tác giả", className: "bg-primary/10 text-primary" },
};

/** Nhãn vai trò (SRS 2.3: Author, Admin) bằng shadcn Badge. */
export function RoleBadge({ role, className }: { role: UserRole; className?: string }) {
  const { label, className: tone } = ROLES[role];
  return <Badge className={cn("font-semibold", tone, className)}>{label}</Badge>;
}
