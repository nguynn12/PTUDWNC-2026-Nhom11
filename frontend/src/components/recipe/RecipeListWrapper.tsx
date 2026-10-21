"use client";

import React from "react";
import { useUIStore } from "@/store/ui-store";
import { RecipeCard } from "./RecipeCard";
import type { RecipeSummaryDto } from "@/types/api";

interface RecipeListWrapperProps {
  recipes: RecipeSummaryDto[];
}

export function RecipeListWrapper({ recipes }: RecipeListWrapperProps) {
  const { recipesViewMode } = useUIStore();

  return (
    <div
      className={
        recipesViewMode === "list"
          ? "recipe-list flex flex-col gap-6 mb-8"
          : "recipe-grid grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-6 mb-8"
      }
    >
      {recipes.map((recipe, idx) => (
        <RecipeCard
          key={recipe.id || recipe.slug}
          recipe={recipe}
          priority={idx < 3}
        />
      ))}
    </div>
  );
}
