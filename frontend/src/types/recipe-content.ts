// ============================================================================
// CULINARY BLOG — TS TYPES CỦA MODULE NỘI DUNG CHI TIẾT CÔNG THỨC & MEDIA (TV4)
// Phân hệ: Recipe Content, Ingredients, Steps, Media Storage & Upload MinIO
// Khớp backend /api/v1/recipes/{id}/ingredients, /steps, /images (SRS FR-RCP-008..010, FR-FILE-001..002)
// ============================================================================

import type { ApiProblemDetails, RecipeAuthorDto } from "./api";

// ----------------------------------------------------------------------------
// ENUMS & ĐỘ KHÓ / TRẠNG THÁI CÔNG THỨC
// ----------------------------------------------------------------------------

export type RecipeDifficulty = "Easy" | "Medium" | "Hard" | 0 | 1 | 2;
export type RecipeStatus = "Draft" | "Published" | "Archived" | "Deleted" | 0 | 1 | 2;
export type DifficultyLevel = RecipeDifficulty | number;

// ----------------------------------------------------------------------------
// 1. NGUYÊN LIỆU CÔNG THỨC (FR-RCP-009, Quyết định E1 & E2)
// ----------------------------------------------------------------------------

export interface RecipeIngredientDto {
  id: string;
  recipeId: string;
  name: string;
  /** Định lượng số thực (Decimal) hoặc null nếu là gia vị nêm nếm vừa đủ. */
  quantity?: number | null;
  /** Đơn vị đo lường (g, kg, ml, muỗng...) hoặc null nếu gia vị vừa đủ. */
  unit?: string | null;
  /** Ghi chú sơ chế (ví dụ: thái lát mỏng, chần qua nước sôi). */
  notes?: string | null;
  /** Thứ tự hiển thị 1..N. */
  orderIndex: number;
  /** Alias tương thích ngược sortOrder. */
  sortOrder?: number;
  createdAt?: string;
  updatedAt?: string | null;
}

export interface CreateRecipeIngredientRequest {
  name: string;
  quantity?: number | null;
  unit?: string | null;
  notes?: string | null;
  orderIndex?: number | null;
  sortOrder?: number | null;
}

export interface UpdateRecipeIngredientRequest {
  name: string;
  quantity?: number | null;
  unit?: string | null;
  notes?: string | null;
  orderIndex: number;
  sortOrder?: number;
}

// ----------------------------------------------------------------------------
// 2. CÁC BƯỚC THỰC HIỆN CÔNG THỨC (FR-RCP-010, Quyết định E5 & E8)
// ----------------------------------------------------------------------------

export interface RecipeStepDto {
  id: string;
  recipeId: string;
  /** Số thứ tự bước làm 1..N. */
  stepNumber: number;
  title?: string | null;
  /** Nội dung hướng dẫn chi tiết từng bước. */
  description: string;
  /** Alias tương thích ngược instruction. */
  instruction?: string;
  /** Thời lượng hẹn giờ đếm ngược tính theo phút (null nếu không cần timer). */
  durationMinutes?: number | null;
  /** Alias tương thích ngược timerMinutes. */
  timerMinutes?: number | null;
  /** Ảnh minh họa riêng cho bước làm. */
  imageUrl?: string | null;
  createdAt?: string;
  updatedAt?: string | null;
}

export interface CreateRecipeStepRequest {
  stepNumber?: number | null;
  title?: string | null;
  description: string;
  instruction?: string;
  durationMinutes?: number | null;
  imageUrl?: string | null;
}

export interface UpdateRecipeStepRequest {
  stepNumber?: number | null;
  title?: string | null;
  description: string;
  instruction?: string;
  durationMinutes?: number | null;
  imageUrl?: string | null;
}

export interface ReorderRecipeStepsRequest {
  stepIds: string[];
}

// ----------------------------------------------------------------------------
// 3. HÌNH ẢNH & MEDIA STORAGE MINIO (FR-RCP-008, FR-FILE-001/002, Quyết định E6 & E7)
// ----------------------------------------------------------------------------

export interface RecipeImageDto {
  id: string;
  /** Alias định danh ảnh theo quyết định kiến trúc E7. */
  imageId?: string;
  recipeId: string;
  /** Đường dẫn ảnh gốc. */
  originalUrl: string;
  /** Alias tương thích ngược imageUrl. */
  imageUrl?: string;
  mediumUrl?: string | null;
  thumbnailUrl?: string | null;
  altText?: string | null;
  /** Đánh dấu ảnh đại diện chính của công thức nấu ăn. */
  isPrimary: boolean;
  orderIndex: number;
  /** Alias tương thích ngược displayOrder. */
  displayOrder?: number;
  createdAt?: string;
}

export interface UpdateRecipeImageRequest {
  altText?: string | null;
  isPrimary?: boolean | null;
  orderIndex?: number | null;
  sortOrder?: number | null;
}

// ----------------------------------------------------------------------------
// 4. THÔNG TIN DINH DƯỠNG CÔNG THỨC (FR-RCP-003)
// ----------------------------------------------------------------------------

export interface RecipeNutritionDto {
  calories?: number | null;
  protein?: number | null;
  proteinGrams?: number | null;
  carbohydrates?: number | null;
  carbsGrams?: number | null;
  fat?: number | null;
  fatGrams?: number | null;
  fiber?: number | null;
  fiberGrams?: number | null;
  sodium?: number | null;
  sodiumMilligrams?: number | null;
  sugarGrams?: number | null;
  source?: string | number | null;
}

// ----------------------------------------------------------------------------
// 5. PAYLOAD RECIPE WIZARD & EDIT FORM (QUẢN LÝ DỮ LIỆU TỔNG THỂ)
// ----------------------------------------------------------------------------

export interface CategoryOptionDto {
  id: string;
  name: string;
  slug: string;
  recipeCount?: number;
}

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

export interface RecipeFullDetailDto {
  id: string;
  title: string;
  slug: string;
  description: string;
  prepTimeMinutes: number;
  cookTimeMinutes: number;
  prepTime?: number;
  cookTime?: number;
  totalTime?: number;
  servings: number;
  difficulty: RecipeDifficulty | number;
  status: RecipeStatus | number;
  categoryId: string;
  categoryName?: string | null;
  categorySlug?: string | null;
  authorId?: string;
  authorName?: string | null;
  author?: RecipeAuthorDto;
  publishedAt?: string | null;
  createdAt: string;
  updatedAt?: string | null;
  primaryImageUrl?: string | null;
  xmin?: string | number | null;
  isDeleted?: boolean;
  deletedAt?: string | null;
  nutrition?: RecipeNutritionDto | null;
  ingredients: RecipeIngredientDto[];
  steps: RecipeStepDto[];
  images: RecipeImageDto[];
}

// ----------------------------------------------------------------------------
// 6. MÃ LỖI PHÂN HỆ RECIPE CONTENT & MEDIA (RFC 7807, Phụ lục B)
// ----------------------------------------------------------------------------

export type RecipeContentErrorCode =
  | "MALFORMED_REQUEST"
  | "VALIDATION_ERROR"
  | "RECIPE_NOT_FOUND"
  | "INGREDIENT_NOT_FOUND"
  | "STEP_NOT_FOUND"
  | "IMAGE_NOT_FOUND"
  | "FILE_TOO_LARGE"
  | "FILE_INVALID_FORMAT"
  | "FORBIDDEN"
  | "UNAUTHORIZED"
  | "RATE_LIMIT_EXCEEDED";

export type RecipeContentProblemDetails = Partial<ApiProblemDetails>;
