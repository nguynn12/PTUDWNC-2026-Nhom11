import React from "react";
import Link from "next/link";
import Image from "next/image";
import { getCategories } from "@/lib/api/categories";
import { getRecipes } from "@/lib/api/recipes";
import { CategoryCard } from "@/components/category/CategoryCard";
import { RecipeCard } from "@/components/recipe/RecipeCard";
import type { CategoryDto, RecipeSummaryDto } from "@/types/api";

// Cấu hình ISR: Tự động tái sinh trang tĩnh ngầm định sau mỗi 3600 giây (1 giờ)
export const revalidate = 3600;

// Dữ liệu mẫu dự phòng khi Backend đang khởi động hoặc chưa có bài viết
const fallbackCategories: CategoryDto[] = [
  {
    id: "cat-1",
    name: "Món Khai Vị",
    slug: "mon-khai-vi",
    description: "Các món salad thanh mát, gỏi cuốn và súp khởi đầu bữa ăn.",
    imageUrl: "https://images.unsplash.com/photo-1540420773420-3366772f4999?auto=format&fit=crop&w=400&q=80",
    orderIndex: 1,
    recipeCount: 12,
  },
  {
    id: "cat-2",
    name: "Món Chính",
    slug: "mon-chinh",
    description: "Món kho, xào, nướng đậm đà phong vị bữa cơm gia đình.",
    imageUrl: "https://images.unsplash.com/photo-1544025162-d76694265947?auto=format&fit=crop&w=400&q=80",
    orderIndex: 2,
    recipeCount: 36,
  },
  {
    id: "cat-3",
    name: "Món Canh & Súp",
    slug: "mon-canh-sup",
    description: "Nước dùng ngọt lành, thanh nhiệt và bồi bổ sức khỏe.",
    imageUrl: "https://images.unsplash.com/photo-1547592166-23ac45744acd?auto=format&fit=crop&w=400&q=80",
    orderIndex: 3,
    recipeCount: 18,
  },
  {
    id: "cat-4",
    name: "Món Tráng Miệng",
    slug: "mon-trang-mieng",
    description: "Bánh ngọt, chè thanh và các món ăn chơi hấp dẫn.",
    imageUrl: "https://images.unsplash.com/photo-1551024709-8f23befc6f87?auto=format&fit=crop&w=400&q=80",
    orderIndex: 4,
    recipeCount: 24,
  },
  {
    id: "cat-5",
    name: "Đồ Uống & Trà",
    slug: "do-uong-tra",
    description: "Nước ép tươi ngon, sinh tố và trà hoa thanh lọc.",
    imageUrl: "https://images.unsplash.com/photo-1513558161293-cdaf765ed2fd?auto=format&fit=crop&w=400&q=80",
    orderIndex: 5,
    recipeCount: 15,
  },
  {
    id: "cat-6",
    name: "Ăn Chay Thực Dưỡng",
    slug: "an-chay-thuc-duong",
    description: "Nguyên liệu tự nhiên thuần khiết từ rau củ tươi non.",
    imageUrl: "https://images.unsplash.com/photo-1512621776951-a57141f2eefd?auto=format&fit=crop&w=400&q=80",
    orderIndex: 6,
    recipeCount: 20,
  },
];

