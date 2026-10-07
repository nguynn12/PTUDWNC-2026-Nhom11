"use client";

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import { useEffect, useState, type ReactNode } from "react";
import { AuthToastHost } from "@/components/auth/AuthToastHost";
import { startSession } from "@/lib/auth/session";

interface ProvidersProps {
  children: ReactNode;
}

export function Providers({ children }: ProvidersProps) {
  const [queryClient] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            staleTime: 60 * 1000, // 60 giây (1 phút) theo đặc tả giáo trình & SRS
            gcTime: 5 * 60 * 1000, // 5 phút garbage collection cache
            retry: 1,
            refetchOnWindowFocus: false,
          },
        },
      })
  );

  // Module Auth (FR-AUTH-004): khôi phục phiên từ refresh token, tự làm mới token, đồng bộ đăng xuất giữa các tab.
  useEffect(() => startSession(), []);

  return (
    <QueryClientProvider client={queryClient}>
      {children}
      <AuthToastHost />
      <ReactQueryDevtools initialIsOpen={false} buttonPosition="bottom-right" />
    </QueryClientProvider>
  );
}
