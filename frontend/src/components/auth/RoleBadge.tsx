import type { UserRole } from "@/types/auth";
import styles from "@/styles/auth.module.css";

const LABEL: Record<UserRole, string> = { Admin: "Quản trị viên", Author: "Tác giả" };

/** Nhãn vai trò (SRS 2.3: Author, Admin). */
export function RoleBadge({ role }: { role: UserRole }) {
  return <span className={`${styles.badge} ${role === "Admin" ? styles.badgeAdmin : ""}`}>{LABEL[role]}</span>;
}
