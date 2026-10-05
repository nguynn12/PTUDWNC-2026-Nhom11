import React from "react";
import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { getRecipeBySlug } from "@/lib/api/recipes";
import { RecipeDetail } from "@/components/recipe/RecipeDetail";
import type { RecipeDetailDto } from "@/types/api";

// ISR 300 giây (5 phút) theo đặc tả SRS Mục 5.1 (Dòng 1780)
export const revalidate = 300;

interface PageProps {
  params: Promise<{ slug: string }>;
}

export async function generateMetadata({ params }: PageProps): Promise<Metadata> {
  const { slug } = await params;

  try {
    const recipe = await getRecipeBySlug(slug);
    if (!recipe) return { title: "Chi Tiết Công Thức" };

    const heroImg =
      recipe.primaryImageUrl ||
      recipe.images?.find((img) => img.isPrimary)?.imageUrl ||
      recipe.images?.[0]?.imageUrl ||
      "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?auto=format&fit=crop&w=1200&q=80";

    return {
      title: `${recipe.title} — Công Thức Nấu Ăn`,
      description:
        recipe.description ||
        `Hướng dẫn cách nấu món ${recipe.title} thơm ngon, chuẩn vị cùng thông tin dinh dưỡng chi tiết tại Culinary Blog.`,
      openGraph: {
        title: `${recipe.title} — Công Thức Nấu Ăn | Culinary Blog`,
        description: recipe.description,
        images: [{ url: heroImg, alt: recipe.title }],
      },
      twitter: {
        card: "summary_large_image",
        title: recipe.title,
        description: recipe.description,
        images: [heroImg],
      },
    };
  } catch {
    return { title: "Chi Tiết Công Thức Nấu Ăn" };
  }
}

export default async function RecipeDetailPage({ params }: PageProps) {
  const { slug } = await params;
  let recipe: RecipeDetailDto | null = null;

  try {
    recipe = await getRecipeBySlug(slug);
  } catch {
    recipe = null;
  }

  // Nếu bài viết không tồn tại trong database hoặc trả về 404
  if (!recipe) {
    notFound();
  }

  return <RecipeDetail recipe={recipe} />;
}
