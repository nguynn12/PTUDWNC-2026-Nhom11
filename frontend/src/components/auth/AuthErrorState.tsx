import styles from "@/styles/auth.module.css";

interface AuthErrorStateProps {
  title?: string;
  message?: string;
  onRetry?: () => void;
  retryLabel?: string;
}

/** Trạng thái lỗi dùng chung (không tải được dữ liệu, error.tsx của route). */
export function AuthErrorState({
  title = "Đã có lỗi xảy ra",
  message,
  onRetry,
  retryLabel = "Thử lại",
}: AuthErrorStateProps) {
  return (
    <div className={styles.stateBox} role="alert">
      <p className={styles.stateTitle}>{title}</p>
      {message && <p className={styles.stateText}>{message}</p>}
      {onRetry && (
        <button type="button" className={`${styles.button} ${styles.buttonSmall}`} onClick={onRetry}>
          {retryLabel}
        </button>
      )}
    </div>
  );
}
