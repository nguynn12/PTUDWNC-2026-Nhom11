import React from "react";
import Link from "next/link";
import Image from "next/image";
import type { CategoryDto } from "@/types/api";

interface CategoryCardProps {
  category: CategoryDto;
}

export function CategoryCard({ category }: CategoryCardProps) {
  // Fallback ảnh ẩm thực đẹp mắt nếu danh mục chưa có ảnh
  const fallbackImage =
    category.imageUrl ||
    "https://images.unsplash.com/photo-1495521821757-a1efb6729352?auto=format&fit=crop&w=400&q=80";

  return (
    <Link
      href={`/categories/${category.slug}`}
      className="card"
      style={{
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        padding: "20px 16px",
        textAlign: "center",
        gap: "12px",
        height: "100%",
        textDecoration: "none",
        backgroundColor: "var(--bg-surface)",
      }}
    >
      <div
        style={{
          position: "relative",
          width: "68px",
          height: "68px",
          borderRadius: "var(--radius-full)",
          overflow: "hidden",
          border: "2px solid var(--primary-light)",
          boxShadow: "var(--shadow-sm)",
          backgroundColor: "var(--primary-light)",
          flexShrink: 0,
        }}
      >
        <Image
          src={fallbackImage}
          alt={category.name}
          fill
          sizes="68px"
          style={{ objectFit: "cover" }}
        />
      </div>

      <div style={{ display: "flex", flexDirection: "column", gap: "4px", width: "100%" }}>
        <h3
          style={{
            fontSize: "1rem",
            fontWeight: 700,
            color: "var(--text-primary)",
            lineHeight: 1.3,
          }}
          className="line-clamp-1"
        >
          {category.name}
        </h3>
        <span
          className="badge badge-neutral"
          style={{
            alignSelf: "center",
            fontSize: "0.75rem",
            padding: "2px 8px",
          }}
        >
          {category.recipeCount || 0} công thức
        </span>
      </div>
    </Link>
  );
}
