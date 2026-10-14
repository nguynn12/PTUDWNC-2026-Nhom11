import type { ReactNode } from "react";
import styles from "@/styles/auth.module.css";

export type FormAlertVariant = "error" | "success" | "warning" | "info";

const VARIANT_CLASS: Record<FormAlertVariant, string> = {
  error: styles.alertError,
  success: styles.alertSuccess,
  warning: styles.alertWarning,
  info: styles.alertInfo,
};

interface FormAlertProps {
  variant?: FormAlertVariant;
  /** Nút hoặc liên kết đặt bên phải thông báo (vd. "Gửi lại email"). */
  action?: ReactNode;
  children: ReactNode;
}

/** Thông báo trong form: lỗi dùng role="alert" để trình đọc màn hình đọc ngay. */
export function FormAlert({ variant = "error", action, children }: FormAlertProps) {
  return (
    <div className={`${styles.alert} ${VARIANT_CLASS[variant]}`} role={variant === "error" ? "alert" : "status"}>
      <span className={styles.alertBody}>{children}</span>
      {action}
    </div>
  );
}
