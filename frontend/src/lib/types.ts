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

export interface CategoryOptionDto {
  id: string;
  name: string;
  slug: string;
  recipeCount?: number;
}

// 1. Nguyên liệu công thức (FR-RCP-009, Quyết định E1 & E2)
export interface RecipeIngredientDto {
  id: string;
  recipeId: string;
  name: string;
  quantity?: number | null;
  unit?: string | null;
  notes?: string | null;
  orderIndex: number;
  createdAt?: string;
  updatedAt?: string | null;
}

export interface CreateRecipeIngredientRequest {
  name: string;
  quantity?: number | null;
  unit?: string | null;
  notes?: string | null;
  orderIndex?: number | null;
}

export interface UpdateRecipeIngredientRequest {
  name: string;
  quantity?: number | null;
  unit?: string | null;
  notes?: string | null;
  orderIndex: number;
}

// 2. Các bước thực hiện (FR-RCP-010, Quyết định E5 & E8)
export interface RecipeStepDto {
  id: string;
  recipeId: string;
  stepNumber: number;
  title: string;
  description: string;
  durationMinutes?: number | null;
  imageUrl?: string | null;
  createdAt?: string;
  updatedAt?: string | null;
}

export interface CreateRecipeStepRequest {
  stepNumber?: number | null;
  title?: string | null;
  description: string;
  durationMinutes?: number | null;
  imageUrl?: string | null;
}

export interface UpdateRecipeStepRequest {
  title?: string | null;
  description: string;
  durationMinutes?: number | null;
  imageUrl?: string | null;
}

export interface ReorderRecipeStepsRequest {
  stepIds: string[];
}

// 3. Hình ảnh & Media MinIO (FR-RCP-008, FR-FILE-001/002, Quyết định E6 & E7)
export interface RecipeImageDto {
  id: string;
  imageId: string; // Alias chuẩn theo quyết định E7
  recipeId: string;
  originalUrl: string;
  mediumUrl?: string | null;
  thumbnailUrl?: string | null;
  altText?: string | null;
  isPrimary: boolean;
  orderIndex: number;
  createdAt?: string;
}

export interface UpdateRecipeImageRequest {
  altText?: string | null;
  isPrimary?: boolean | null;
  orderIndex?: number | null;
}

// 4. Payload Tạo / Cập nhật Công thức tổng thể cho Multi-step Wizard & Edit Form
export interface CreateRecipePayload {
  title: string;
  description: string;
  categoryId: string;
  prepTimeMinutes: number;
  cookTimeMinutes: number;
  servings: number;
  difficulty: RecipeDifficulty | number;
  nutrition?: RecipeNutritionDto | null;
}

export interface UpdateRecipePayload extends CreateRecipePayload {
  xmin?: string | number | null;
}

export interface RecipeFullDetailDto extends RecipeSummaryDto {
  ingredients: RecipeIngredientDto[];
  steps: RecipeStepDto[];
  images: RecipeImageDto[];
}
