import React from "react";
import Link from "next/link";
import Image from "next/image";
import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import type { RecipeSummaryDto, RecipeDifficulty } from "@/types/api";

interface RecipeCardProps {
  recipe: RecipeSummaryDto;
  priority?: boolean;
}

function getDifficultyVariant(diff: RecipeDifficulty | number): {
  label: string;
  variant: "easy" | "medium" | "hard";
} {
  if (diff === "Easy" || diff === 0) return { label: "Dễ", variant: "easy" };
  if (diff === "Hard" || diff === 2) return { label: "Khó", variant: "hard" };
  return { label: "Vừa", variant: "medium" };
}

export function RecipeCard({ recipe, priority = false }: RecipeCardProps) {
  const diffInfo = getDifficultyVariant(recipe.difficulty);
  const fallbackImage =
    recipe.primaryImageUrl ||
    "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?auto=format&fit=crop&w=800&q=80";

  const authorAvatar =
    recipe.author?.avatarUrl ||
    `https://ui-avatars.com/api/?name=${encodeURIComponent(
      recipe.author?.displayName || recipe.author?.userName || "Bếp Trưởng"
    )}&background=fed8c4&color=c85a17`;

  const totalTime =
    recipe.totalTime ||
    (recipe.prepTime ?? recipe.prepTimeMinutes ?? 0) +
      (recipe.cookTime ?? recipe.cookTimeMinutes ?? 0) ||
    30;

  return (
    <Card className="group flex flex-col h-full overflow-hidden border border-[var(--border-subtle)] bg-[var(--bg-card)] rounded-2xl hover:shadow-xl hover:-translate-y-1 transition-all duration-300">
      {/* Khối Ảnh bìa công thức với tỷ lệ 16:10 */}
      <Link
        href={`/recipes/${recipe.slug}`}
        className="relative block w-full pt-[62.5%] overflow-hidden bg-[var(--bg-surface-alt)]"
      >
        <Image
          src={fallbackImage}
          alt={recipe.title}
          fill
          priority={priority}
          sizes="(max-width: 640px) 100vw, (max-width: 1024px) 50vw, 33vw"
          className="object-cover transition-transform duration-500 ease-out group-hover:scale-105"
        />

        {/* Lớp phủ chuyển sắc nhẹ để làm nổi bật nhãn */}
        <div className="absolute inset-0 bg-gradient-to-t from-black/40 via-transparent to-black/20 pointer-events-none" />

        {/* Badges góc trên */}
        <div className="absolute top-3 inset-x-3 flex items-center justify-between pointer-events-none gap-2">
          {recipe.categoryName && (
            <Badge variant="category" className="shadow-xs backdrop-blur-md">
              {recipe.categoryName}
            </Badge>
          )}

          <div className="ml-auto flex items-center gap-1.5">
            <Badge variant={diffInfo.variant} className="shadow-xs">
              {diffInfo.label}
            </Badge>

            {recipe.status && recipe.status !== "Published" && (
              <Badge
                variant={recipe.status === "Draft" ? "statusDraft" : "statusArchived"}
                className="shadow-xs"
              >
                {recipe.status === "Draft" ? "Bản nháp" : "Lưu trữ"}
              </Badge>
            )}
          </div>
        </div>
      </Link>

      {/* Thân thẻ */}
      <div className="p-5 flex flex-col flex-1 gap-2.5">
        {/* Chỉ số nhanh: Thời gian & Calo & Khẩu phần */}
        <div className="flex items-center flex-wrap gap-3 text-xs font-medium text-[var(--text-muted)]">
          <span className="inline-flex items-center gap-1">
            <span>⏱️</span> {totalTime} phút
          </span>
          {Boolean(recipe.calories) && (
            <span className="inline-flex items-center gap-1">
              <span>🔥</span> {recipe.calories} kcal
            </span>
          )}
          {Boolean(recipe.servings) && (
            <span className="inline-flex items-center gap-1">
              <span>👥</span> {recipe.servings} phần
            </span>
          )}
        </div>

        {/* Tiêu đề công thức */}
        <h3 className="text-lg font-bold leading-snug tracking-tight m-0">
          <Link
            href={`/recipes/${recipe.slug}`}
            className="line-clamp-2 text-[var(--text-primary)] hover:text-[var(--primary)] transition-colors duration-200"
          >
            {recipe.title}
          </Link>
        </h3>

        {/* Mô tả ngắn */}
        <p className="line-clamp-2 text-sm text-[var(--text-secondary)] leading-relaxed m-0">
          {recipe.description}
        </p>

        {/* Chân thẻ: Thông tin tác giả & CTA */}
        <div className="mt-auto pt-3 border-t border-[var(--border-subtle)] flex items-center justify-between gap-3">
          <div className="flex items-center gap-2 min-w-0">
            <div className="relative w-7 h-7 rounded-full overflow-hidden border border-[var(--border-subtle)] flex-shrink-0">
              <Image
                src={authorAvatar}
                alt={recipe.author?.displayName || recipe.author?.userName || "Tác giả"}
                fill
                sizes="28px"
                unoptimized
                className="object-cover"
              />
            </div>
            <span className="text-xs font-semibold text-[var(--text-primary)] truncate">
              {recipe.author?.displayName || recipe.author?.userName || "Ẩm thực gia"}
            </span>
          </div>

          <Link
            href={`/recipes/${recipe.slug}`}
            className="inline-flex items-center text-xs font-bold text-[var(--primary)] hover:text-[var(--primary-hover)] whitespace-nowrap transition-colors"
          >
            Xem cách làm
            <span className="ml-1 transition-transform group-hover:translate-x-0.5">→</span>
          </Link>
        </div>
      </div>
    </Card>
  );
}
