import React from "react";
import Link from "next/link";
import { Button } from "@/components/ui/button";

interface EmptyStateProps {
  title?: string;
  description?: string;
  icon?: React.ReactNode;
  actionText?: string;
  actionHref?: string;
  onAction?: () => void;
  className?: string;
}

export function EmptyState({
  title = "Không tìm thấy dữ liệu",
  description = "Hiện tại chưa có mục nào phù hợp với yêu cầu của bạn.",
  icon = "🍽️",
  actionText,
  actionHref,
  onAction,
  className = "",
}: EmptyStateProps) {
  return (
    <div
      className={`flex flex-col items-center justify-center p-8 md:p-14 text-center rounded-2xl border border-dashed border-[var(--border-subtle)] bg-[var(--bg-surface)] ${className}`}
    >
      <div className="text-4xl md:text-5xl mb-4 select-none animate-bounce-subtle">
        {icon}
      </div>
      <h3 className="text-lg md:text-xl font-bold text-[var(--text-primary)] mb-2">
        {title}
      </h3>
      <p className="text-sm md:text-base text-[var(--text-secondary)] max-w-md mb-6 leading-relaxed">
        {description}
      </p>
      {actionText && (
        actionHref ? (
          <Link href={actionHref}>
            <Button variant="default">{actionText}</Button>
          </Link>
        ) : onAction ? (
          <Button variant="default" onClick={onAction}>
            {actionText}
          </Button>
        ) : null
      )}
    </div>
  );
}
