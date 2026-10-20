"use client";

import { Eye, EyeOff } from "lucide-react";
import { useState, type ComponentProps, type ReactNode } from "react";
import type { FieldError } from "react-hook-form";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { describedBy, FieldMessage } from "./Field";

interface PasswordFieldProps extends Omit<ComponentProps<"input">, "type"> {
  label: string;
  error?: FieldError;
  hint?: string;
  /** Liên kết đặt cùng hàng với nhãn (vd. "Quên mật khẩu?"). */
  labelAction?: ReactNode;
}

/** Ô mật khẩu có nút hiện/ẩn. */
export function PasswordField({ label, error, hint, labelAction, id, ...rest }: PasswordFieldProps) {
  const [visible, setVisible] = useState(false);
  const inputId = id ?? rest.name ?? label;

  return (
    <div className="grid content-start gap-2">
      <div className="flex items-center justify-between gap-2">
        <Label htmlFor={inputId}>{label}</Label>
        {labelAction}
      </div>
      <div className="relative">
        <Input
          id={inputId}
          type={visible ? "text" : "password"}
          className="h-10 pr-11"
          aria-invalid={error ? true : undefined}
          aria-describedby={describedBy(inputId, error, hint)}
          {...rest}
        />
        <Button
          type="button"
          variant="ghost"
          size="icon-sm"
          className="absolute top-1/2 right-1.5 -translate-y-1/2 text-muted-foreground hover:bg-muted hover:text-foreground"
          onClick={() => setVisible((v) => !v)}
          aria-label={visible ? "Ẩn mật khẩu" : "Hiện mật khẩu"}
          aria-pressed={visible}
        >
          {visible ? <EyeOff /> : <Eye />}
        </Button>
      </div>
      <FieldMessage id={inputId} error={error} hint={hint} />
    </div>
  );
}
