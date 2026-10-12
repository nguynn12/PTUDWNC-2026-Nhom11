import React from "react";
import Link from "next/link";
import Image from "next/image";
import type { RecipeDetailDto, RecipeDifficulty } from "@/types/api";

interface RecipeDetailProps {
  recipe: RecipeDetailDto;
}

function getDifficultyBadge(diff: RecipeDifficulty) {
  if (diff === "Easy" || diff === 0) return { label: "Độ khó: Dễ", className: "badge-easy" };
  if (diff === "Hard" || diff === 2) return { label: "Độ khó: Khó", className: "badge-hard" };
  return { label: "Độ khó: Vừa", className: "badge-medium" };
}

export function RecipeDetail({ recipe }: RecipeDetailProps) {
  const diff = getDifficultyBadge(recipe.difficulty);
  const heroImage =
    recipe.primaryImageUrl ||
    recipe.images?.find((img) => img.isPrimary)?.imageUrl ||
    recipe.images?.[0]?.imageUrl ||
    "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?auto=format&fit=crop&w=1200&q=85";

  const authorAvatar =
    recipe.author?.avatarUrl ||
    `https://ui-avatars.com/api/?name=${encodeURIComponent(
      recipe.author?.displayName || recipe.author?.userName || "Bếp Trưởng"
    )}&background=fed8c4&color=c85a17`;

  // Định dạng ngày đăng bài
  const formattedDate = recipe.publishedAt
    ? new Date(recipe.publishedAt).toLocaleDateString("vi-VN", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
      })
    : null;

  // Sắp xếp nguyên liệu theo sortOrder
  const sortedIngredients = [...(recipe.ingredients || [])].sort(
    (a, b) => a.sortOrder - b.sortOrder
  );

  // Sắp xếp các bước theo stepNumber
  const sortedSteps = [...(recipe.steps || [])].sort(
    (a, b) => a.stepNumber - b.stepNumber
  );

  // Thẻ dữ liệu có cấu trúc JSON-LD Schema.org Recipe phục vụ SEO Google (SRS NFR-SEO)
  const jsonLd = {
    "@context": "https://schema.org",
    "@type": "Recipe",
    name: recipe.title,
    description: recipe.description,
    image: heroImage,
    author: {
      "@type": "Person",
      name: recipe.author?.displayName || recipe.author?.userName || "Culinary Blog",
    },
    datePublished: recipe.publishedAt,
    prepTime: `PT${recipe.prepTime || 15}M`,
    cookTime: `PT${recipe.cookTime || 30}M`,
    totalTime: `PT${recipe.totalTime || (recipe.prepTime + recipe.cookTime) || 45}M`,
    recipeYield: `${recipe.servings || 4} khẩu phần`,
    recipeCategory: recipe.categoryName,
    recipeIngredient: sortedIngredients.map(
      (ing) => `${ing.quantity} ${ing.unit} ${ing.name}`.trim()
    ),
    recipeInstructions: sortedSteps.map((step) => ({
      "@type": "HowToStep",
      text: step.description,
      name: step.title || `Bước ${step.stepNumber}`,
    })),
    nutrition: recipe.nutrition
      ? {
          "@type": "NutritionInformation",
          calories: recipe.nutrition.calories ? `${recipe.nutrition.calories} calories` : undefined,
          fatContent: recipe.nutrition.fatGrams ? `${recipe.nutrition.fatGrams} g` : undefined,
          carbohydrateContent: recipe.nutrition.carbsGrams ? `${recipe.nutrition.carbsGrams} g` : undefined,
          proteinContent: recipe.nutrition.proteinGrams ? `${recipe.nutrition.proteinGrams} g` : undefined,
        }
      : undefined,
  };

  return (
    <article className="container section" style={{ maxWidth: "980px" }}>
      {/* Nhúng thẻ dữ liệu có cấu trúc JSON-LD Schema.org cho SEO */}
      <script
        type="application/ld+json"
        dangerouslySetInnerHTML={{ __html: JSON.stringify(jsonLd) }}
      />

      {/* Điều hướng Breadcrumbs */}
      <nav
        aria-label="Breadcrumb"
        style={{
          display: "flex",
          alignItems: "center",
          gap: "8px",
          fontSize: "0.88rem",
          color: "var(--text-muted)",
          marginBottom: "20px",
          flexWrap: "wrap",
        }}
      >
        <Link href="/" style={{ color: "var(--text-secondary)" }}>Trang chủ</Link>
        <span>/</span>
        <Link href="/recipes" style={{ color: "var(--text-secondary)" }}>Công thức</Link>
        {recipe.categorySlug && (
          <>
            <span>/</span>
            <Link
              href={`/categories/${recipe.categorySlug}`}
              style={{ color: "var(--primary)", fontWeight: 600 }}
            >
              {recipe.categoryName}
            </Link>
          </>
        )}
      </nav>

      {/* Tiêu đề & Thông tin đầu bài */}
      <header style={{ marginBottom: "28px", display: "flex", flexDirection: "column", gap: "14px" }}>
        <div style={{ display: "flex", gap: "8px", flexWrap: "wrap" }}>
          {recipe.categoryName && (
            <span className="badge badge-category">{recipe.categoryName}</span>
          )}
          <span className={`badge ${diff.className}`}>{diff.label}</span>
        </div>

        <h1 style={{ fontSize: "clamp(2rem, 3.5vw + 1rem, 2.85rem)", lineHeight: 1.25 }}>
          {recipe.title}
        </h1>

        <p style={{ fontSize: "1.1rem", color: "var(--text-secondary)", lineHeight: 1.6 }}>
          {recipe.description}
        </p>

        {/* Thông tin tác giả & Thời gian xuất bản */}
        <div
          style={{
            display: "flex",
            alignItems: "center",
            gap: "12px",
            paddingTop: "12px",
            borderTop: "1px solid var(--border-subtle)",
          }}
        >
          <div
            style={{
              position: "relative",
              width: "44px",
              height: "44px",
              borderRadius: "var(--radius-full)",
              overflow: "hidden",
              border: "2px solid var(--primary-light)",
            }}
          >
            <Image
              src={authorAvatar}
              alt={recipe.author?.displayName || recipe.author?.userName || "Tác giả"}
              fill
              sizes="44px"
              unoptimized
              style={{ objectFit: "cover" }}
            />
          </div>
          <div>
            <div style={{ fontWeight: 700, fontSize: "0.95rem", color: "var(--text-primary)" }}>
              {recipe.author?.displayName || recipe.author?.userName || "Chuyên gia Ẩm thực"}
            </div>
            {formattedDate && (
              <div style={{ fontSize: "0.82rem", color: "var(--text-muted)" }} suppressHydrationWarning>
                Xuất bản: {formattedDate}
              </div>
            )}
          </div>
        </div>
      </header>

      {/* Ảnh chính Hero Image (LCP Optimization với cờ priority) */}
      <div
        style={{
          position: "relative",
          width: "100%",
          paddingTop: "56.25%", // Tỷ lệ 16:9
          borderRadius: "var(--radius-xl)",
          overflow: "hidden",
          boxShadow: "var(--shadow-md)",
          marginBottom: "36px",
          backgroundColor: "var(--bg-surface-alt)",
        }}
      >
        <Image
          src={heroImage}
          alt={recipe.title}
          fill
          priority
          sizes="(max-width: 1024px) 100vw, 980px"
          style={{ objectFit: "cover" }}
        />
      </div>

      {/* Thanh thông số nhanh (Quick Stats Bar) */}
      <div
        style={{
          display: "grid",
          gridTemplateColumns: "repeat(auto-fit, minmax(130px, 1fr))",
          gap: "16px",
          padding: "20px",
          backgroundColor: "var(--bg-surface)",
          border: "1px solid var(--border-subtle)",
          borderRadius: "var(--radius-lg)",
          boxShadow: "var(--shadow-xs)",
          marginBottom: "40px",
        }}
      >
        <div style={{ textAlign: "center" }}>
          <div style={{ fontSize: "0.8rem", color: "var(--text-muted)", textTransform: "uppercase", fontWeight: 600 }}>Chuẩn bị</div>
          <div style={{ fontSize: "1.2rem", fontWeight: 700, color: "var(--text-primary)", marginTop: "4px" }}>
            {recipe.prepTime || 15} phút
          </div>
        </div>
        <div style={{ textAlign: "center" }}>
          <div style={{ fontSize: "0.8rem", color: "var(--text-muted)", textTransform: "uppercase", fontWeight: 600 }}>Thời gian nấu</div>
          <div style={{ fontSize: "1.2rem", fontWeight: 700, color: "var(--text-primary)", marginTop: "4px" }}>
            {recipe.cookTime || 30} phút
          </div>
        </div>
        <div style={{ textAlign: "center" }}>
          <div style={{ fontSize: "0.8rem", color: "var(--text-muted)", textTransform: "uppercase", fontWeight: 600 }}>Tổng thời gian</div>
          <div style={{ fontSize: "1.2rem", fontWeight: 700, color: "var(--primary)", marginTop: "4px" }}>
            {recipe.totalTime || (recipe.prepTime + recipe.cookTime) || 45} phút
          </div>
        </div>
        <div style={{ textAlign: "center" }}>
          <div style={{ fontSize: "0.8rem", color: "var(--text-muted)", textTransform: "uppercase", fontWeight: 600 }}>Khẩu phần</div>
          <div style={{ fontSize: "1.2rem", fontWeight: 700, color: "var(--text-primary)", marginTop: "4px" }}>
            {recipe.servings || 4} người
          </div>
        </div>
        {recipe.nutrition?.calories && (
          <div style={{ textAlign: "center" }}>
            <div style={{ fontSize: "0.8rem", color: "var(--text-muted)", textTransform: "uppercase", fontWeight: 600 }}>Năng lượng</div>
            <div style={{ fontSize: "1.2rem", fontWeight: 700, color: "var(--secondary)", marginTop: "4px" }}>
              {recipe.nutrition.calories} kcal
            </div>
          </div>
        )}
      </div>

      {/* Bố cục 2 cột: Nguyên liệu & Dinh dưỡng bên trái, Các bước bên phải */}
      <div
        style={{
          display: "grid",
          gridTemplateColumns: "1fr",
          gap: "40px",
        }}
      >
        {/* KHỐI 1: NGUYÊN LIỆU NẤU ĂN */}
        <section
          style={{
            padding: "28px",
            backgroundColor: "var(--bg-surface)",
            borderRadius: "var(--radius-lg)",
            border: "1px solid var(--border-subtle)",
            boxShadow: "var(--shadow-sm)",
          }}
        >
          <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", marginBottom: "20px" }}>
            <h2 style={{ fontSize: "1.45rem", display: "flex", alignItems: "center", gap: "8px" }}>
              🥗 Nguyên liệu chuẩn bị
            </h2>
            <span style={{ fontSize: "0.88rem", color: "var(--text-muted)" }}>
              {sortedIngredients.length} nguyên liệu
            </span>
          </div>

          <ul style={{ listStyle: "none", display: "flex", flexDirection: "column", gap: "12px", padding: 0 }}>
            {sortedIngredients.map((ing) => (
              <li
                key={ing.id || ing.name}
                style={{
                  display: "flex",
                  alignItems: "baseline",
                  justifyContent: "space-between",
                  paddingBottom: "10px",
                  borderBottom: "1px dashed var(--border-subtle)",
                  fontSize: "1rem",
                }}
              >
                <span style={{ fontWeight: 600, color: "var(--text-primary)" }}>
                  {ing.name}
                </span>
                <span
                  style={{
                    color: "var(--primary)",
                    fontWeight: 700,
                    backgroundColor: "var(--primary-light)",
                    padding: "2px 8px",
                    borderRadius: "var(--radius-sm)",
                    fontSize: "0.9rem",
                  }}
                >
                  {ing.quantity} {ing.unit}
                </span>
              </li>
            ))}
          </ul>
        </section>

        {/* KHỐI 2: THÔNG TIN DINH DƯỠNG (KÈM LƯU Ý BẮT BUỘC THEO SRS DÒNG 1295) */}
        {recipe.nutrition && (
          <section
            style={{
              padding: "24px",
              backgroundColor: "var(--bg-surface-alt)",
              borderRadius: "var(--radius-lg)",
              border: "1px solid var(--border-default)",
            }}
          >
            <h3 style={{ fontSize: "1.2rem", marginBottom: "16px", display: "flex", alignItems: "center", gap: "8px" }}>
              📊 Giá trị dinh dưỡng (mỗi khẩu phần)
            </h3>

            <div
              style={{
                display: "grid",
                gridTemplateColumns: "repeat(auto-fit, minmax(110px, 1fr))",
                gap: "12px",
                marginBottom: "16px",
              }}
            >
              {recipe.nutrition.calories && (
                <div style={{ backgroundColor: "var(--bg-surface)", padding: "12px", borderRadius: "var(--radius-md)", textAlign: "center" }}>
                  <div style={{ fontSize: "0.78rem", color: "var(--text-muted)" }}>Calo</div>
                  <div style={{ fontSize: "1.1rem", fontWeight: 700, color: "var(--primary)" }}>{recipe.nutrition.calories} kcal</div>
                </div>
              )}
              {recipe.nutrition.proteinGrams !== undefined && recipe.nutrition.proteinGrams !== null && (
                <div style={{ backgroundColor: "var(--bg-surface)", padding: "12px", borderRadius: "var(--radius-md)", textAlign: "center" }}>
                  <div style={{ fontSize: "0.78rem", color: "var(--text-muted)" }}>Chất đạm</div>
                  <div style={{ fontSize: "1.1rem", fontWeight: 700 }}>{recipe.nutrition.proteinGrams} g</div>
                </div>
              )}
              {recipe.nutrition.carbsGrams !== undefined && recipe.nutrition.carbsGrams !== null && (
                <div style={{ backgroundColor: "var(--bg-surface)", padding: "12px", borderRadius: "var(--radius-md)", textAlign: "center" }}>
                  <div style={{ fontSize: "0.78rem", color: "var(--text-muted)" }}>Tinh bột</div>
                  <div style={{ fontSize: "1.1rem", fontWeight: 700 }}>{recipe.nutrition.carbsGrams} g</div>
                </div>
              )}
              {recipe.nutrition.fatGrams !== undefined && recipe.nutrition.fatGrams !== null && (
                <div style={{ backgroundColor: "var(--bg-surface)", padding: "12px", borderRadius: "var(--radius-md)", textAlign: "center" }}>
                  <div style={{ fontSize: "0.78rem", color: "var(--text-muted)" }}>Chất béo</div>
                  <div style={{ fontSize: "1.1rem", fontWeight: 700 }}>{recipe.nutrition.fatGrams} g</div>
                </div>
              )}
              {recipe.nutrition.fiberGrams !== undefined && recipe.nutrition.fiberGrams !== null && (
                <div style={{ backgroundColor: "var(--bg-surface)", padding: "12px", borderRadius: "var(--radius-md)", textAlign: "center" }}>
                  <div style={{ fontSize: "0.78rem", color: "var(--text-muted)" }}>Chất xơ</div>
                  <div style={{ fontSize: "1.1rem", fontWeight: 700 }}>{recipe.nutrition.fiberGrams} g</div>
                </div>
              )}
            </div>

            {/* DÒNG LƯU Ý BẮT BUỘC THEO ĐẶC TẢ SRS DÒNG 1295 */}
            <p
              style={{
                fontSize: "0.85rem",
                color: "var(--text-muted)",
                fontStyle: "italic",
                borderLeft: "3px solid var(--accent-amber)",
                paddingLeft: "10px",
                margin: 0,
              }}
            >
              “Thông tin dinh dưỡng do tác giả cung cấp và chỉ mang tính tham khảo.”
            </p>
          </section>
        )}

        {/* KHỐI 3: HƯỚNG DẪN CÁC BƯỚC NẤU ĂN */}
        <section style={{ display: "flex", flexDirection: "column", gap: "24px" }}>
          <h2 style={{ fontSize: "1.6rem", display: "flex", alignItems: "center", gap: "10px" }}>
            👩‍🍳 Hướng dẫn các bước thực hiện
          </h2>

          <div style={{ display: "flex", flexDirection: "column", gap: "24px" }}>
            {sortedSteps.map((step) => (
              <div
                key={step.id || step.stepNumber}
                style={{
                  display: "flex",
                  gap: "20px",
                  padding: "24px",
                  backgroundColor: "var(--bg-surface)",
                  borderRadius: "var(--radius-lg)",
                  border: "1px solid var(--border-subtle)",
                  boxShadow: "var(--shadow-xs)",
                }}
              >
                {/* Số thứ tự bước tròn */}
                <div
                  style={{
                    width: "40px",
                    height: "40px",
                    borderRadius: "var(--radius-full)",
                    backgroundColor: "var(--primary)",
                    color: "var(--text-inverse)",
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                    fontWeight: 800,
                    fontSize: "1.1rem",
                    flexShrink: 0,
                    boxShadow: "0 2px 6px rgba(200, 90, 23, 0.3)",
                  }}
                >
                  {step.stepNumber}
                </div>

                {/* Nội dung bước làm */}
                <div style={{ flex: 1, display: "flex", flexDirection: "column", gap: "10px" }}>
                  <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", flexWrap: "wrap", gap: "8px" }}>
                    <h3 style={{ fontSize: "1.2rem", margin: 0, color: "var(--text-primary)" }}>
                      {step.title || `Bước ${step.stepNumber}`}
                    </h3>

                    {/* Hẹn giờ nấu nếu có */}
                    {step.timerMinutes && (
                      <span
                        className="badge"
                        style={{
                          backgroundColor: "var(--accent-amber-light)",
                          color: "var(--accent-amber)",
                          border: "1px solid #fde68a",
                          fontSize: "0.85rem",
                        }}
                      >
                        ⏱️ Hẹn giờ: {step.timerMinutes} phút
                      </span>
                    )}
                  </div>

                  <p style={{ fontSize: "1rem", color: "var(--text-secondary)", lineHeight: 1.7, margin: 0 }}>
                    {step.description}
                  </p>

                  {/* Ảnh minh họa bước làm nếu có */}
                  {step.imageUrl && (
                    <div
                      style={{
                        position: "relative",
                        width: "100%",
                        height: "240px",
                        borderRadius: "var(--radius-md)",
                        overflow: "hidden",
                        marginTop: "10px",
                      }}
                    >
                      <Image
                        src={step.imageUrl}
                        alt={`Ảnh minh họa bước ${step.stepNumber}`}
                        fill
                        sizes="(max-width: 768px) 100vw, 600px"
                        style={{ objectFit: "cover" }}
                      />
                    </div>
                  )}
                </div>
              </div>
            ))}
          </div>
        </section>
      </div>
    </article>
  );
}
