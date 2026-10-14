import type { ReactNode } from "react";
import styles from "@/styles/auth.module.css";

interface AuthCardProps {
  title?: ReactNode;
  subtitle?: ReactNode;
  footer?: ReactNode;
  /** Thẻ rộng hơn (trang hồ sơ). */
  wide?: boolean;
  children: ReactNode;
}

/** Khung thẻ dùng chung cho các màn hình đăng nhập, đăng ký, xác nhận email, hồ sơ. */
export function AuthCard({ title, subtitle, footer, wide, children }: AuthCardProps) {
  return (
    <div className={`${styles.card} ${wide ? styles.wide : ""}`}>
      {(title || subtitle) && (
        <header className={styles.cardHeader}>
          {title && <h1 className={styles.title}>{title}</h1>}
          {subtitle && <p className={styles.subtitle}>{subtitle}</p>}
        </header>
      )}
      {children}
      {footer && <p className={styles.footer}>{footer}</p>}
    </div>
  );
}
