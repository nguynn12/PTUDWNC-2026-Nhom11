import type { MetadataRoute } from "next";

// Lấy cùng tập URL do backend sinh; không duy trì thêm một truy vấn phân trang riêng.
export const dynamic = "force-dynamic";

export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  const backend = process.env.BACKEND_API_URL || "http://localhost:5000";
  const response = await fetch(`${backend}/sitemap.xml`, { cache: "no-store" });
  if (!response.ok) throw new Error(`Không thể tải sitemap: HTTP ${response.status}`);
  const xml = await response.text();
  const baseUrl = process.env.NEXT_PUBLIC_SITE_URL || "http://localhost:3000";
  const decode = (value: string) => value.replace(/&amp;/g, "&").replace(/&lt;/g, "<")
    .replace(/&gt;/g, ">").replace(/&quot;/g, '"').replace(/&apos;/g, "'");
  return [...xml.matchAll(/<url>([\s\S]*?)<\/url>/g)].map(([, block]) => {
    const location = block.match(/<loc>(.*?)<\/loc>/)?.[1];
    if (!location) throw new Error("Sitemap thiếu URL hợp lệ.");
    const source = new URL(decode(location));
    const modified = block.match(/<lastmod>(.*?)<\/lastmod>/)?.[1];
    return {
      url: new URL(source.pathname + source.search, baseUrl).toString(),
      ...(modified ? { lastModified: new Date(modified) } : {}),
    };
  });
}
