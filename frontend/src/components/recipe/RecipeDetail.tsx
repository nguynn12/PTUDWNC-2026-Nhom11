"use client";

import React, { useState } from "react";
import Link from "next/link";
import Image from "next/image";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import type { RecipeDetailDto, RecipeDifficulty } from "@/types/api";

interface RecipeDetailProps {
  recipe: RecipeDetailDto;
}

function getDifficultyBadge(diff: RecipeDifficulty): {
  label: string;
  variant: "easy" | "medium" | "hard";
} {
  if (diff === "Easy" || diff === 0) return { label: "Độ khó: Dễ", variant: "easy" };
  if (diff === "Hard" || diff === 2) return { label: "Độ khó: Khó", variant: "hard" };
  return { label: "Độ khó: Vừa", variant: "medium" };
}

export function RecipeDetail({ recipe }: RecipeDetailProps) {
  const diff = getDifficultyBadge(recipe.difficulty);

  const heroImage =
    recipe.primaryImageUrl ||
    recipe.images?.find((img) => img.isPrimary)?.imageUrl ||
    recipe.images?.[0]?.imageUrl ||
    "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?auto=format&fit=crop&w=1200&q=85";

  // State thư viện ảnh (Image Gallery)
  const [selectedImage, setSelectedImage] = useState<string>(heroImage);

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

  // Danh sách hình ảnh của công thức (loại bỏ trùng lặp nếu có)
  const allImages = React.useMemo(() => {
    const list: string[] = [];
    if (recipe.primaryImageUrl) list.push(recipe.primaryImageUrl);
    if (recipe.images && recipe.images.length > 0) {
      recipe.images.forEach((img) => {
        if (img.imageUrl && !list.includes(img.imageUrl)) {
          list.push(img.imageUrl);
        }
      });
    }
    return list.length > 0 ? list : [heroImage];
  }, [recipe.primaryImageUrl, recipe.images, heroImage]);

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
    totalTime: `PT${recipe.totalTime || (recipe.prepTime || 0) + (recipe.cookTime || 0) || 45}M`,
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
    <article className="container mx-auto px-4 py-8 md:py-12 max-w-5xl">
      {/* Nhúng thẻ dữ liệu có cấu trúc JSON-LD Schema.org cho SEO */}
      <script
        type="application/ld+json"
        dangerouslySetInnerHTML={{ __html: JSON.stringify(jsonLd) }}
      />

      {/* Điều hướng Breadcrumbs */}
      <nav
        aria-label="Breadcrumb"
        className="flex items-center gap-2 text-sm text-[var(--text-muted)] mb-6 flex-wrap"
      >
        <Link href="/" className="hover:text-[var(--primary)] transition-colors">
          Trang chủ
        </Link>
        <span>/</span>
        <Link href="/recipes" className="hover:text-[var(--primary)] transition-colors">
          Công thức
        </Link>
        {recipe.categorySlug && (
          <>
            <span>/</span>
            <Link
              href={`/categories/${recipe.categorySlug}`}
              className="text-[var(--primary)] font-semibold hover:underline"
            >
              {recipe.categoryName}
            </Link>
          </>
        )}
      </nav>

      {/* Tiêu đề & Thông tin đầu bài */}
      <header className="mb-8 space-y-4">
        <div className="flex items-center gap-2 flex-wrap">
          {recipe.categoryName && (
            <Badge variant="category" className="border border-[var(--border-subtle)] text-xs font-semibold">
              {recipe.categoryName}
            </Badge>
          )}
          <Badge variant={diff.variant} className="text-xs font-semibold">
            {diff.label}
          </Badge>
          {recipe.status && recipe.status !== "Published" && (
            <Badge
              variant={recipe.status === "Draft" ? "statusDraft" : "statusArchived"}
              className="text-xs"
            >
              {recipe.status === "Draft" ? "Bản nháp" : "Lưu trữ"}
            </Badge>
          )}
        </div>

        <h1 className="text-3xl sm:text-4xl md:text-5xl font-extrabold text-[var(--text-primary)] tracking-tight leading-tight">
          {recipe.title}
        </h1>

        <p className="text-base sm:text-lg text-[var(--text-secondary)] leading-relaxed max-w-3xl">
          {recipe.description}
        </p>

        {/* Thông tin tác giả & Thời gian xuất bản */}
        <div className="flex items-center gap-3 pt-4 border-t border-[var(--border-subtle)]">
          <div className="relative w-11 h-11 rounded-full overflow-hidden border-2 border-[var(--primary-light)] flex-shrink-0">
            <Image
              src={authorAvatar}
              alt={recipe.author?.displayName || recipe.author?.userName || "Tác giả"}
              fill
              sizes="44px"
              unoptimized
              className="object-cover"
            />
          </div>
          <div>
            <div className="font-bold text-sm sm:text-base text-[var(--text-primary)]">
              {recipe.author?.displayName || recipe.author?.userName || "Chuyên gia Ẩm thực"}
            </div>
            {formattedDate && (
              <div className="text-xs text-[var(--text-muted)]" suppressHydrationWarning>
                Xuất bản: {formattedDate}
              </div>
            )}
          </div>
        </div>
      </header>

      {/* ẢNH CHÍNH & THƯ VIỆN ẢNH (Image Gallery) */}
      <section className="mb-10 space-y-3">
        {/* Khối Ảnh chính Hero Image (Tỷ lệ 16:9) */}
        <div className="relative w-full pt-[56.25%] rounded-3xl overflow-hidden shadow-lg bg-[var(--bg-surface-alt)] border border-[var(--border-subtle)]">
          <Image
            src={selectedImage}
            alt={recipe.title}
            fill
            priority
            sizes="(max-width: 1024px) 100vw, 1024px"
            className="object-cover transition-all duration-300"
          />
        </div>

        {/* Thư viện ảnh dạng dải thumbnail bên dưới */}
        {allImages.length > 1 && (
          <div className="flex items-center gap-3 overflow-x-auto pb-2 pt-1">
            {allImages.map((imgUrl, idx) => (
              <button
                key={idx}
                type="button"
                onClick={() => setSelectedImage(imgUrl)}
                className={`relative w-20 h-20 sm:w-24 sm:h-24 rounded-xl overflow-hidden flex-shrink-0 border-2 transition-all cursor-pointer ${
                  selectedImage === imgUrl
                    ? "border-[var(--primary)] ring-2 ring-[var(--primary-subtle)] scale-102"
                    : "border-transparent opacity-70 hover:opacity-100"
                }`}
                title={`Xem ảnh ${idx + 1}`}
              >
                <Image
                  src={imgUrl}
                  alt={`Thumbnail ${idx + 1}`}
                  fill
                  sizes="96px"
                  className="object-cover"
                />
              </button>
            ))}
          </div>
        )}
      </section>

      {/* THANH THÔNG SỐ NHANH (Quick Stats Bar) */}
      <section className="grid grid-cols-2 sm:grid-cols-4 lg:grid-cols-5 gap-3 sm:gap-4 p-5 sm:p-6 bg-[var(--bg-surface)] border border-[var(--border-subtle)] rounded-2xl shadow-xs mb-10">
        <div className="text-center p-2 rounded-xl bg-[var(--bg-surface-alt)]/60">
          <div className="text-xs text-[var(--text-muted)] uppercase tracking-wider font-bold">
            Chuẩn bị
          </div>
          <div className="text-lg sm:text-xl font-bold text-[var(--text-primary)] mt-1">
            {recipe.prepTime || 15} phút
          </div>
        </div>

        <div className="text-center p-2 rounded-xl bg-[var(--bg-surface-alt)]/60">
          <div className="text-xs text-[var(--text-muted)] uppercase tracking-wider font-bold">
            Nấu nướng
          </div>
          <div className="text-lg sm:text-xl font-bold text-[var(--text-primary)] mt-1">
            {recipe.cookTime || 30} phút
          </div>
        </div>

        <div className="text-center p-2 rounded-xl bg-[var(--bg-surface-alt)]/60">
          <div className="text-xs text-[var(--text-muted)] uppercase tracking-wider font-bold">
            Tổng thời gian
          </div>
          <div className="text-lg sm:text-xl font-bold text-[var(--primary)] mt-1">
            {recipe.totalTime || (recipe.prepTime || 0) + (recipe.cookTime || 0) || 45} phút
          </div>
        </div>

        <div className="text-center p-2 rounded-xl bg-[var(--bg-surface-alt)]/60">
          <div className="text-xs text-[var(--text-muted)] uppercase tracking-wider font-bold">
            Khẩu phần
          </div>
          <div className="text-lg sm:text-xl font-bold text-[var(--text-primary)] mt-1">
            {recipe.servings || 4} người
          </div>
        </div>

        {Boolean(recipe.nutrition?.calories) && (
          <div className="col-span-2 sm:col-span-4 lg:col-span-1 text-center p-2 rounded-xl bg-[var(--secondary-light)]/60">
            <div className="text-xs text-[var(--secondary)] uppercase tracking-wider font-bold">
              Năng lượng
            </div>
            <div className="text-lg sm:text-xl font-bold text-[var(--secondary)] mt-1">
              {recipe.nutrition?.calories} kcal
            </div>
          </div>
        )}
      </section>

      {/* BỐ CỤC NỘI DUNG: NGUYÊN LIỆU + DINH DƯỠNG & CÁC BƯỚC THỰC HIỆN */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-8 lg:gap-10">
        {/* CỘT TRÁI (Lg: 4/12): Nguyên liệu & Dinh dưỡng */}
        <div className="lg:col-span-5 space-y-8">
          {/* KHỐI 1: NGUYÊN LIỆU NẤU ĂN */}
          <Card className="rounded-2xl border-[var(--border-subtle)] bg-[var(--bg-surface)] shadow-xs">
            <div className="p-5 sm:p-6">
              <div className="flex items-center justify-between mb-5 pb-3 border-b border-[var(--border-subtle)]">
                <h2 className="text-xl font-bold text-[var(--text-primary)] m-0 flex items-center gap-2">
                  <span>🥗</span>
                  <span>Nguyên liệu</span>
                </h2>
                <Badge variant="outline" className="text-xs font-semibold">
                  {sortedIngredients.length} nguyên liệu
                </Badge>
              </div>

              <ul className="space-y-3 p-0 list-none m-0">
                {sortedIngredients.map((ing) => (
                  <li
                    key={ing.id || ing.name}
                    className="flex items-center justify-between pb-2.5 border-b border-dashed border-[var(--border-subtle)] last:border-none text-sm sm:text-base"
                  >
                    <span className="font-medium text-[var(--text-primary)]">
                      {ing.name}
                    </span>
                    <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-bold text-[var(--primary)] bg-[var(--primary-light)]">
                      {ing.quantity} {ing.unit}
                    </span>
                  </li>
                ))}
              </ul>
            </div>
          </Card>

          {/* KHỐI 2: THÔNG TIN DINH DƯỠNG */}
          {recipe.nutrition && (
            <Card className="rounded-2xl border-[var(--border-subtle)] bg-[var(--bg-surface-alt)]/50 shadow-xs">
              <div className="p-5 sm:p-6 space-y-4">
                <h3 className="text-lg font-bold text-[var(--text-primary)] m-0 flex items-center gap-2">
                  <span>📊</span>
                  <span>Giá trị dinh dưỡng (mỗi khẩu phần)</span>
                </h3>

                <div className="grid grid-cols-2 sm:grid-cols-3 gap-2.5">
                  {recipe.nutrition.calories && (
                    <div className="bg-[var(--bg-surface)] p-3 rounded-xl border border-[var(--border-subtle)] text-center">
                      <div className="text-xs text-[var(--text-muted)] font-medium">Calo</div>
                      <div className="text-base font-bold text-[var(--primary)] mt-0.5">
                        {recipe.nutrition.calories} kcal
                      </div>
                    </div>
                  )}
                  {recipe.nutrition.proteinGrams !== undefined && recipe.nutrition.proteinGrams !== null && (
                    <div className="bg-[var(--bg-surface)] p-3 rounded-xl border border-[var(--border-subtle)] text-center">
                      <div className="text-xs text-[var(--text-muted)] font-medium">Chất đạm</div>
                      <div className="text-base font-bold text-[var(--text-primary)] mt-0.5">
                        {recipe.nutrition.proteinGrams} g
                      </div>
                    </div>
                  )}
                  {recipe.nutrition.carbsGrams !== undefined && recipe.nutrition.carbsGrams !== null && (
                    <div className="bg-[var(--bg-surface)] p-3 rounded-xl border border-[var(--border-subtle)] text-center">
                      <div className="text-xs text-[var(--text-muted)] font-medium">Tinh bột</div>
                      <div className="text-base font-bold text-[var(--text-primary)] mt-0.5">
                        {recipe.nutrition.carbsGrams} g
                      </div>
                    </div>
                  )}
                  {recipe.nutrition.fatGrams !== undefined && recipe.nutrition.fatGrams !== null && (
                    <div className="bg-[var(--bg-surface)] p-3 rounded-xl border border-[var(--border-subtle)] text-center">
                      <div className="text-xs text-[var(--text-muted)] font-medium">Chất béo</div>
                      <div className="text-base font-bold text-[var(--text-primary)] mt-0.5">
                        {recipe.nutrition.fatGrams} g
                      </div>
                    </div>
                  )}
                  {recipe.nutrition.fiberGrams !== undefined && recipe.nutrition.fiberGrams !== null && (
                    <div className="bg-[var(--bg-surface)] p-3 rounded-xl border border-[var(--border-subtle)] text-center">
                      <div className="text-xs text-[var(--text-muted)] font-medium">Chất xơ</div>
                      <div className="text-base font-bold text-[var(--text-primary)] mt-0.5">
                        {recipe.nutrition.fiberGrams} g
                      </div>
                    </div>
                  )}
                </div>

                {/* DÒNG LƯU Ý BẮT BUỘC THEO ĐẶC TẢ SRS DÒNG 1295 */}
                <p className="text-xs text-[var(--text-muted)] italic border-l-2 border-amber-500 pl-2.5 m-0 leading-relaxed">
                  “Thông tin dinh dưỡng do tác giả cung cấp và chỉ mang tính tham khảo.”
                </p>
              </div>
            </Card>
          )}
        </div>

        {/* CỘT PHẢI (Lg: 7/12): Hướng dẫn các bước nấu ăn */}
        <div className="lg:col-span-7 space-y-6">
          <div className="flex items-center justify-between pb-2 border-b border-[var(--border-subtle)]">
            <h2 className="text-2xl font-bold text-[var(--text-primary)] m-0 flex items-center gap-2">
              <span>👩‍🍳</span>
              <span>Các bước thực hiện</span>
            </h2>
            <span className="text-xs font-semibold text-[var(--text-muted)]">
              {sortedSteps.length} bước
            </span>
          </div>

          <div className="space-y-5">
            {sortedSteps.map((step) => (
              <Card
                key={step.id || step.stepNumber}
                className="overflow-hidden border-[var(--border-subtle)] bg-[var(--bg-surface)] shadow-xs rounded-2xl"
              >
                <CardContent className="p-5 sm:p-6">
                  <div className="flex items-start gap-4">
                    {/* Số thứ tự bước tròn */}
                    <div className="w-9 h-9 sm:w-10 sm:h-10 rounded-full bg-[var(--primary)] text-white flex items-center justify-center font-extrabold text-sm sm:text-base flex-shrink-0 shadow-sm shadow-[var(--primary)]/30">
                      {step.stepNumber}
                    </div>

                    {/* Nội dung bước làm */}
                    <div className="flex-1 space-y-3 min-w-0">
                      <div className="flex items-center justify-between flex-wrap gap-2">
                        <h3 className="text-base sm:text-lg font-bold text-[var(--text-primary)] m-0">
                          {step.title || `Bước ${step.stepNumber}`}
                        </h3>

                        {/* Hẹn giờ nấu nếu có */}
                        {step.timerMinutes && (
                          <Badge variant="medium" className="text-xs font-medium">
                            ⏱️ Hẹn giờ: {step.timerMinutes} phút
                          </Badge>
                        )}
                      </div>

                      <p className="text-sm sm:text-base text-[var(--text-secondary)] leading-relaxed m-0 whitespace-pre-line">
                        {step.description}
                      </p>

                      {/* Ảnh minh họa bước làm nếu có */}
                      {step.imageUrl && (
                        <div className="relative w-full pt-[52%] rounded-xl overflow-hidden mt-3 border border-[var(--border-subtle)] bg-[var(--bg-surface-alt)]">
                          <Image
                            src={step.imageUrl}
                            alt={`Ảnh minh họa bước ${step.stepNumber}`}
                            fill
                            sizes="(max-width: 768px) 100vw, 600px"
                            className="object-cover"
                          />
                        </div>
                      )}
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </div>
      </div>
    </article>
  );
}
