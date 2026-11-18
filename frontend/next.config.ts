import type { NextConfig } from "next";

const backendUrl = process.env.BACKEND_API_URL || "http://localhost:5000";

const nextConfig: NextConfig = {
  devIndicators: false,
  reactStrictMode: true,
  turbopack: {
    root: process.cwd(),
  },
  images: {
    // Biến thể WebP đã được tối ưu ở backend; browser dùng PublicUrl trực tiếp.
    // Tránh optimizer trong container truy cập localhost của chính container web.
    unoptimized: true,
    remotePatterns: [
      {
        protocol: "https",
        hostname: "www.savourydays.com",
        pathname: "/wp-content/uploads/**",
      },
      {
        protocol: "https",
        hostname: "helenrecipes.com",
        pathname: "/wp-content/uploads/**",
      },
      {
        protocol: "http",
        hostname: "localhost",
        pathname: "/**",
      },
      {
        protocol: "http",
        hostname: "127.0.0.1",
        pathname: "/**",
      },
      {
        protocol: "http",
        hostname: "minio",
        pathname: "/**",
      },
      {
        protocol: "http",
        hostname: "localhost",
        port: "9000",
        pathname: "/**",
      },
      {
        protocol: "http",
        hostname: "127.0.0.1",
        port: "9000",
        pathname: "/**",
      },
      {
        protocol: "https",
        hostname: "picsum.photos",
        pathname: "/**",
      },
      {
        protocol: "https",
        hostname: "monngonmoingay.com",
        pathname: "/wp-content/uploads/**",
      },
      {
        protocol: "https",
        hostname: "www.recipetineats.com",
        pathname: "/tachyon/**",
      },
      {
        protocol: "https",
        hostname: "www.allrecipes.com",
        pathname: "/thmb/**",
      },
      {
        protocol: "https",
        hostname: "images.immediate.co.uk",
        pathname: "/production/**",
      },
      {
        protocol: "https",
        hostname: "fastly.picsum.photos",
        pathname: "/**",
      },
      {
        protocol: "https",
        hostname: "images.unsplash.com",
        pathname: "/**",
      },
      {
        protocol: "https",
        hostname: "plus.unsplash.com",
        pathname: "/**",
      },
      {
        protocol: "https",
        hostname: "ui-avatars.com",
        pathname: "/**",
      },
      {
        protocol: "https",
        hostname: "lh3.googleusercontent.com",
        pathname: "/**",
      },
      {
        protocol: "https",
        hostname: "avatars.githubusercontent.com",
        pathname: "/**",
      },
      {
        protocol: "https",
        hostname: "via.placeholder.com",
        pathname: "/**",
      },
      {
        protocol: "https",
        hostname: "res.cloudinary.com",
        pathname: "/**",
      },
      {
        protocol: "https",
        hostname: "cloudflare-ipfs.com",
        pathname: "/**",
      },
      {
        protocol: "https",
        hostname: "storage.culinaryblog.com",
        pathname: "/**",
      },
      {
        protocol: "https",
        hostname: "loremflickr.com",
        pathname: "/**",
      },
      {
        protocol: "https",
        hostname: "cdn.jsdelivr.net",
        pathname: "/**",
      },
    ],
  },
  async rewrites() {
    return [
      {
        source: "/api/v1/:path*",
        destination: `${backendUrl}/api/v1/:path*`,
      },
      {
        source: "/uploads/:path*",
        destination: `${backendUrl}/uploads/:path*`,
      },
    ];
  },
};

export default nextConfig;
