import React from "react";
import Link from "next/link";
import Image from "next/image";
import type { CategoryDto } from "@/types/api";
import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";

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
      className="group block h-full focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary rounded-xl"
    >
      <Card className="h-full flex flex-col items-center p-5 text-center transition-all duration-300 hover:-translate-y-1 hover:border-primary/40 hover:shadow-md bg-card">
        {/* Khung ảnh đại diện danh mục */}
        <div className="relative w-18 h-18 sm:w-20 sm:h-20 rounded-full overflow-hidden border-2 border-primary/20 bg-muted shadow-xs shrink-0 mb-3.5 transition-transform duration-300 group-hover:scale-105 group-hover:border-primary/50">
          <Image
            src={fallbackImage}
            alt={category.name}
            fill
            sizes="(max-width: 640px) 72px, 80px"
            className="object-cover transition-transform duration-500 group-hover:scale-110"
          />
        </div>

        {/* Thông tin danh mục */}
        <div className="flex flex-col items-center gap-1.5 w-full">
          <h3 className="font-semibold text-base sm:text-lg text-foreground group-hover:text-primary transition-colors line-clamp-1 leading-snug">
            {category.name}
          </h3>

          {category.description && (
            <p className="text-xs text-muted-foreground line-clamp-1 px-1">
              {category.description}
            </p>
          )}

          <Badge
            variant="secondary"
            className="mt-1 text-xs font-medium text-muted-foreground group-hover:bg-primary/10 group-hover:text-primary transition-colors"
          >
            {category.recipeCount || 0} công thức
          </Badge>
        </div>
      </Card>
    </Link>
  );
}
