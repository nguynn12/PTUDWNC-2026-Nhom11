'use client';

import React, { useEffect } from 'react';
import { ErrorState } from '@/components/common/ErrorState';

export interface RouteErrorProps {
  error: Error & { digest?: string };
  reset: () => void;
}

/**
 * Error boundary cấp route ứng dụng (Lab 6 - TV3).
 * Bắt tất cả runtime errors trong App Router và hiển thị giao diện phục hồi thân thiện.
 */
export default function AppError({ error, reset }: RouteErrorProps) {
  useEffect(() => {
    // Ghi log lỗi runtime để kiểm toán và gỡ lỗi
    console.error('Unhandled Route Error Boundary:', error);
  }, [error]);

  return (
    <div className="container mx-auto px-4 py-16 flex items-center justify-center min-h-[70vh]">
      <ErrorState
        title="Đã xảy ra sự cố không mong muốn"
        message={error.message || 'Hệ thống ẩm thực tạm thời không thể xử lý yêu cầu của bạn. Vui lòng thử lại sau.'}
        errorCode={error.digest ? `DIGEST_${error.digest.slice(0, 8).toUpperCase()}` : 'ROUTE_ERROR'}
        onRetry={reset}
        retryLabel="Thử tải lại trang"
        showBackHome={true}
      />
    </div>
  );
}
