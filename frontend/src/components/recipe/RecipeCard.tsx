import React from "react";
import Link from "next/link";
import Image from "next/image";
import type { RecipeSummaryDto, RecipeDifficulty } from "@/types/api";

interface RecipeCardProps {
  recipe: RecipeSummaryDto;
  priority?: boolean;
}

function getDifficultyLabel(diff: RecipeDifficulty): { label: string; className: string } {
  if (diff === "Easy" || diff === 0) return { label: "Dễ", className: "badge-easy" };
  if (diff === "Hard" || diff === 2) return { label: "Khó", className: "badge-hard" };
  return { label: "Vừa", className: "badge-medium" };
}

export function RecipeCard({ recipe, priority = false }: RecipeCardProps) {
  const diffInfo = getDifficultyLabel(recipe.difficulty);
  const fallbackImage =
    recipe.primaryImageUrl ||
    "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?auto=format&fit=crop&w=800&q=80";

  const authorAvatar =
    recipe.author?.avatarUrl ||
    `https://ui-avatars.com/api/?name=${encodeURIComponent(
      recipe.author?.displayName || recipe.author?.userName || "Bếp Trưởng"
    )}&background=fed8c4&color=c85a17`;

  return (
    <article
      className="card"
      style={{
        display: "flex",
        flexDirection: "column",
        height: "100%",
        position: "relative",
      }}
    >
      {/* Khối Ảnh bìa công thức */}
      <Link
        href={`/recipes/${recipe.slug}`}
        style={{
          position: "relative",
          width: "100%",
          paddingTop: "62.5%", // Tỷ lệ khung hình 16:10
          overflow: "hidden",
          display: "block",
          backgroundColor: "var(--bg-surface-alt)",
        }}
      >
        <Image
          src={fallbackImage}
          alt={recipe.title}
          fill
          priority={priority}
          sizes="(max-width: 640px) 100vw, (max-width: 1024px) 50vw, 33vw"
          style={{
            objectFit: "cover",
            transition: "transform var(--transition-base)",
          }}
        />

        {/* Badges góc trên */}
        <div
          style={{
            position: "absolute",
            top: "12px",
            left: "12px",
            right: "12px",
            display: "flex",
            justifyContent: "space-between",
            pointerEvents: "none",
          }}
        >
          {recipe.categoryName && (
            <span
              className="badge badge-category"
              style={{
                boxShadow: "var(--shadow-sm)",
                backdropFilter: "blur(4px)",
              }}
            >
              {recipe.categoryName}
            </span>
          )}
          <span
            className={`badge ${diffInfo.className}`}
            style={{
              boxShadow: "var(--shadow-sm)",
              marginLeft: "auto",
            }}
          >
            {diffInfo.label}
          </span>
        </div>
      </Link>

      {/* Thân thẻ */}
      <div
        className="card-body"
        style={{
          display: "flex",
          flexDirection: "column",
          gap: "10px",
          flex: 1,
        }}
      >
        {/* Chỉ số nhanh: Thời gian & Calo */}
        <div
          style={{
            display: "flex",
            alignItems: "center",
            gap: "12px",
            fontSize: "0.82rem",
            color: "var(--text-muted)",
          }}
        >
          <span style={{ display: "inline-flex", alignItems: "center", gap: "4px" }}>
            ⏱️ {recipe.totalTime || (recipe.prepTime + recipe.cookTime) || 30} phút
          </span>
          {recipe.calories && (
            <span style={{ display: "inline-flex", alignItems: "center", gap: "4px" }}>
              🔥 {recipe.calories} kcal
            </span>
          )}
          {recipe.servings && (
            <span style={{ display: "inline-flex", alignItems: "center", gap: "4px" }}>
              👥 {recipe.servings} phần
            </span>
          )}
        </div>

        {/* Tiêu đề công thức */}
        <h3 style={{ fontSize: "1.15rem", lineHeight: 1.35, margin: 0 }}>
          <Link
            href={`/recipes/${recipe.slug}`}
            className="line-clamp-2"
            style={{
              color: "var(--text-primary)",
              transition: "color var(--transition-fast)",
            }}
          >
            {recipe.title}
          </Link>
        </h3>

        {/* Mô tả ngắn */}
        <p
          className="line-clamp-2"
          style={{
            fontSize: "0.88rem",
            color: "var(--text-secondary)",
            lineHeight: 1.5,
            margin: 0,
          }}
        >
          {recipe.description}
        </p>

        {/* Chân thẻ: Thông tin tác giả */}
        <div
          style={{
            marginTop: "auto",
            paddingTop: "12px",
            borderTop: "1px solid var(--border-subtle)",
            display: "flex",
            alignItems: "center",
            justifyContent: "space-between",
          }}
        >
          <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
            <div
              style={{
                position: "relative",
                width: "26px",
                height: "26px",
                borderRadius: "var(--radius-full)",
                overflow: "hidden",
                border: "1px solid var(--border-subtle)",
              }}
            >
              <Image
                src={authorAvatar}
                alt={recipe.author?.displayName || recipe.author?.userName || "Tác giả"}
                fill
                sizes="26px"
                unoptimized
                style={{ objectFit: "cover" }}
              />
            </div>
            <span
              style={{
                fontSize: "0.82rem",
                fontWeight: 600,
                color: "var(--text-primary)",
              }}
            >
              {recipe.author?.displayName || recipe.author?.userName || "Ẩm thực gia"}
            </span>
          </div>

          <Link
            href={`/recipes/${recipe.slug}`}
            style={{
              fontSize: "0.82rem",
              fontWeight: 600,
              color: "var(--primary)",
            }}
          >
            Xem cách làm →
          </Link>
        </div>
      </div>
    </article>
  );
}
