import React from "react";
import Link from "next/link";
import { ChefHat, ExternalLink, Heart, Code2 } from "lucide-react";
import { Container } from "@/components/common/Container";

export function Footer() {
  const currentYear = 2026;

  return (
    <footer className="mt-auto w-full border-t border-border/70 bg-card/40 backdrop-blur-xs">
      <Container className="py-12 lg:py-16">
        <div className="grid grid-cols-1 gap-8 md:grid-cols-2 lg:grid-cols-4 lg:gap-12">
          {/* CỘT 1: THƯƠNG HIỆU & SỨ MỆNH */}
          <div className="space-y-4">
            <Link href="/" className="flex items-center gap-2.5">
              <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-primary text-primary-foreground shadow-sm">
                <ChefHat className="h-5 w-5" />
              </div>
              <span className="font-serif text-xl font-bold tracking-tight text-foreground">
                Culinary<span className="text-primary">Blog</span>
              </span>
            </Link>

            <p className="text-sm text-muted-foreground leading-relaxed">
              Không gian khám phá và sẻ chia nghệ thuật ẩm thực. Nơi hội tụ các
              công thức nấu ăn chuẩn vị, định lượng khoa học và truyền cảm hứng
              vào bếp mỗi ngày.
            </p>

            <div className="flex items-center gap-3 pt-1 text-muted-foreground">
              <a
                href="https://github.com/nguynn12/PTUDWNC-2026-Nhom11"
                target="_blank"
                rel="noreferrer"
                className="inline-flex items-center gap-1.5 text-xs hover:text-foreground transition-colors"
                aria-label="GitHub Repository dự án"
              >
                <Code2 className="h-4 w-4" />
                <span>Mã nguồn dự án</span>
              </a>
            </div>
          </div>

          {/* CỘT 2: KHÁM PHÁ CÔNG THỨC */}
          <div className="space-y-3">
            <h4 className="font-serif text-base font-semibold text-foreground">
              Khám Phá
            </h4>
            <ul className="space-y-2 text-sm text-muted-foreground">
              <li>
                <Link
                  href="/recipes"
                  className="hover:text-primary transition-colors inline-block"
                >
                  Công thức nấu ăn mới nhất
                </Link>
              </li>
              <li>
                <Link
                  href="/categories"
                  className="hover:text-primary transition-colors inline-block"
                >
                  Danh mục món ăn phong phú
                </Link>
              </li>
              <li>
                <Link
                  href="/search"
                  className="hover:text-primary transition-colors inline-block"
                >
                  Tìm kiếm toàn văn bản (FTS)
                </Link>
              </li>
              <li>
                <Link
                  href="/categories/mon-chinh"
                  className="hover:text-primary transition-colors inline-block"
                >
                  Món chính gia đình
                </Link>
              </li>
              <li>
                <Link
                  href="/categories/an-chay-thuc-duong"
                  className="hover:text-primary transition-colors inline-block"
                >
                  Ăn chay & thực dưỡng thanh tịnh
                </Link>
              </li>
            </ul>
          </div>

          {/* CỘT 3: HỆ THỐNG & QUẢN TRỊ */}
          <div className="space-y-3">
            <h4 className="font-serif text-base font-semibold text-foreground">
              Quản Trị & Hệ Thống
            </h4>
            <ul className="space-y-2 text-sm text-muted-foreground">
              <li>
                <Link
                  href="/dashboard/categories"
                  className="hover:text-primary transition-colors inline-block"
                >
                  Quản trị Danh mục (Admin)
                </Link>
              </li>
              <li>
                <Link
                  href="/dashboard"
                  className="hover:text-primary transition-colors inline-block"
                >
                  Bảng điều khiển tác giả
                </Link>
              </li>
              <li>
                <a
                  href="http://localhost:5000/scalar/v1"
                  target="_blank"
                  rel="noreferrer"
                  className="inline-flex items-center gap-1.5 text-primary hover:underline font-medium"
                >
                  <span>Scalar API Reference</span>
                  <ExternalLink className="h-3.5 w-3.5" />
                </a>
              </li>
              <li>
                <a
                  href="http://localhost:5000/health"
                  target="_blank"
                  rel="noreferrer"
                  className="inline-flex items-center gap-1.5 text-secondary hover:underline font-medium"
                >
                  <span>Kiểm tra Health Checks</span>
                  <ExternalLink className="h-3.5 w-3.5" />
                </a>
              </li>
            </ul>
          </div>

          {/* CỘT 4: THÔNG TIN DỰ ÁN */}
          <div className="space-y-3">
            <h4 className="font-serif text-base font-semibold text-foreground">
              Công Nghệ & Đồ Án
            </h4>
            <p className="text-sm text-muted-foreground leading-relaxed">
              Dự án môn học{" "}
              <strong className="text-foreground font-medium">
                Phát triển Ứng dụng Web Nâng cao
              </strong>{" "}
              — Học kỳ I, Năm học 2026.
            </p>
            <div className="rounded-lg bg-muted/60 p-3 text-xs text-muted-foreground space-y-1 border border-border/50">
              <p>
                <span className="font-semibold text-foreground">Kiến trúc:</span>{" "}
                Clean Architecture + Next.js App Router
              </p>
              <p>
                <span className="font-semibold text-foreground">Giao diện:</span>{" "}
                Tailwind CSS v4 + shadcn/ui
              </p>
              <p>
                <span className="font-semibold text-foreground">Nhóm thực hiện:</span>{" "}
                Nhóm 11
              </p>
            </div>
          </div>
        </div>

        {/* THANH BẢN QUYỀN CUỐI TRANG */}
        <div className="mt-12 pt-6 border-t border-border/60 flex flex-col sm:flex-row items-center justify-between gap-4 text-xs text-muted-foreground">
          <p>© {currentYear} Culinary Blog. Bản quyền thuộc về Nhóm 11 - PTUDWNC.</p>

          <p className="flex items-center gap-1">
            <span>Thiết kế và phát triển với</span>
            <Heart className="h-3.5 w-3.5 text-red-500 fill-red-500" />
            <span>chuẩn WCAG 2.1 AA</span>
          </p>
        </div>
      </Container>
    </footer>
  );
}
