import React from "react";
import { Button } from "@/components/ui/button";

interface ErrorStateProps {
  title?: string;
  message?: string;
  onRetry?: () => void;
  className?: string;
}

export function ErrorState({
  title = "Đã xảy ra sự cố",
  message = "Không thể tải dữ liệu vào lúc này. Vui lòng thử lại sau.",
  onRetry,
  className = "",
}: ErrorStateProps) {
  return (
    <div
      className={`flex flex-col items-center justify-center p-8 md:p-14 text-center rounded-2xl border border-[var(--danger-light)] bg-[var(--danger-light)]/30 ${className}`}
    >
      <div className="text-4xl md:text-5xl mb-4 select-none">
        ⚠️
      </div>
      <h3 className="text-lg md:text-xl font-bold text-[var(--danger)] mb-2">
        {title}
      </h3>
      <p className="text-sm md:text-base text-[var(--text-secondary)] max-w-md mb-6 leading-relaxed">
        {message}
      </p>
      {onRetry && (
        <Button variant="default" onClick={onRetry}>
          Thử lại
        </Button>
      )}
    </div>
  );
}
