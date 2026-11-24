import React from "react";
import Link from "next/link";
import { ChefHat } from "lucide-react";
import { Container } from "@/components/common/Container";

export function Footer() {
  return (
    <footer className="mt-auto w-full border-t border-border/70 bg-card/40 backdrop-blur-xs">
      <Container className="py-12 lg:py-16">
        <div className="grid grid-cols-1 gap-10 md:grid-cols-3 md:gap-8 lg:gap-12">
          {/* CỘT 1: THƯƠNG HIỆU & SỨ MỆNH */}
          <div className="space-y-4 max-w-sm">
            <Link href="/" className="inline-flex items-center gap-2.5">
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
          </div>

          {/* CỘT 2: KHÁM PHÁ CÔNG THỨC */}
          <div className="space-y-4 md:justify-self-center">
            <h4 className="font-serif text-base font-semibold text-foreground flex items-center h-9">
              Khám Phá
            </h4>
            <ul className="space-y-2.5 text-sm text-muted-foreground">
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
                  Tìm kiếm công thức
                </Link>
              </li>
              <li>
                <Link
                  href="/recipes?sortBy=totalTime&sortOrder=asc"
                  className="hover:text-primary transition-colors inline-block"
                >
                  Món nấu nhanh
                </Link>
              </li>
              <li>
                <Link
                  href="/recipes"
                  className="hover:text-primary transition-colors inline-block"
                >
                  Gợi ý thực đơn hôm nay
                </Link>
              </li>
            </ul>
          </div>

          {/* CỘT 3: DANH MỤC NỔI BẬT */}
          <div className="space-y-4 md:justify-self-end">
            <h4 className="font-serif text-base font-semibold text-foreground flex items-center h-9">
              Danh Mục Nổi Bật
            </h4>
            <ul className="space-y-2.5 text-sm text-muted-foreground">
              <li>
                <Link
                  href="/categories/mon-nuong"
                  className="hover:text-primary transition-colors inline-block"
                >
                  Món nướng
                </Link>
              </li>
              <li>
                <Link
                  href="/categories/mon-ham-ca-ri"
                  className="hover:text-primary transition-colors inline-block"
                >
                  Món hầm & Cà ri
                </Link>
              </li>
              <li>
                <Link
                  href="/categories/mon-canh"
                  className="hover:text-primary transition-colors inline-block"
                >
                  Món canh thanh mát
                </Link>
              </li>
              <li>
                <Link
                  href="/categories/mon-chay"
                  className="hover:text-primary transition-colors inline-block"
                >
                  Món chay thanh đạm
                </Link>
              </li>
              <li>
                <Link
                  href="/categories/banh-mon-an-nhe"
                  className="hover:text-primary transition-colors inline-block"
                >
                  Bánh & Món ăn nhẹ
                </Link>
              </li>
            </ul>
          </div>
        </div>
      </Container>
    </footer>
  );
}
