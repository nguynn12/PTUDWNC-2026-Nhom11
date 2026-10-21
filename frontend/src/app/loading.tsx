import React from 'react';
import { PageLoader } from '@/components/common/PageLoader';

/**
 * Loading boundary cấp route toàn ứng dụng Next.js (Lab 6 - TV3).
 * Tự động kích hoạt thông qua React Suspense khi người dùng chuyển trang.
 */
export default function AppLoading() {
  return (
    <div className="container mx-auto px-4 py-16 flex items-center justify-center min-h-[70vh]">
      <PageLoader text="Đang tải dữ liệu ẩm thực..." />
    </div>
  );
}
