// Định nghĩa các types và DTOs phục vụ Frontend phân hệ Recipe Lifecycle (TV3) & Recipe Content/Media (TV4)
// Tái sử dụng các kiểu dữ liệu chuẩn từ @/types/api để tránh trùng lặp khai báo (SRS Mục 5 & Lab 5)

import type {
  RecipeStatus,
  RecipeDifficulty,
  RecipeNutritionDto,
  RecipeSummaryDto as BaseRecipeSummaryDto,
  ApiProblemDetails as ProblemDetails,
  ApiResponse,
  PaginationMeta as PagedMeta,
} from '@/types/api';

export type {
  RecipeStatus,
  RecipeDifficulty,
  RecipeNutritionDto,
  ProblemDetails,
  ApiResponse,
  PagedMeta,
};

export interface RecipeSummaryDto extends BaseRecipeSummaryDto {
  prepTimeMinutes: number;
  cookTimeMinutes: number;
  status: RecipeStatus | number;
}

export type DifficultyLevel = RecipeDifficulty | number;

export interface PaginatedList<T> {
  items: T[];
  pageIndex?: number;
  page?: number;
  totalPages: number;
  totalCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface DashboardMetrics {
  totalRecipes: number;
  publishedCount: number;
  draftCount: number;
  archivedCount: number;
  trashCount: number;
}

// =========================================================
// TYPES PHÂN HỆ THÀNH VIÊN 4 (NGUYỄN PHÚ QUÝ - 2312731)
// Đã được chuẩn hóa và gom về src/types/recipe-content.ts (Lab 5)
// =========================================================

export type {
  CategoryOptionDto,
  RecipeIngredientDto,
  CreateRecipeIngredientRequest,
  UpdateRecipeIngredientRequest,
  RecipeStepDto,
  CreateRecipeStepRequest,
  UpdateRecipeStepRequest,
  ReorderRecipeStepsRequest,
  RecipeImageDto,
  UpdateRecipeImageRequest,
  CreateRecipePayload,
  UpdateRecipePayload,
  RecipeFullDetailDto,
  RecipeContentErrorCode,
  RecipeContentProblemDetails,
} from "@/types/recipe-content";
