import styles from "@/styles/auth.module.css";

/** Vòng quay nhỏ; size="sm" dùng trong nút. */
export function Spinner({ size = "md", label }: { size?: "sm" | "md"; label?: string }) {
  return (
    <span
      className={`${styles.spinner} ${size === "sm" ? styles.spinnerSm : ""}`}
      role={label ? "status" : undefined}
      aria-label={label}
      aria-hidden={label ? undefined : true}
    />
  );
}

/** Trạng thái đang tải dùng chung (kiểm tra phiên, tải hồ sơ, loading.tsx). */
export function AuthLoading({ message = "Đang tải..." }: { message?: string }) {
  return (
    <div className={styles.stateBox} role="status" aria-live="polite">
      <Spinner />
      <p className={styles.stateText}>{message}</p>
    </div>
  );
}
