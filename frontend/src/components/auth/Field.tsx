import type { InputHTMLAttributes } from "react";
import type { FieldError } from "react-hook-form";
import styles from "@/styles/auth.module.css";

interface FieldProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  error?: FieldError;
  /** Gợi ý hiển thị dưới ô khi chưa có lỗi. */
  hint?: string;
}

/** Ô nhập có nhãn, gợi ý và lỗi; React 19 truyền ref như prop thường nên dùng trực tiếp với register(). */
export function Field({ label, error, hint, id, ...rest }: FieldProps) {
  const inputId = id ?? rest.name;
  const describedBy = error ? `${inputId}-error` : hint ? `${inputId}-hint` : undefined;
  return (
    <div className={styles.field}>
      <label className={styles.label} htmlFor={inputId}>
        {label}
      </label>
      <input
        id={inputId}
        className={`${styles.input} ${error ? styles.inputError : ""}`}
        aria-invalid={error ? true : undefined}
        aria-describedby={describedBy}
        {...rest}
      />
      {error ? (
        <span id={`${inputId}-error`} className={styles.error}>
          {error.message}
        </span>
      ) : (
        hint && (
          <span id={`${inputId}-hint`} className={styles.fieldHint}>
            {hint}
          </span>
        )
      )}
    </div>
  );
}
