'use client';

import React from 'react';
import { AlertTriangle, Trash2 } from 'lucide-react';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import { cn } from '@/lib/utils';

export interface ConfirmDialogProps {
  /** Trạng thái mở/đóng modal. */
  isOpen: boolean;
  /** Tiêu đề hộp thoại xác nhận. */
  title: string;
  /** Nội dung thông điệp chi tiết. */
  message: string;
  /** Nhãn nút xác nhận. Mặc định là 'Xác nhận'. */
  confirmLabel?: string;
  /** Nhãn nút hủy. Mặc định là 'Hủy bỏ'. */
  cancelLabel?: string;
  /** Đánh dấu thao tác mang tính hủy hoại (xóa, thanh lọc) để đổi màu cảnh báo đỏ. */
  isDestructive?: boolean;
  /** Hàm callback khi xác nhận. */
  onConfirm: () => void;
  /** Hàm callback khi hủy hoặc đóng modal. */
  onCancel: () => void;
}

/**
 * ConfirmDialog chuẩn hóa theo shadcn/ui AlertDialog (Lab 6 - TV3).
 * Giữ nguyên 100% props và hàm gọi để không làm vỡ Dashboard và các trang quản trị.
 */
export function ConfirmDialog({
  isOpen,
  title,
  message,
  confirmLabel = 'Xác nhận',
  cancelLabel = 'Hủy bỏ',
  isDestructive = false,
  onConfirm,
  onCancel,
}: ConfirmDialogProps) {
  return (
    <AlertDialog open={isOpen} onOpenChange={(open) => !open && onCancel()}>
      <AlertDialogContent className="max-w-md">
        <AlertDialogHeader className="flex flex-col sm:flex-row items-start sm:items-center gap-4">
          <div
            className={cn(
              'rounded-full p-3 flex items-center justify-center shrink-0 shadow-sm',
              isDestructive
                ? 'bg-destructive/10 text-destructive border border-destructive/20'
                : 'bg-primary/10 text-primary border border-primary/20'
            )}
          >
            {isDestructive ? (
              <Trash2 className="w-5 h-5" aria-hidden="true" />
            ) : (
              <AlertTriangle className="w-5 h-5" aria-hidden="true" />
            )}
          </div>
          <div className="space-y-1 text-left">
            <AlertDialogTitle className="text-lg font-bold text-foreground">
              {title}
            </AlertDialogTitle>
            <AlertDialogDescription className="text-sm text-muted-foreground leading-relaxed">
              {message}
            </AlertDialogDescription>
          </div>
        </AlertDialogHeader>

        <AlertDialogFooter className="mt-4 gap-2 sm:gap-0">
          <AlertDialogCancel onClick={onCancel} className="cursor-pointer">
            {cancelLabel}
          </AlertDialogCancel>
          <AlertDialogAction
            onClick={onConfirm}
            variant={isDestructive ? 'destructive' : 'default'}
            className="cursor-pointer font-medium"
          >
            {confirmLabel}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}

export default ConfirmDialog;
