"use client";

import React from "react";
import { useUIStore } from "@/store/useUIStore";
import { RecipeCard } from "./RecipeCard";
import type { RecipeSummaryDto } from "@/types/api";

interface RecipeListWrapperProps {
  recipes: RecipeSummaryDto[];
}

export function RecipeListWrapper({ recipes }: RecipeListWrapperProps) {
  const { recipesViewMode } = useUIStore();

  return (
    <div className={recipesViewMode === "list" ? "recipe-list" : "recipe-grid"}>
      {recipes.map((recipe, idx) => (
        <RecipeCard key={recipe.id || recipe.slug} recipe={recipe} priority={idx < 3} />
      ))}
    </div>
  );
}
