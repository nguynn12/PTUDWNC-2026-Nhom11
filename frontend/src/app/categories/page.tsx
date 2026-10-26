import React from "react";
import type { Metadata } from "next";
import { getCategories } from "@/lib/api/categories";
import { CategoryCard } from "@/components/category/CategoryCard";
import { Container } from "@/components/common/Container";
import { PageHeader } from "@/components/common/PageHeader";
import { ErrorState } from "@/components/common/ErrorState";
import { EmptyState } from "@/components/common/EmptyState";
import type { CategoryDto } from "@/types/api";

// Cấu hình ISR: Tự động tái sinh ngầm định trang sau 60 giây khi có danh mục mới
export const revalidate = 60;

export const metadata: Metadata = {
  title: "Danh Mục Món Ăn Phong Phú",
  description:
    "Khám phá danh sách các chủ đề ẩm thực đa dạng tại Culinary Blog: từ món khai vị, món chính, canh súp đến tráng miệng và ẩm thực thực dưỡng.",
  keywords: [
    "danh mục món ăn",
    "chủ đề ẩm thực",
    "món khai vị",
    "món chính",
    "món tráng miệng",
    "món chay",
  ],
  alternates: {
    canonical: "/categories",
  },
  openGraph: {
    title: "Danh Mục Món Ăn Phong Phú | Culinary Blog",
    description: "Khám phá các danh mục món ăn phong phú và công thức nấu nướng tuyển chọn từ các chuyên gia ẩm thực.",
    url: "/categories",
  },
};

export default async function CategoriesPage() {
  let categories: CategoryDto[] = [];
  let fetchError = false;

  try {
    categories = await getCategories();
  } catch (err) {
    console.error("Lỗi khi tải danh sách danh mục từ backend:", err);
    fetchError = true;
  }

  // Sắp xếp danh mục tăng dần theo orderIndex và thứ tự chữ cái tiếng Việt
  const sortedCategories = [...categories].sort((a, b) => {
    const orderA = a.orderIndex ?? 999;
    const orderB = b.orderIndex ?? 999;
    if (orderA !== orderB) return orderA - orderB;
    return a.name.localeCompare(b.name, "vi");
  });

  return (
    <div className="py-8 sm:py-12">
      <Container>
        {/* Header trang có Breadcrumbs chuẩn SEO */}
        <PageHeader
          breadcrumbs={[
            { label: "Trang chủ", href: "/" },
            { label: "Danh mục" },
          ]}
          badge={
            sortedCategories.length > 0
              ? `Tổng hợp ${sortedCategories.length} chủ đề ẩm thực`
              : "Chủ đề ẩm thực phong phú"
          }
          title="Tất Cả Danh Mục Món Ăn"
          description="Lựa chọn danh mục yêu thích để tìm kiếm những công thức nấu ăn phù hợp nhất với khẩu vị và nhu cầu dinh dưỡng của gia đình bạn."
        />

        {/* Xử lý các trạng thái: Lỗi mạng, Trạng thái rỗng, hoặc Lưới danh mục thật */}
        {fetchError && sortedCategories.length === 0 ? (
          <div className="mt-8">
            <ErrorState
              title="Không thể tải danh sách danh mục"
              message="Đã có sự cố khi kết nối tới máy chủ. Vui lòng kiểm tra lại dịch vụ Backend hoặc thử lại sau ít phút."
              showBackHome={true}
            />
          </div>
        ) : sortedCategories.length === 0 ? (
          <div className="mt-8">
            <EmptyState
              title="Chưa có danh mục món ăn nào"
              description="Hệ thống đang cập nhật thêm các chủ đề ẩm thực mới. Bạn vui lòng quay lại sau nhé!"
              actionText="Về trang chủ"
              actionHref="/"
            />
          </div>
        ) : (
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-4 xl:grid-cols-5 gap-4 sm:gap-6 mt-8">
            {sortedCategories.map((category) => (
              <CategoryCard key={category.id || category.slug} category={category} />
            ))}
          </div>
        )}
      </Container>
    </div>
  );
}