const fallbackRecipes: RecipeSummaryDto[] = [
  {
    id: "rec-1",
    title: "Phở Bò Tái Lăn Hà Nội Truyền Thống",
    slug: "pho-bo-tai-lan-ha-noi",
    description: "Nước dùng trong veo ngọt thanh ninh từ xương bò, thịt bò xào lăn thơm lừng tỏi gừng.",
    prepTime: 30,
    cookTime: 90,
    totalTime: 120,
    servings: 4,
    difficulty: "Medium",
    primaryImageUrl: "https://images.unsplash.com/photo-1582878826629-29b7ad1cdc43?auto=format&fit=crop&w=800&q=80",
    categoryId: "cat-2",
    categoryName: "Món Chính",
    categorySlug: "mon-chinh",
    calories: 520,
    author: { id: "a1", userName: "quanquan", displayName: "Trần Quốc Quân" },
    createdAt: new Date().toISOString(),
  },
  {
    id: "rec-2",
    title: "Cá Hồi Áp Chảo Sốt Bơ Tỏi Măng Tây",
    slug: "ca-hoi-ap-chao-sot-bo-toi",
    description: "Miếng cá hồi giòn da béo ngậy, đẫm sốt bơ tỏi chanh tươi cùng măng tây giòn ngọt.",
    prepTime: 15,
    cookTime: 15,
    totalTime: 30,
    servings: 2,
    difficulty: "Easy",
    primaryImageUrl: "https://images.unsplash.com/photo-1467003909585-2f8a72700288?auto=format&fit=crop&w=800&q=80",
    categoryId: "cat-2",
    categoryName: "Món Chính",
    categorySlug: "mon-chinh",
    calories: 450,
    author: { id: "a1", userName: "quanquan", displayName: "Trần Quốc Quân" },
    createdAt: new Date().toISOString(),
  },
  {
    id: "rec-3",
    title: "Salad Ức Gà Nướng Bơ Quả Mediterranean",
    slug: "salad-uc-ga-nuong-bo-qua",
    description: "Bữa ăn giàu đạm thanh mát, sốt dầu ô liu giấm táo chuẩn chế độ dinh dưỡng lành mạnh.",
    prepTime: 20,
    cookTime: 10,
    totalTime: 30,
    servings: 2,
    difficulty: "Easy",
    primaryImageUrl: "https://images.unsplash.com/photo-1540420773420-3366772f4999?auto=format&fit=crop&w=800&q=80",
    categoryId: "cat-1",
    categoryName: "Món Khai Vị",
    categorySlug: "mon-khai-vi",
    calories: 380,
    author: { id: "a1", userName: "quanquan", displayName: "Trần Quốc Quân" },
    createdAt: new Date().toISOString(),
  },
  {
    id: "rec-4",
    title: "Bò Kho Bánh Mì Nước Cốt Dừa Chuẩn Vị Nam Bộ",
    slug: "bo-kho-banh-mi-nuoc-cot-dua",
    description: "Bò nạm mềm nhừ đậm đà, nước sốt sánh vàng óng ánh thơm ngát hoa hồi và sả tươi.",
    prepTime: 25,
    cookTime: 60,
    totalTime: 85,
    servings: 6,
    difficulty: "Medium",
    primaryImageUrl: "https://images.unsplash.com/photo-1544025162-d76694265947?auto=format&fit=crop&w=800&q=80",
    categoryId: "cat-2",
    categoryName: "Món Chính",
    categorySlug: "mon-chinh",
    calories: 610,
    author: { id: "a1", userName: "quanquan", displayName: "Trần Quốc Quân" },
    createdAt: new Date().toISOString(),
  },
  {
    id: "rec-5",
    title: "Bánh Mousse Xoài Chanh Leo Mịn Màng",
    slug: "banh-mousse-xoai-chanh-leo",
    description: "Vị chua ngọt thanh khiết hòa quyện cùng lớp kem tươi béo ngậy tan chảy nơi đầu lưỡi.",
    prepTime: 40,
    cookTime: 0,
    totalTime: 40,
    servings: 8,
    difficulty: "Hard",
    primaryImageUrl: "https://images.unsplash.com/photo-1551024709-8f23befc6f87?auto=format&fit=crop&w=800&q=80",
    categoryId: "cat-4",
    categoryName: "Món Tráng Miệng",
    categorySlug: "mon-trang-mieng",
    calories: 290,
    author: { id: "a1", userName: "quanquan", displayName: "Trần Quốc Quân" },
    createdAt: new Date().toISOString(),
  },
  {
    id: "rec-6",
    title: "Súp Bí Đỏ Hạt Sen Nấu Nấm Đông Cô",
    slug: "sup-bi-do-hat-sen-nam-dong-co",
    description: "Món súp chay bồi bổ thể lực, vị ngọt thanh mát tự nhiên từ bí đỏ và hạt sen bùi thơm.",
    prepTime: 15,
    cookTime: 25,
    totalTime: 40,
    servings: 4,
    difficulty: "Easy",
    primaryImageUrl: "https://images.unsplash.com/photo-1547592166-23ac45744acd?auto=format&fit=crop&w=800&q=80",
    categoryId: "cat-3",
    categoryName: "Món Canh & Súp",
    categorySlug: "mon-canh-sup",
    calories: 210,
    author: { id: "a1", userName: "quanquan", displayName: "Trần Quốc Quân" },
    createdAt: new Date().toISOString(),
  },
];

