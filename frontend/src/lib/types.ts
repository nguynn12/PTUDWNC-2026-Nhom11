// Re-export toàn bộ kiểu dữ liệu chuẩn từ src/types/api.ts nhằm loại bỏ trùng lặp theo yêu cầu Lab 5
export * from '@/types/api';

// Các request interfaces đặc thù cho form tạo/sửa Wizard đa bước
export interface CategoryOptionDto {
  id: string;
  name: string;
  slug: string;
  recipeCount?: number;
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

export interface UpdateRecipeImageRequest {
  altText?: string | null;
  isPrimary?: boolean | null;
  orderIndex?: number | null;
}

export interface CreateRecipePayload {
  title: string;
  description: string;
  categoryId: string;
  prepTimeMinutes: number;
  cookTimeMinutes: number;
  servings: number;
  difficulty: import('@/types/api').RecipeDifficulty;
  nutrition?: import('@/types/api').RecipeNutritionDto | null;
}

export interface UpdateRecipePayload extends CreateRecipePayload {
  xmin?: string | number | null;
}
