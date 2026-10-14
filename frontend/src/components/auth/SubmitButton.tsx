import type { ButtonHTMLAttributes } from "react";
import styles from "@/styles/auth.module.css";
import { Spinner } from "./AuthLoading";

interface SubmitButtonProps extends Omit<ButtonHTMLAttributes<HTMLButtonElement>, "type"> {
  loading?: boolean;
  /** Chữ hiển thị khi đang gửi (mặc định giữ nguyên children). */
  loadingText?: string;
}

/** Nút gửi form có trạng thái đang xử lý (khoá nút, hiện vòng quay). */
export function SubmitButton({ loading = false, loadingText, disabled, children, className, ...rest }: SubmitButtonProps) {
  return (
    <button
      type="submit"
      className={`${styles.button} ${className ?? ""}`}
      disabled={disabled || loading}
      aria-busy={loading || undefined}
      {...rest}
    >
      {loading && <Spinner size="sm" />}
      <span>{loading && loadingText ? loadingText : children}</span>
    </button>
  );
}