export default async function HomePage() {
  // Tải dữ liệu song song từ Backend với Fallback an toàn
  let categories: CategoryDto[] = [];
  let recipes: RecipeSummaryDto[] = [];

  try {
    const [catRes, recRes] = await Promise.all([
      getCategories().catch(() => []),
      getRecipes({ pageSize: 6, sortBy: "newest" }).catch(() => ({ data: [], meta: null })),
    ]);

    categories = catRes && catRes.length > 0 ? catRes.slice(0, 6) : fallbackCategories;
    recipes = recRes?.data && recRes.data.length > 0 ? recRes.data.slice(0, 6) : fallbackRecipes;
  } catch {
    categories = fallbackCategories;
    recipes = fallbackRecipes;
  }

  return (
    <div>
      {/* =====================================================================
          1. HERO BANNER ẤM CÚNG & NỔI BẬT
          ===================================================================== */}
      <section
        style={{
          position: "relative",
          background: "linear-gradient(135deg, #fff7ed 0%, #fbf9f6 50%, #fef3c7 100%)",
          borderBottom: "1px solid var(--border-subtle)",
          paddingTop: "64px",
          paddingBottom: "80px",
          overflow: "hidden",
        }}
      >
        <div className="container">
          <div
            style={{
              display: "grid",
              gridTemplateColumns: "1fr",
              gap: "40px",
              alignItems: "center",
            }}
            className="hero-grid"
          >
            {/* Cột chữ Hero */}
            <div style={{ display: "flex", flexDirection: "column", gap: "20px" }}>
              <span className="eyebrow">
                ✨ Kho tàng công thức nấu ăn tuyển chọn
              </span>

              <h1 style={{ fontSize: "clamp(2.4rem, 4.5vw + 1rem, 3.8rem)", lineHeight: 1.15 }}>
                Nghệ Thuật Ẩm Thực &amp;{" "}
                <span style={{ color: "var(--primary)" }}>Hương Vị Đích Thực</span>
              </h1>

              <p style={{ fontSize: "1.15rem", color: "var(--text-secondary)", lineHeight: 1.65, maxWidth: "58ch" }}>
                Khám phá hàng ngàn công thức nấu ăn chuẩn vị từ các đầu bếp tài hoa. Từng bước làm tỉ mỉ, định lượng nguyên liệu chính xác và thông tin dinh dưỡng minh bạch.
              </p>

              {/* Nút Call To Action */}
              <div style={{ display: "flex", gap: "14px", flexWrap: "wrap", marginTop: "8px" }}>
                <Link href="/recipes" className="btn btn-primary btn-lg">
                  🍲 Khám phá công thức
                </Link>
                <Link href="/search" className="btn btn-outline btn-lg">
                  🔍 Tìm kiếm theo nguyên liệu
                </Link>
              </div>

              {/* Thống kê nhanh */}
              <div
                style={{
                  display: "flex",
                  gap: "32px",
                  paddingTop: "24px",
                  borderTop: "1px solid var(--border-subtle)",
                  marginTop: "12px",
                }}
              >
                <div>
                  <div style={{ fontSize: "1.6rem", fontWeight: 800, color: "var(--primary)" }}>100+</div>
                  <div style={{ fontSize: "0.82rem", color: "var(--text-muted)" }}>Công thức chuẩn vị</div>
                </div>
                <div>
                  <div style={{ fontSize: "1.6rem", fontWeight: 800, color: "var(--secondary)" }}>100%</div>
                  <div style={{ fontSize: "0.82rem", color: "var(--text-muted)" }}>Dinh dưỡng minh bạch</div>
                </div>
                <div>
                  <div style={{ fontSize: "1.6rem", fontWeight: 800, color: "var(--accent-amber)" }}>20+</div>
                  <div style={{ fontSize: "0.82rem", color: "var(--text-muted)" }}>Danh mục phong phú</div>
                </div>
              </div>
            </div>

            {/* Cột ảnh minh họa Hero */}
            <div
              style={{
                position: "relative",
                width: "100%",
                paddingTop: "75%",
                borderRadius: "var(--radius-xl)",
                overflow: "hidden",
                boxShadow: "var(--shadow-lg)",
                border: "4px solid #ffffff",
              }}
            >
              <Image
                src="https://images.unsplash.com/photo-1555396273-367ea4eb4db5?auto=format&fit=crop&w=1000&q=85"
                alt="Không gian bếp ẩm thực ấm cúng"
                fill
                priority
                sizes="(max-width: 860px) 100vw, 500px"
                style={{ objectFit: "cover" }}
              />
            </div>
          </div>
        </div>

        <style jsx>{`
          @media (min-width: 860px) {
            .hero-grid {
              grid-template-columns: 1.2fr 1fr !important;
            }
          }
        `}</style>
      </section>

      {/* =====================================================================
          2. KHỐI DANH MỤC MÓN ĂN NỔI BẬT (FEATURED CATEGORIES)
          ===================================================================== */}
      <section className="section" style={{ backgroundColor: "var(--bg-main)" }}>
        <div className="container">
          <div className="section-header section-header-row">
            <div>
              <span className="eyebrow">Khám phá theo chủ đề</span>
              <h2>Danh Mục Món Ăn Nổi Bật</h2>
            </div>
            <Link
              href="/categories"
              className="btn btn-ghost"
              style={{ fontWeight: 700, color: "var(--primary)" }}
            >
              Xem tất cả danh mục →
            </Link>
          </div>

          <div className="category-grid">
            {categories.map((cat) => (
              <CategoryCard key={cat.id || cat.slug} category={cat} />
            ))}
          </div>
        </div>
      </section>

      {/* =====================================================================
          3. KHỐI CÔNG THỨC MỚI NHẤT (RECENT RECIPES)
          ===================================================================== */}
      <section
        className="section"
        style={{
          backgroundColor: "var(--bg-surface)",
          borderTop: "1px solid var(--border-subtle)",
          borderBottom: "1px solid var(--border-subtle)",
        }}
      >
        <div className="container">
          <div className="section-header section-header-row">
            <div>
              <span className="eyebrow">Cập nhật mỗi ngày</span>
              <h2>Công Thức Mới Nhất</h2>
            </div>
            <Link
              href="/recipes"
              className="btn btn-ghost"
              style={{ fontWeight: 700, color: "var(--primary)" }}
            >
              Duyệt toàn bộ kho công thức →
            </Link>
          </div>

          <div className="recipe-grid">
            {recipes.map((rec, idx) => (
              <RecipeCard key={rec.id || rec.slug} recipe={rec} priority={idx < 3} />
            ))}
          </div>

          <div style={{ textAlign: "center", marginTop: "48px" }}>
            <Link href="/recipes" className="btn btn-primary btn-lg">
              Xem thêm các công thức khác →
            </Link>
          </div>
        </div>
      </section>

      {/* =====================================================================
          4. KHỐI TIÊU CHUẨN CHẤT LƯỢNG
          ===================================================================== */}
      <section className="section" style={{ backgroundColor: "var(--bg-surface-alt)" }}>
        <div className="container">
          <div style={{ textAlign: "center", maxWidth: "640px", margin: "0 auto 48px" }}>
            <span className="eyebrow">Tại sao chọn Culinary Blog</span>
            <h2>Nâng Tầm Kỹ Năng Nấu Nướng</h2>
            <p>Mang đến trải nghiệm vào bếp dễ dàng, hứng khởi và chuẩn chỉnh dinh dưỡng.</p>
          </div>

          <div
            style={{
              display: "grid",
              gridTemplateColumns: "repeat(auto-fit, minmax(260px, 1fr))",
              gap: "24px",
            }}
          >
            <div className="card" style={{ padding: "28px", textAlign: "center" }}>
              <div style={{ fontSize: "2.4rem", marginBottom: "12px" }}>⏱️</div>
              <h3 style={{ fontSize: "1.2rem", marginBottom: "8px" }}>Định Lượng Chuẩn Xác</h3>
              <p style={{ fontSize: "0.9rem" }}>Thời gian chuẩn bị, thời gian nấu và tỷ lệ gia vị được kiểm chứng kỹ lưỡng.</p>
            </div>

            <div className="card" style={{ padding: "28px", textAlign: "center" }}>
              <div style={{ fontSize: "2.4rem", marginBottom: "12px" }}>🥗</div>
              <h3 style={{ fontSize: "1.2rem", marginBottom: "8px" }}>Dinh Dưỡng Rõ Ràng</h3>
              <p style={{ fontSize: "0.9rem" }}>Bảng dinh dưỡng chi tiết về calo, chất béo, đạm và carbs hỗ trợ ăn uống khoa học.</p>
            </div>

            <div className="card" style={{ padding: "28px", textAlign: "center" }}>
              <div style={{ fontSize: "2.4rem", marginBottom: "12px" }}>🔍</div>
              <h3 style={{ fontSize: "1.2rem", marginBottom: "8px" }}>Tìm Kiếm Toàn Văn Bản</h3>
              <p style={{ fontSize: "0.9rem" }}>Hỗ trợ tìm kiếm tiếng Việt thông minh có dấu hoặc không dấu với tốc độ phản hồi tính bằng mili-giây.</p>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
}
