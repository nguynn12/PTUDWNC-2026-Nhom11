"use client";

import React, { useState, Suspense } from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  Search,
  Menu,
  Home,
  BookOpen,
  Layers,
  ShieldCheck,
  ChefHat,
} from "lucide-react";
import { Container } from "@/components/common/Container";
import {
  Sheet,
  SheetTrigger,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetDescription,
} from "@/components/ui/sheet";
import { UserMenu } from "@/components/layout/UserMenu";
import { HeaderSearch } from "@/components/search/HeaderSearch";
import { cn } from "@/lib/utils";

interface NavLinkItem {
  href: string;
  label: string;
  icon: React.ComponentType<{ className?: string }>;
}

const navLinks: NavLinkItem[] = [
  { href: "/", label: "Trang chủ", icon: Home },
  { href: "/recipes", label: "Khám phá công thức", icon: BookOpen },
  { href: "/categories", label: "Danh mục", icon: Layers },
  { href: "/search", label: "Tìm kiếm", icon: Search },
  { href: "/dashboard/categories", label: "Quản trị Admin", icon: ShieldCheck },
];

export function Navbar() {
  const pathname = usePathname();
  const [isMobileOpen, setIsMobileOpen] = useState(false);

  const isActive = (href: string) => {
    if (href === "/") return pathname === "/";
    return pathname.startsWith(href);
  };

  return (
    <header className="sticky top-0 z-50 w-full border-b border-border/60 bg-background/90 backdrop-blur-md transition-all">
      <Container className="flex h-18 items-center justify-between gap-4">
        {/* LOGO THƯƠNG HIỆU */}
        <Link
          href="/"
          className="flex items-center gap-2.5 transition-transform hover:scale-[1.02] shrink-0"
        >
          <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-primary text-primary-foreground shadow-md shadow-primary/25">
            <ChefHat className="h-5 w-5" />
          </div>
          <div>
            <span className="block font-serif text-xl font-bold tracking-tight text-foreground leading-none">
              Culinary<span className="text-primary">Blog</span>
            </span>
            <span className="block text-[0.65rem] font-semibold uppercase tracking-wider text-muted-foreground mt-0.5">
              Nghệ thuật ẩm thực
            </span>
          </div>
        </Link>

        {/* MENU ĐIỀU HƯỚNG MÁY TÍNH (DESKTOP) */}
        <nav className="hidden lg:flex items-center gap-1">
          {navLinks.map((link) => {
            const active = isActive(link.href);
            return (
              <Link
                key={link.href}
                href={link.href}
                className={cn(
                  "px-3.5 py-2 text-sm font-medium rounded-lg transition-colors",
                  active
                    ? "bg-primary/10 text-primary font-semibold"
                    : "text-muted-foreground hover:bg-muted hover:text-foreground"
                )}
              >
                {link.label}
              </Link>
            );
          })}
        </nav>

        {/* CỤM CHỨC NĂNG BÊN PHẢI (QUICK SEARCH + USER MENU + MOBILE MENU) */}
        <div className="flex items-center gap-2 sm:gap-3">
          {/* Ô TÌM KIẾM NHANH TRÊN DESKTOP */}
          <div className="hidden md:flex relative items-center w-48 xl:w-64">
            <Suspense fallback={<div className="w-48 xl:w-64 h-9 bg-muted/50 rounded-full animate-pulse" />}>
              <HeaderSearch className="w-48 xl:w-64" />
            </Suspense>
          </div>

          {/* TÀI KHOẢN NGƯỜI DÙNG */}
          <div className="shrink-0">
            <UserMenu />
          </div>

          {/* MENU DI ĐỘNG BẰNG SHADCN SHEET */}
          <Sheet open={isMobileOpen} onOpenChange={setIsMobileOpen}>
            <SheetTrigger
              className="inline-flex lg:hidden items-center justify-center h-9 w-9 rounded-lg border border-input bg-background shadow-xs hover:bg-accent hover:text-accent-foreground transition-colors cursor-pointer shrink-0"
              aria-label="Mở menu di động"
            >
              <Menu className="h-5 w-5" />
            </SheetTrigger>

            <SheetContent side="right" className="w-[300px] sm:w-[360px] p-0 flex flex-col">
              <SheetHeader className="p-5 border-b border-border/60 text-left">
                <div className="flex items-center gap-2.5">
                  <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary text-primary-foreground shadow-sm">
                    <ChefHat className="h-4 w-4" />
                  </div>
                  <div>
                    <SheetTitle className="font-serif text-lg font-bold">
                      Culinary<span className="text-primary">Blog</span>
                    </SheetTitle>
                    <SheetDescription className="text-xs">
                      Khám phá & sáng tạo món ngon mỗi ngày
                    </SheetDescription>
                  </div>
                </div>
              </SheetHeader>

              {/* Ô TÌM KIẾM NHANH TRONG MOBILE SHEET */}
              <div className="p-4 border-b border-border/40">
                <Suspense fallback={<div className="w-full h-9 bg-muted/50 rounded-full animate-pulse" />}>
                  <HeaderSearch
                    className="w-full"
                    inputClassName="h-10 rounded-lg text-sm bg-muted/60"
                    placeholder="Tìm món ngon..."
                    onSearchSubmit={() => setIsMobileOpen(false)}
                    onSearchComplete={() => setIsMobileOpen(false)}
                  />
                </Suspense>
              </div>

              {/* DANH SÁCH LIÊN KẾT TRONG MOBILE SHEET */}
              <nav className="flex-1 overflow-y-auto p-3 space-y-1">
                {navLinks.map((link) => {
                  const Icon = link.icon;
                  const active = isActive(link.href);
                  return (
                    <Link
                      key={link.href}
                      href={link.href}
                      onClick={() => setIsMobileOpen(false)}
                      className={cn(
                        "flex items-center gap-3 px-3.5 py-2.5 rounded-lg text-sm font-medium transition-colors",
                        active
                          ? "bg-primary text-primary-foreground font-semibold shadow-sm"
                          : "text-muted-foreground hover:bg-muted hover:text-foreground"
                      )}
                    >
                      <Icon className="h-4 w-4 shrink-0" />
                      <span>{link.label}</span>
                    </Link>
                  );
                })}
              </nav>

              {/* FOOTER CỦA SHEET */}
              <div className="p-4 border-t border-border/40 text-center text-xs text-muted-foreground">
                <p>Culinary Blog — Phiên bản 1.0.0</p>
                <p className="mt-0.5 text-[0.7rem] text-muted-foreground/80">PTUDWNC 2026 - Nhóm 11</p>
              </div>
            </SheetContent>
          </Sheet>
        </div>
      </Container>
    </header>
  );
}
