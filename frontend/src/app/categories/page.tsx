import React from "react";
import type { Metadata } from "next";
import { getCategories } from "@/lib/api/categories";
import { CategoryCard } from "@/components/category/CategoryCard";
import { Container } from "@/components/common/Container";
import { PageHeader } from "@/components/common/PageHeader";
import type { CategoryDto } from "@/types/api";

export const revalidate = 3600; // ISR 1 giờ

export const metadata: Metadata = {
  title: "Danh Mục Món Ăn",
  description:
    "Khám phá danh sách các danh mục món ăn phong phú tại Culinary Blog: từ món khai vị, món chính, canh súp đến tráng miệng và ẩm thực thực dưỡng.",
  openGraph: {
    title: "Danh Mục Món Ăn | Culinary Blog",
    description: "Khám phá các danh mục món ăn phong phú và công thức nấu nướng tuyển chọn.",
  },
};

const fallbackCategories: CategoryDto[] = [
  { id: "1", name: "Món Khai Vị", slug: "mon-khai-vi", description: "Salad, súp và gỏi cuốn thanh mát", orderIndex: 1, recipeCount: 12 },
  { id: "2", name: "Món Chính", slug: "mon-chinh", description: "Món kho, xào, nướng đậm đà", orderIndex: 2, recipeCount: 36 },
  { id: "3", name: "Món Canh & Súp", slug: "mon-canh-sup", description: "Nước dùng ngọt lành, thanh nhiệt", orderIndex: 3, recipeCount: 18 },
  { id: "4", name: "Món Tráng Miệng", slug: "mon-trang-mieng", description: "Bánh ngọt và chè thơm ngon", orderIndex: 4, recipeCount: 24 },
  { id: "5", name: "Đồ Uống & Trà", slug: "do-uong-tra", description: "Nước ép tươi ngon và sinh tố", orderIndex: 5, recipeCount: 15 },
  { id: "6", name: "Ăn Chay Thực Dưỡng", slug: "an-chay-thuc-duong", description: "Món chay thanh tịnh từ rau củ", orderIndex: 6, recipeCount: 20 },
  { id: "7", name: "Món Bánh Truyền Thống", slug: "mon-banh-truyen-thong", description: "Hương vị bánh dân gian Việt Nam", orderIndex: 7, recipeCount: 14 },
  { id: "8", name: "Món Nhanh & Tiện Lợi", slug: "mon-nhanh-tien-loi", description: "Dưới 20 phút cho người bận rộn", orderIndex: 8, recipeCount: 22 },
];

export default async function CategoriesPage() {
  let categories: CategoryDto[] = [];

  try {
    const data = await getCategories();
    categories = data && data.length > 0 ? data : fallbackCategories;
  } catch {
    categories = fallbackCategories;
  }

  return (
    <div className="py-8 sm:py-12">
      <Container>
        {/* Header trang */}
        <PageHeader
          breadcrumbs={[
            { label: "Trang chủ", href: "/" },
            { label: "Danh mục" },
          ]}
          badge="Chủ đề ẩm thực phong phú"
          title="Tất Cả Danh Mục Món Ăn"
          description="Lựa chọn danh mục yêu thích để tìm kiếm những công thức nấu ăn phù hợp nhất với khẩu vị và nhu cầu của gia đình bạn."
        />

        {/* Lưới danh mục */}
        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-4 sm:gap-6 mt-8">
          {categories.map((category) => (
            <CategoryCard key={category.id || category.slug} category={category} />
          ))}
        </div>
      </Container>
    </div>
  );
}
