"use client";

import React from "react";
import Link from "next/link";
import type { CategoryDto } from "@/types/api";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { cn } from "@/lib/utils";
import {
  UtensilsCrossed,
  ArrowRight,
  ChevronRight,
  Flame,
  Layers,
  Sparkles,
} from "lucide-react";

export interface CategorySidebarProps {
  categories: CategoryDto[];
  activeSlug?: string;
  className?: string;
  title?: string;
  showAllLink?: boolean;
}

export function CategorySidebar({
  categories,
  activeSlug,
  className,
  title = "Danh Mục Món Ăn",
  showAllLink = true,
}: CategorySidebarProps) {
  // Sắp xếp danh mục theo orderIndex tăng dần
  const sortedCategories = [...categories].sort(
    (a, b) => (a.orderIndex ?? 0) - (b.orderIndex ?? 0)
  );

  return (
    <aside
      aria-label={title}
      className={cn("w-full transition-all duration-200", className)}
    >
      {/* =====================================================================
          1. GIAO DIỆN DESKTOP (lg >= 1024px): CỘT DỌC CỐ ĐỊNH KHI CUỘN
          ===================================================================== */}
      <div className="hidden lg:block sticky top-24 max-h-[calc(100vh-120px)] flex flex-col">
        <Card className="p-4 shadow-xs border-border/70 bg-card flex flex-col h-full overflow-hidden">
          {/* Header của Sidebar */}
          <div className="flex items-center justify-between pb-3.5 border-b border-border/60 shrink-0">
            <div className="flex items-center gap-2.5">
              <div className="w-8 h-8 rounded-lg bg-primary/10 text-primary flex items-center justify-center shrink-0">
                <UtensilsCrossed className="w-4 h-4" />
              </div>
              <div>
                <h3 className="font-semibold text-base text-foreground tracking-tight leading-none">
                  {title}
                </h3>
                <p className="text-xs text-muted-foreground mt-1">
                  Khám phá theo chủ đề
                </p>
              </div>
            </div>
            <Badge
              variant="secondary"
              className="text-xs font-semibold px-2 py-0.5 rounded-full"
            >
              {categories.length}
            </Badge>
          </div>

          {/* Danh sách danh mục cuộn mượt */}
          <div className="overflow-y-auto pr-1 py-2 space-y-1 scrollbar-thin scrollbar-thumb-muted-foreground/20 hover:scrollbar-thumb-muted-foreground/40 flex-1">
            {sortedCategories.map((cat) => {
              const isActive = activeSlug === cat.slug;
              return (
                <Link
                  key={cat.id || cat.slug}
                  href={`/categories/${cat.slug}`}
                  className={cn(
                    "group flex items-center justify-between px-3 py-2.5 rounded-lg text-sm transition-all duration-200",
                    isActive
                      ? "bg-primary text-primary-foreground font-semibold shadow-xs"
                      : "text-foreground/85 hover:bg-muted/70 hover:text-primary"
                  )}
                >
                  <div className="flex items-center gap-2.5 min-w-0 pr-2">
                    <span
                      className={cn(
                        "w-2 h-2 rounded-full shrink-0 transition-all",
                        isActive
                          ? "bg-white scale-125"
                          : "bg-primary/40 group-hover:bg-primary group-hover:scale-125"
                      )}
                    />
                    <span className="truncate">{cat.name}</span>
                  </div>

                  <div className="flex items-center gap-1 shrink-0">
                    <span
                      className={cn(
                        "text-xs px-2 py-0.5 rounded-full font-medium transition-colors",
                        isActive
                          ? "bg-white/20 text-white"
                          : "bg-muted text-muted-foreground group-hover:bg-primary/10 group-hover:text-primary"
                      )}
                    >
                      {cat.recipeCount ?? 0}
                    </span>
                    <ChevronRight
                      className={cn(
                        "w-3.5 h-3.5 transition-transform group-hover:translate-x-0.5",
                        isActive
                          ? "text-white"
                          : "text-muted-foreground/50 group-hover:text-primary"
                      )}
                    />
                  </div>
                </Link>
              );
            })}
          </div>

          {/* Footer của Sidebar */}
          {showAllLink && (
            <div className="pt-3 border-t border-border/60 shrink-0">
              <Button
                asChild
                variant="ghost"
                size="sm"
                className="w-full justify-between text-xs font-semibold text-primary hover:text-primary hover:bg-primary/10 transition-colors"
              >
                <Link href="/categories">
                  <span className="flex items-center gap-1.5">
                    <Layers className="w-3.5 h-3.5" />
                    Xem tất cả danh mục
                  </span>
                  <ArrowRight className="w-3.5 h-3.5" />
                </Link>
              </Button>
            </div>
          )}
        </Card>
      </div>

      {/* =====================================================================
          2. GIAO DIỆN MOBILE & TABLET (< 1024px): THANH CUỘN NGANG DẠNG CHIPS
          ===================================================================== */}
      <div className="block lg:hidden mb-6">
        <div className="flex items-center justify-between mb-2.5 px-0.5">
          <div className="flex items-center gap-1.5 text-xs font-semibold uppercase tracking-wider text-primary">
            <Sparkles className="w-3.5 h-3.5" />
            <span>Chủ đề ẩm thực</span>
          </div>
          {showAllLink && (
            <Link
              href="/categories"
              className="text-xs font-medium text-muted-foreground hover:text-primary inline-flex items-center gap-1"
            >
              Tất cả ({categories.length}) <ChevronRight className="w-3 h-3" />
            </Link>
          )}
        </div>

        {/* Thanh cuộn ngang các danh mục (Horizontal Scroll Chips) */}
        <div className="flex items-center gap-2 overflow-x-auto pb-2 scrollbar-none -mx-4 px-4 sm:mx-0 sm:px-0">
          <Link
            href="/"
            className={cn(
              "shrink-0 inline-flex items-center gap-1.5 px-3 py-1.5 rounded-full text-xs font-medium border transition-all duration-200",
              !activeSlug
                ? "bg-primary text-primary-foreground border-primary shadow-xs"
                : "bg-card text-foreground/80 border-border/70 hover:border-primary/40 hover:bg-muted"
            )}
          >
            <Flame className="w-3 h-3" />
            <span>Mới nhất</span>
          </Link>

          {sortedCategories.map((cat) => {
            const isActive = activeSlug === cat.slug;
            return (
              <Link
                key={cat.id || cat.slug}
                href={`/categories/${cat.slug}`}
                className={cn(
                  "shrink-0 inline-flex items-center gap-1.5 px-3 py-1.5 rounded-full text-xs font-medium border transition-all duration-200",
                  isActive
                    ? "bg-primary text-primary-foreground border-primary shadow-xs"
                    : "bg-card text-foreground/80 border-border/70 hover:border-primary/40 hover:bg-muted"
                )}
              >
                <span>{cat.name}</span>
                <span
                  className={cn(
                    "text-[10px] px-1.5 py-0.2 rounded-full",
                    isActive
                      ? "bg-white/20 text-white"
                      : "bg-muted text-muted-foreground"
                  )}
                >
                  {cat.recipeCount ?? 0}
                </span>
              </Link>
            );
          })}
        </div>
      </div>
    </aside>
  );
}
