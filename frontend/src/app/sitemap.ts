import type { MetadataRoute } from "next";
import { getCategories } from "@/lib/api/categories";
import { getRecipes } from "@/lib/api/recipes";

/**
 * Tự động sinh XML Sitemap động cho toàn bộ hệ thống Culinary Blog (Next.js 15).
 * Truy vấn danh sách Category và Recipe từ CSDL để cập nhật chỉ mục tìm kiếm theo thời gian thực.
 */
export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  const baseUrl = process.env.NEXT_PUBLIC_SITE_URL || "http://localhost:3000";

  // 1. Các trang tĩnh cố định
  const staticRoutes: MetadataRoute.Sitemap = [
    {
      url: `${baseUrl}`,
      lastModified: new Date(),
      changeFrequency: "hourly",
      priority: 1.0,
    },
    {
      url: `${baseUrl}/categories`,
      lastModified: new Date(),
      changeFrequency: "daily",
      priority: 0.9,
    },
    {
      url: `${baseUrl}/recipes`,
      lastModified: new Date(),
      changeFrequency: "daily",
      priority: 0.9,
    },
    {
      url: `${baseUrl}/search`,
      lastModified: new Date(),
      changeFrequency: "weekly",
      priority: 0.7,
    },
  ];

  // 2. Các trang danh mục động (/categories/[slug])
  let categoryRoutes: MetadataRoute.Sitemap = [];
  try {
    const categories = await getCategories();
    if (categories && Array.isArray(categories)) {
      categoryRoutes = categories.map((cat) => ({
        url: `${baseUrl}/categories/${cat.slug}`,
        lastModified: cat.createdAt ? new Date(cat.createdAt) : new Date(),
        changeFrequency: "daily" as const,
        priority: 0.8,
      }));
    }
  } catch (err) {
    console.error("Lỗi khi sinh sitemap cho categories:", err);
  }

  // 3. Các trang công thức động (/recipes/[slug])
  let recipeRoutes: MetadataRoute.Sitemap = [];
  try {
    const recipesRes = await getRecipes({ pageSize: 100, sortBy: "newest" });
    if (recipesRes?.data && Array.isArray(recipesRes.data)) {
      recipeRoutes = recipesRes.data.map((rec) => ({
        url: `${baseUrl}/recipes/${rec.slug}`,
        lastModified: rec.publishedAt
          ? new Date(rec.publishedAt)
          : rec.createdAt
          ? new Date(rec.createdAt)
          : new Date(),
        changeFrequency: "weekly" as const,
        priority: 0.7,
      }));
    }
  } catch (err) {
    console.error("Lỗi khi sinh sitemap cho recipes:", err);
  }

  return [...staticRoutes, ...categoryRoutes, ...recipeRoutes];
}
