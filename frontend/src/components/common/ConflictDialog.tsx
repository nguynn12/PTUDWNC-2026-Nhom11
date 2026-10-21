'use client';

import React from 'react';
import { AlertTriangle, RefreshCw } from 'lucide-react';
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
import { Badge } from '@/components/ui/badge';

export interface ConflictDialogProps {
  /** Trạng thái mở/đóng modal. */
  isOpen: boolean;
  /** Tên công thức đang gặp xung đột. */
  recipeTitle?: string;
  /** Thông điệp lỗi chi tiết trả về từ máy chủ. */
  errorMessage?: string;
  /** Giá trị Concurrency Token (PostgreSQL xmin) hiện hành. */
  currentXmin?: string | number | null;
  /** Hàm callback khi người dùng đồng ý nạp lại dữ liệu mới nhất. */
  onReload: () => void;
  /** Hàm callback đóng modal. */
  onClose: () => void;
}

/**
 * ConflictDialog xử lý lỗi xung đột dữ liệu đồng thời HTTP 409 qua Optimistic Concurrency Token xmin (Lab 6 - TV3).
 * Xây dựng bằng shadcn/ui AlertDialog với đầy đủ thông tin cảnh báo và chỉ dẫn bảo vệ dữ liệu tác giả.
 */
export function ConflictDialog({
  isOpen,
  recipeTitle,
  errorMessage,
  currentXmin,
  onReload,
  onClose,
}: ConflictDialogProps) {
  return (
    <AlertDialog open={isOpen} onOpenChange={(open) => !open && onClose()}>
      <AlertDialogContent className="max-w-lg">
        <AlertDialogHeader className="flex flex-col sm:flex-row items-start gap-4">
          <div className="rounded-full bg-amber-500/10 text-amber-600 border border-amber-500/20 p-3 shrink-0 shadow-sm">
            <AlertTriangle className="w-6 h-6" aria-hidden="true" />
          </div>

          <div className="space-y-1.5 text-left">
            <div className="flex items-center gap-2">
              <AlertDialogTitle className="text-lg font-bold text-destructive">
                Xung đột Dữ liệu Đồng thời (HTTP 409)
              </AlertDialogTitle>
              <Badge variant="outline" className="border-amber-400 text-amber-700 font-mono text-[10px]">
                RFC 7807
              </Badge>
            </div>
            <AlertDialogDescription className="text-sm text-muted-foreground">
              Phát hiện thay đổi ngoài ý muốn từ một phiên làm việc khác.
            </AlertDialogDescription>
          </div>
        </AlertDialogHeader>

        <div className="space-y-4 my-2 text-sm">
          <div className="rounded-xl bg-muted/60 p-4 border border-border/60 text-foreground leading-relaxed">
            Công thức <strong className="text-primary font-semibold">&ldquo;{recipeTitle || 'này'}&rdquo;</strong> đã được chỉnh sửa hoặc chuyển đổi trạng thái bởi một người dùng hoặc thiết bị khác trong khi bạn đang thao tác.
          </div>

          {errorMessage && (
            <div className="rounded-lg bg-destructive/10 p-3 border border-destructive/20 font-mono text-xs text-destructive break-all">
              {errorMessage}
            </div>
          )}

          {currentXmin && (
            <div className="flex items-center justify-between text-xs text-muted-foreground bg-background px-3 py-2 rounded-lg border border-border/40">
              <span>Mã phiên bản (xmin token) cục bộ:</span>
              <span className="font-mono font-bold text-foreground bg-muted px-2 py-0.5 rounded">
                &ldquo;{currentXmin}&rdquo;
              </span>
            </div>
          )}
        </div>

        <AlertDialogFooter className="mt-4 gap-2 sm:gap-0">
          <AlertDialogCancel onClick={onClose} className="cursor-pointer">
            Để tôi xem lại
          </AlertDialogCancel>
          <AlertDialogAction
            onClick={onReload}
            className="gap-2 bg-primary text-primary-foreground font-medium shadow-sm cursor-pointer"
          >
            <RefreshCw className="w-4 h-4" aria-hidden="true" />
            Nạp lại dữ liệu mới nhất
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}

export default ConflictDialog;
