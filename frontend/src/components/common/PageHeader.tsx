import React from "react";
import Link from "next/link";
import { ChevronRight, Home } from "lucide-react";
import { cn } from "@/lib/utils";
import { Badge } from "@/components/ui/badge";

export interface BreadcrumbItem {
  label: string;
  href?: string;
}

export interface PageHeaderProps {
  title: string;
  description?: string;
  eyebrow?: string;
  breadcrumbs?: BreadcrumbItem[];
  children?: React.ReactNode;
  align?: "left" | "center";
  className?: string;
}

export function PageHeader({
  title,
  description,
  eyebrow,
  breadcrumbs,
  children,
  align = "left",
  className,
}: PageHeaderProps) {
  const isCentered = align === "center";

  return (
    <div
      className={cn(
        "py-6 sm:py-8 lg:py-10 border-b border-border/40 mb-8",
        isCentered && "text-center",
        className
      )}
    >
      {/* BREADCRUMBS */}
      {breadcrumbs && breadcrumbs.length > 0 && (
        <nav
          aria-label="Breadcrumb"
          className={cn(
            "flex items-center gap-1.5 text-xs sm:text-sm text-muted-foreground mb-4",
            isCentered && "justify-center"
          )}
        >
          <Link
            href="/"
            className="flex items-center gap-1 hover:text-foreground transition-colors"
          >
            <Home className="h-3.5 w-3.5" />
            <span className="sr-only">Trang chủ</span>
          </Link>

          {breadcrumbs.map((crumb, index) => {
            const isLast = index === breadcrumbs.length - 1;
            return (
              <React.Fragment key={crumb.label + index}>
                <ChevronRight className="h-3.5 w-3.5 text-muted-foreground/60 shrink-0" />
                {crumb.href && !isLast ? (
                  <Link
                    href={crumb.href}
                    className="hover:text-foreground transition-colors truncate max-w-[180px] sm:max-w-none"
                  >
                    {crumb.label}
                  </Link>
                ) : (
                  <span
                    aria-current={isLast ? "page" : undefined}
                    className={cn(
                      "truncate max-w-[200px] sm:max-w-none",
                      isLast
                        ? "font-medium text-foreground"
                        : "text-muted-foreground"
                    )}
                  >
                    {crumb.label}
                  </span>
                )}
              </React.Fragment>
            );
          })}
        </nav>
      )}

      {/* HEADER CONTENT & ACTIONS */}
      <div
        className={cn(
          "flex flex-col gap-4",
          !isCentered && "sm:flex-row sm:items-end sm:justify-between"
        )}
      >
        <div className={cn("space-y-2", isCentered ? "mx-auto max-w-2xl" : "max-w-3xl")}>
          {eyebrow && (
            <div>
              <Badge variant="secondary" className="mb-2 font-medium">
                {eyebrow}
              </Badge>
            </div>
          )}

          <h1 className="text-3xl sm:text-4xl lg:text-5xl font-bold tracking-tight text-foreground font-serif">
            {title}
          </h1>

          {description && (
            <p className="text-base sm:text-lg text-muted-foreground leading-relaxed">
              {description}
            </p>
          )}
        </div>

        {/* ACTIONS SLOT */}
        {children && (
          <div
            className={cn(
              "flex flex-wrap items-center gap-2 pt-2 sm:pt-0 shrink-0",
              isCentered && "justify-center"
            )}
          >
            {children}
          </div>
        )}
      </div>
    </div>
  );
}
