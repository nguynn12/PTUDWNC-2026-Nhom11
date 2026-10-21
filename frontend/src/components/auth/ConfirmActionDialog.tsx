"use client";

import type { ReactNode } from "react";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import { cn } from "@/lib/utils";
import { PROFILE_THEME_CLASS } from "./profile-theme";
import { dangerButtonClass, outlineButtonClass, primaryButtonClass, sectionTitleClass } from "./profile-styles";

interface ConfirmActionDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title: string;
  description: ReactNode;
  confirmLabel: string;
  cancelLabel?: string;
  /** "danger" chỉ dùng cho thao tác không hoàn tác được. */
  tone?: "default" | "danger";
  onConfirm: () => void;
}

/** Hộp thoại xác nhận (shadcn AlertDialog: giữ focus trong hộp thoại, Esc để đóng, đọc được bằng trình đọc màn hình). */
export function ConfirmActionDialog({
  open,
  onOpenChange,
  title,
  description,
  confirmLabel,
  cancelLabel = "Quay lại",
  tone = "default",
  onConfirm,
}: ConfirmActionDialogProps) {
  return (
    <AlertDialog open={open} onOpenChange={onOpenChange}>
      {/* Hộp thoại render ra ngoài khu vực .cb-account nên phải mang lại token và font. */}
      <AlertDialogContent className={cn(PROFILE_THEME_CLASS, "bg-card text-foreground sm:max-w-md")}>
        <AlertDialogHeader>
          <AlertDialogTitle className={sectionTitleClass}>{title}</AlertDialogTitle>
          <AlertDialogDescription className="text-[15px] leading-relaxed">{description}</AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel className={outlineButtonClass}>{cancelLabel}</AlertDialogCancel>
          <AlertDialogAction className={tone === "danger" ? dangerButtonClass : primaryButtonClass} onClick={onConfirm}>
            {confirmLabel}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
