'use client';

import React, { useEffect } from 'react';
import { AlertCircle, RotateCcw } from 'lucide-react';

export interface GlobalErrorProps {
  error: Error & { digest?: string };
  reset: () => void;
}

/**
 * Root Global Error Boundary (Lab 6 - TV3).
 * Bắt lỗi nghiêm trọng ở cấp root layout và ngăn ngừa White Screen of Death hoàn toàn.
 * Theo đặc tả của Next.js, global-error.tsx phải tự khai báo thẻ <html> và <body>.
 */
export default function GlobalError({ error, reset }: GlobalErrorProps) {
  useEffect(() => {
    console.error('CRITICAL Root Layout Error:', error);
  }, [error]);

  return (
    <html lang="vi">
      <body className="min-h-screen bg-[#fcfaf6] text-[#231f1d] flex items-center justify-center p-4 font-sans antialiased">
        <main
          role="alert"
          aria-live="assertive"
          className="max-w-lg w-full bg-white rounded-3xl p-8 md:p-10 shadow-2xl border border-red-200 text-center flex flex-col items-center gap-6"
        >
          <div className="rounded-full bg-red-100 p-5 text-red-600 shadow-inner">
            <AlertCircle className="w-12 h-12" aria-hidden="true" />
          </div>

          <div className="space-y-3">
            <h1 className="text-2xl font-bold tracking-tight text-neutral-900">
              Sự cố hệ thống nghiêm trọng
            </h1>
            <p className="text-sm text-neutral-600 leading-relaxed">
              Trang web gặp sự cố không thể tự phục hồi. Chúng tôi đã ghi nhận lỗi này và đang tiến hành khắc phục.
            </p>
            {error.digest && (
              <span className="inline-block px-3 py-1 bg-red-50 text-red-700 text-xs font-mono rounded-full border border-red-200">
                Mã sự cố: {error.digest.slice(0, 10).toUpperCase()}
              </span>
            )}
          </div>

          <div className="flex flex-wrap items-center justify-center gap-3 pt-2">
            <button
              type="button"
              onClick={() => reset()}
              className="inline-flex items-center gap-2 px-6 py-2.5 rounded-xl bg-[#c85a17] text-white font-medium hover:bg-[#b04d13] transition-colors shadow-sm cursor-pointer"
            >
              <RotateCcw className="w-4 h-4" aria-hidden="true" />
              Tải lại trang web
            </button>
            <a
              href="/"
              className="inline-flex items-center gap-2 px-6 py-2.5 rounded-xl bg-neutral-100 text-neutral-700 font-medium hover:bg-neutral-200 transition-colors border border-neutral-300"
            >
              Về trang chủ
            </a>
          </div>
        </main>
      </body>
    </html>
  );
}
