import type { MetadataRoute } from "next";

/**
 * Cấu hình tệp điều hướng bot tìm kiếm (robots.txt) tự động cho Culinary Blog (Next.js 15).
 * Cho phép lập chỉ mục nội dung ẩm thực công khai và bảo vệ các đường dẫn nội bộ/quản trị.
 */
export default function robots(): MetadataRoute.Robots {
  const baseUrl = process.env.NEXT_PUBLIC_SITE_URL || "http://localhost:3000";

  return {
    rules: [
      {
        userAgent: "*",
        allow: "/",
        disallow: [
          "/dashboard/",
          "/admin/",
          "/api/",
          "/auth/",
          "/settings/",
        ],
      },
    ],
    sitemap: `${baseUrl}/sitemap.xml`,
  };
}
