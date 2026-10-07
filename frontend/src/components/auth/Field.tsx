import type { InputHTMLAttributes } from "react";
import type { FieldError } from "react-hook-form";
import styles from "@/styles/auth.module.css";

interface FieldProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  error?: FieldError;
}

export function Field({ label, error, id, ...rest }: FieldProps) {
  const inputId = id ?? rest.name;
  return (
    <div className={styles.field}>
      <label className={styles.label} htmlFor={inputId}>
        {label}
      </label>
      <input
        id={inputId}
        className={`${styles.input} ${error ? styles.inputError : ""}`}
        aria-invalid={error ? true : undefined}
        aria-describedby={error ? `${inputId}-error` : undefined}
        {...rest}
      />
      {error && (
        <span id={`${inputId}-error`} className={styles.error}>
          {error.message}
        </span>
      )}
    </div>
  );
}
