import React from 'react';
import Link from 'next/link';
import { ChefHat, Home, Compass } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';

/**
 * Trang 404 Not Found theo bộ nhận diện thương hiệu ẩm thực (Lab 6 - TV3).
 */
export default function NotFound() {
  return (
    <div className="container mx-auto px-4 py-20 flex items-center justify-center min-h-[70vh]">
      <Card className="max-w-xl w-full border-border/60 shadow-lg text-center overflow-hidden">
        <CardContent className="flex flex-col items-center p-8 md:p-12 gap-6">
          <div className="relative">
            <div className="text-6xl mb-1 animate-pulse">🍲</div>
            <div className="absolute -bottom-2 -right-2 rounded-full bg-primary/10 p-2 text-primary border border-primary/20">
              <ChefHat className="w-5 h-5" aria-hidden="true" />
            </div>
          </div>

          <div className="space-y-3">
            <span className="inline-block px-3 py-1 rounded-full text-xs font-semibold tracking-wider uppercase bg-muted text-muted-foreground border border-border/50">
              Lỗi 404 — Không tìm thấy trang
            </span>
            <h1 className="text-2xl md:text-3xl font-extrabold tracking-tight text-foreground">
              Món Ăn Này Chưa Sẵn Sàng!
            </h1>
            <p className="text-sm md:text-base text-muted-foreground leading-relaxed max-w-md mx-auto">
              Đường dẫn công thức hoặc danh mục bạn đang tìm kiếm có thể đã được đổi tên, chuyển đi hoặc không tồn tại trong hệ thống của chúng tôi.
            </p>
          </div>

          <div className="flex flex-wrap items-center justify-center gap-4 pt-2">
            <Button asChild variant="default" className="gap-2 font-medium shadow-sm">
              <Link href="/">
                <Home className="w-4 h-4" aria-hidden="true" />
                Về trang chủ
              </Link>
            </Button>
            <Button asChild variant="outline" className="gap-2 font-medium border-border/80 hover:bg-muted/50">
              <Link href="/recipes">
                <Compass className="w-4 h-4" aria-hidden="true" />
                Khám phá công thức
              </Link>
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
