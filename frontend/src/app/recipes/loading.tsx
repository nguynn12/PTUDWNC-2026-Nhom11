import React from "react";
import { Skeleton } from "@/components/ui/skeleton";

export default function RecipesLoading() {
  return (
    <div className="container mx-auto px-4 py-8 md:py-12 max-w-7xl animate-pulse">
      {/* Breadcrumb Skeleton */}
      <div className="flex items-center gap-2 mb-6">
        <Skeleton className="h-4 w-16 rounded" />
        <span className="text-[var(--text-muted)]">/</span>
        <Skeleton className="h-4 w-28 rounded" />
      </div>

      {/* Header Skeleton */}
      <div className="mb-8 space-y-3">
        <Skeleton className="h-4 w-40 rounded" />
        <Skeleton className="h-10 w-80 max-w-full rounded-xl" />
        <Skeleton className="h-5 w-full max-w-xl rounded" />
      </div>

      {/* Layout 2 cột: Sidebar Skeleton + Cards Skeleton */}
      <div className="flex flex-col lg:flex-row gap-8 items-start">
        {/* Sidebar Skeleton */}
        <div className="hidden lg:block w-72 shrink-0 bg-[var(--bg-surface)] border border-[var(--border-subtle)] rounded-2xl p-6 space-y-6">
          <Skeleton className="h-6 w-32 rounded" />
          <div className="space-y-2">
            <Skeleton className="h-4 w-20 rounded" />
            <Skeleton className="h-10 w-full rounded-xl" />
          </div>
          <div className="space-y-2">
            <Skeleton className="h-4 w-24 rounded" />
            <div className="grid grid-cols-3 gap-2">
              <Skeleton className="h-8 rounded-lg" />
              <Skeleton className="h-8 rounded-lg" />
              <Skeleton className="h-8 rounded-lg" />
            </div>
          </div>
          <div className="space-y-2">
            <Skeleton className="h-4 w-28 rounded" />
            <Skeleton className="h-10 w-full rounded-xl" />
          </div>
          <Skeleton className="h-10 w-full rounded-xl" />
        </div>

        {/* Content Skeleton */}
        <div className="flex-1 w-full space-y-6">
          {/* Toolbar Skeleton */}
          <div className="h-14 bg-[var(--bg-surface)] border border-[var(--border-subtle)] rounded-2xl px-5 flex items-center justify-between">
            <Skeleton className="h-5 w-36 rounded" />
            <Skeleton className="h-9 w-40 rounded-xl" />
          </div>

          {/* Grid Skeleton */}
          <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-6">
            {Array.from({ length: 6 }).map((_, i) => (
              <div
                key={i}
                className="rounded-2xl border border-[var(--border-subtle)] bg-[var(--bg-card)] overflow-hidden flex flex-col"
              >
                <Skeleton className="w-full pt-[62.5%]" />
                <div className="p-5 space-y-3 flex-1 flex flex-col">
                  <div className="flex gap-3">
                    <Skeleton className="h-4 w-16 rounded" />
                    <Skeleton className="h-4 w-20 rounded" />
                  </div>
                  <Skeleton className="h-6 w-5/6 rounded" />
                  <Skeleton className="h-4 w-full rounded" />
                  <Skeleton className="h-4 w-2/3 rounded" />
                  <div className="mt-auto pt-3 border-t border-[var(--border-subtle)] flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      <Skeleton className="w-7 h-7 rounded-full" />
                      <Skeleton className="h-4 w-24 rounded" />
                    </div>
                    <Skeleton className="h-4 w-16 rounded" />
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
