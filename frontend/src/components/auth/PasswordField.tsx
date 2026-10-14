"use client";

import { useState, type InputHTMLAttributes } from "react";
import type { FieldError } from "react-hook-form";
import styles from "@/styles/auth.module.css";

interface PasswordFieldProps extends Omit<InputHTMLAttributes<HTMLInputElement>, "type"> {
  label: string;
  error?: FieldError;
  hint?: string;
}

/** Ô mật khẩu có nút Hiện/Ẩn. */
export function PasswordField({ label, error, hint, id, ...rest }: PasswordFieldProps) {
  const [visible, setVisible] = useState(false);
  const inputId = id ?? rest.name;
  const describedBy = error ? `${inputId}-error` : hint ? `${inputId}-hint` : undefined;

  return (
    <div className={styles.field}>
      <label className={styles.label} htmlFor={inputId}>
        {label}
      </label>
      <div className={styles.passwordWrap}>
        <input
          id={inputId}
          type={visible ? "text" : "password"}
          className={`${styles.input} ${styles.passwordInput} ${error ? styles.inputError : ""}`}
          aria-invalid={error ? true : undefined}
          aria-describedby={describedBy}
          {...rest}
        />
        <button
          type="button"
          className={styles.passwordToggle}
          onClick={() => setVisible((v) => !v)}
          aria-label={visible ? "Ẩn mật khẩu" : "Hiện mật khẩu"}
          aria-pressed={visible}
        >
          {visible ? "Ẩn" : "Hiện"}
        </button>
      </div>
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
