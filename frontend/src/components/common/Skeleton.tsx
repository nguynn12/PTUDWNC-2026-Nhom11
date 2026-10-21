import React from "react";
import { Skeleton } from "@/components/ui/skeleton";

export function RecipeCardSkeleton() {
  return (
    <div className="rounded-2xl border border-[var(--border-subtle)] bg-[var(--bg-card)] overflow-hidden flex flex-col h-full">
      <Skeleton className="w-full pt-[62.5%] rounded-none" />
      <div className="p-5 flex flex-col gap-3 flex-1">
        <div className="flex gap-2">
          <Skeleton className="h-5 w-16 rounded-full" />
          <Skeleton className="h-5 w-12 rounded-full" />
        </div>
        <Skeleton className="h-6 w-4/5 rounded-lg" />
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
  );
}

export function CategoryCardSkeleton() {
  return (
    <div className="rounded-2xl border border-[var(--border-subtle)] bg-[var(--bg-surface)] p-5 text-center flex flex-col items-center gap-3">
      <Skeleton className="w-14 h-14 rounded-full" />
      <Skeleton className="w-3/4 h-5 rounded-md" />
      <Skeleton className="w-1/2 h-3.5 rounded-md" />
    </div>
  );
}

export function RecipeDetailSkeleton() {
  return (
    <div className="container mx-auto px-4 py-8 md:py-12 max-w-5xl animate-pulse space-y-8">
      {/* Breadcrumb & Badges */}
      <div className="flex items-center gap-2">
        <Skeleton className="h-4 w-16 rounded" />
        <span className="text-[var(--text-muted)]">/</span>
        <Skeleton className="h-4 w-24 rounded" />
        <span className="text-[var(--text-muted)]">/</span>
        <Skeleton className="h-4 w-32 rounded" />
      </div>

      {/* Header */}
      <div className="space-y-4">
        <div className="flex gap-2">
          <Skeleton className="h-6 w-24 rounded-full" />
          <Skeleton className="h-6 w-20 rounded-full" />
        </div>
        <Skeleton className="h-10 sm:h-12 w-4/5 rounded-2xl" />
        <Skeleton className="h-5 w-full max-w-2xl rounded" />
        <div className="flex items-center gap-3 pt-4 border-t border-[var(--border-subtle)]">
          <Skeleton className="w-11 h-11 rounded-full" />
          <div className="space-y-1.5">
            <Skeleton className="h-4 w-32 rounded" />
            <Skeleton className="h-3 w-20 rounded" />
          </div>
        </div>
      </div>

      {/* Hero Image */}
      <Skeleton className="w-full pt-[56.25%] rounded-3xl" />

      {/* Quick stats */}
      <div className="grid grid-cols-2 sm:grid-cols-4 lg:grid-cols-5 gap-3 p-5 rounded-2xl border border-[var(--border-subtle)] bg-[var(--bg-surface)]">
        <Skeleton className="h-16 rounded-xl" />
        <Skeleton className="h-16 rounded-xl" />
        <Skeleton className="h-16 rounded-xl" />
        <Skeleton className="h-16 rounded-xl" />
        <Skeleton className="h-16 rounded-xl col-span-2 sm:col-span-4 lg:col-span-1" />
      </div>

      {/* 2-col content */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-8">
        <div className="lg:col-span-5 space-y-6">
          <Skeleton className="h-64 rounded-2xl" />
          <Skeleton className="h-48 rounded-2xl" />
        </div>
        <div className="lg:col-span-7 space-y-4">
          <Skeleton className="h-32 rounded-2xl" />
          <Skeleton className="h-32 rounded-2xl" />
          <Skeleton className="h-40 rounded-2xl" />
        </div>
      </div>
    </div>
  );
}
