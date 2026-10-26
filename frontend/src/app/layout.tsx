import type { Metadata, Viewport } from "next";
import type { ReactNode } from "react";
import "./globals.css";
import { Providers } from "./providers";
import { Navbar } from "@/components/layout/Navbar";
import { Footer } from "@/components/layout/Footer";
import { Plus_Jakarta_Sans, Lora } from "next/font/google";
import { cn } from "@/lib/utils";

const plusJakartaSans = Plus_Jakarta_Sans({
  subsets: ["vietnamese", "latin"],
  variable: "--font-sans",
  weight: ["400", "500", "600", "700", "800"],
  display: "swap",
});

const lora = Lora({
  subsets: ["vietnamese", "latin"],
  variable: "--font-serif",
  weight: ["500", "600", "700"],
  display: "swap",
});

export const viewport: Viewport = {
  width: "device-width",
  initialScale: 1,
  maximumScale: 5,
  themeColor: "#c85a17",
};

export const metadata: Metadata = {
  title: {
    default: "Culinary Blog — Khám Phá & Chia Sẻ Công Thức Nấu Ăn",
    template: "%s | Culinary Blog",
  },
  description:
    "Nền tảng blog ẩm thực cao cấp: tra cứu hàng ngàn công thức nấu ăn chuẩn vị, thực đơn theo danh mục và thông tin dinh dưỡng trực quan.",
  keywords: [
    "công thức nấu ăn",
    "món ngon mỗi ngày",
    "ẩm thực việt nam",
    "culinary blog",
    "hướng dẫn nấu ăn",
    "dinh dưỡng món ăn",
  ],
  authors: [{ name: "Trần Quốc Quân", url: "https://github.com/nguynn12/PTUDWNC-2026-Nhom11" }],
  creator: "Trần Quốc Quân (Nhóm 11 - PTUDWNC)",
  metadataBase: new URL("http://localhost:3000"),
  openGraph: {
    title: "Culinary Blog — Khám Phá & Chia Sẻ Công Thức Nấu Ăn",
    description:
      "Nền tảng blog ẩm thực cao cấp: tra cứu hàng ngàn công thức nấu ăn chuẩn vị, thực đơn theo danh mục và thông tin dinh dưỡng trực quan.",
    url: "http://localhost:3000",
    siteName: "Culinary Blog",
    locale: "vi_VN",
    type: "website",
  },
  robots: {
    index: true,
    follow: true,
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: ReactNode;
}>) {
  return (
    <html lang="vi" suppressHydrationWarning className={cn("font-sans", plusJakartaSans.variable, lora.variable)}>
      <body className="font-sans antialiased" suppressHydrationWarning>
        <Providers>
          <Navbar />
          <main id="main-content">{children}</main>
          <Footer />
        </Providers>
      </body>
    </html>
  );
}
