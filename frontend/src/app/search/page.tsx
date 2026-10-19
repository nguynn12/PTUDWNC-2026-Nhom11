import React, { Suspense } from "react";
import type { Metadata } from "next";
import { SearchClient } from "@/components/search/SearchClient";
import { Container } from "@/components/common/Container";
import { PageHeader } from "@/components/common/PageHeader";
import { Skeleton } from "@/components/ui/skeleton";

export const metadata: Metadata = {
  title: "Tìm Kiếm Công Thức Nấu Ăn",
  description:
    "Tìm kiếm công thức nấu ăn thông minh với PostgreSQL Full-Text Search. Tra cứu theo tên món ăn, nguyên liệu với khả năng tìm kiếm tiếng Việt có dấu và không dấu tại Culinary Blog.",
  openGraph: {
    title: "Tìm Kiếm Công Thức Nấu Ăn | Culinary Blog",
    description: "Công cụ tìm kiếm công thức nấu ăn thông minh và nhanh chóng.",
  },
};

function SearchLoadingFallback() {
  return (
    <div className="max-w-3xl mx-auto space-y-6 pt-4">
      <Skeleton className="h-12 w-full rounded-2xl" />
      <div className="flex gap-2 justify-center">
        {Array.from({ length: 6 }).map((_, i) => (
          <Skeleton key={i} className="h-7 w-20 rounded-full" />
        ))}
      </div>
    </div>
  );
}

export default function SearchPage() {
  return (
    <div className="py-8 sm:py-12">
      <Container>
        {/* Header trang */}
        <PageHeader
          breadcrumbs={[
            { label: "Trang chủ", href: "/" },
            { label: "Tìm kiếm" },
          ]}
          badge="Tra cứu thông minh"
          title="Tìm Kiếm Công Thức Ẩm Thực"
          description="Nhập từ khóa tiếng Việt (có dấu hoặc không dấu) để tìm kiếm các món ngon chuẩn vị cùng hướng dẫn chi tiết."
          align="center"
        />

        {/* Nội dung tìm kiếm Client bọc Suspense */}
        <div className="mt-8">
          <Suspense fallback={<SearchLoadingFallback />}>
            <SearchClient />
          </Suspense>
        </div>
      </Container>
    </div>
  );
}
