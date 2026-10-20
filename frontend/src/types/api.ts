// ============================================================================
// CULINARY BLOG  EAPI CONTRACTS & DTO TYPES
// Chuẩn hóa theo SRS v1.3.0 & Backend Minimal APIs
// ============================================================================

// ----------------------------------------------------------------------------
// ENUMS & CONSTANTS
// ----------------------------------------------------------------------------
export type RecipeDifficulty = "Easy" | "Medium" | "Hard" | 0 | 1 | 2;
export type DifficultyLevel = RecipeDifficulty | number;
export type RecipeStatus = "Draft" | "Published" | "Archived" | "Deleted" | number;

// ----------------------------------------------------------------------------
// COMMON RESPONSE & PAGINATION
// ----------------------------------------------------------------------------
export interface PaginationMeta {
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  pageIndex?: number;
  total?: number;
  hasNextPage?: boolean;
  hasPreviousPage?: boolean;
}

export interface PaginatedList<T> {
  items: T[];
  pageIndex?: number;
  page?: number;
  totalPages: number;
  totalCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface ApiResponse<T> {
  data: T;
  meta?: PaginationMeta | null;
}

export interface ApiProblemDetails {
  type: string;
  title: string;
  status: number;
  detail: string;
  instance?: string;
  errors?: Record<string, string[]>;
}

export type ProblemDetails = ApiProblemDetails;

export class AppError extends Error {
  status: number;
  errorCode: string;
  detail: string;
  validationErrors?: Record<string, string[]>;

  constructor(problem: ApiProblemDetails) {
    super(problem.detail || problem.title || "Đã xảy ra lỗi không xác định.");
    this.name = "AppError";
    this.status = problem.status || 500;
    this.errorCode = problem.type || "UNKNOWN_ERROR";
    this.detail = problem.detail || "";
    this.validationErrors = problem.errors;
  }
}

// ----------------------------------------------------------------------------
// CATEGORY DTOS (FR-CAT-001 -> FR-CAT-005)
// ----------------------------------------------------------------------------
export interface CategoryDto {
  id: string;
  name: string;
  slug: string;
  description?: string | null;
  imageUrl?: string | null;
  orderIndex: number;
  recipeCount: number;
  createdAt?: string;
  updatedAt?: string | null;
}

export interface CategoryDetailDto {
  id: string;
  name: string;
  slug: string;
  description?: string | null;
  imageUrl?: string | null;
  orderIndex: number;
  recipeCount: number;
  createdAt?: string;
  updatedAt?: string | null;
}

export interface CreateCategoryRequest {
  name: string;
  description?: string;
  imageUrl?: string;
  orderIndex?: number;
}

export interface UpdateCategoryRequest {
  name: string;
  description?: string;
  imageUrl?: string;
  orderIndex?: number;
}

// ----------------------------------------------------------------------------
// RECIPE DTOS (FR-RCP-001, FR-RCP-002, FR-SRCH-001)
// ----------------------------------------------------------------------------
export interface RecipeAuthorDto {
  id: string;
  userName: string;
  displayName?: string | null;
  avatarUrl?: string | null;
}

export interface RecipeIngredientDto {
  id: string;
  recipeId?: string;
  name: string;
  quantity?: number | null;
  unit?: string | null;
  notes?: string | null;
  sortOrder?: number;
  orderIndex?: number;
  createdAt?: string;
  updatedAt?: string | null;
}

export interface RecipeStepDto {
  id: string;
  recipeId?: string;
  stepNumber: number;
  title?: string | null;
  description: string;
  imageUrl?: string | null;
  timerMinutes?: number | null;
  durationMinutes?: number | null;
  orderIndex?: number;
  sortOrder?: number;
  createdAt?: string;
  updatedAt?: string | null;
}

export interface RecipeImageDto {
  id: string;
  recipeId?: string;
  imageId?: string;
  imageUrl?: string;
  originalUrl?: string;
  mediumUrl?: string | null;
  thumbnailUrl?: string | null;
  altText?: string | null;
  isPrimary: boolean;
  displayOrder?: number;
  orderIndex?: number;
  createdAt?: string;
  updatedAt?: string | null;
}

export interface RecipeNutritionDto {
  calories?: number | null;
  fatGrams?: number | null;
  carbsGrams?: number | null;
  proteinGrams?: number | null;
  fiberGrams?: number | null;
  sodiumMilligrams?: number | null;
  sugarGrams?: number | null;
  source?: string | number | null;
  // Aliases đồng bềEvới form TV3 & TV4
  fat?: number | null;
  carbohydrates?: number | null;
  protein?: number | null;
  fiber?: number | null;
  sodium?: number | null;
}

export interface RecipeSummaryDto {
  id: string;
  title: string;
  slug: string;
  description: string;
  prepTime?: number;
  cookTime?: number;
  totalTime?: number;
  prepTimeMinutes?: number;
  cookTimeMinutes?: number;
  servings: number;
  difficulty: RecipeDifficulty | number;
  status?: RecipeStatus | number;
  primaryImageUrl?: string | null;
  categoryId: string;
  categoryName?: string | null;
  categorySlug?: string | null;
  author?: RecipeAuthorDto | null;
  authorId?: string;
  authorName?: string | null;
  calories?: number | null;
  publishedAt?: string | null;
  createdAt: string;
  updatedAt?: string | null;
  xmin?: string | number | null; // Concurrency token phục vụ If-Match
  isDeleted?: boolean;
  deletedAt?: string | null;
  nutrition?: RecipeNutritionDto | null;
}

export interface RecipeDetailDto {
  id: string;
  title: string;
  slug: string;
  description: string;
  prepTime?: number;
  cookTime?: number;
  totalTime?: number;
  prepTimeMinutes?: number;
  cookTimeMinutes?: number;
  servings: number;
  difficulty: RecipeDifficulty;
  status: RecipeStatus;
  primaryImageUrl?: string | null;
  categoryId: string;
  categoryName?: string | null;
  categorySlug?: string | null;
  author?: RecipeAuthorDto | null;
  nutrition?: RecipeNutritionDto | null;
  ingredients: RecipeIngredientDto[];
  steps: RecipeStepDto[];
  images: RecipeImageDto[];
  publishedAt?: string | null;
  createdAt: string;
  updatedAt?: string | null;
  rowVersion?: string;
  xmin?: string | number | null;
}

export interface RecipeFullDetailDto extends RecipeDetailDto {
  prepTimeMinutes?: number;
  cookTimeMinutes?: number;
  xmin?: string | number | null;
}

export interface DashboardMetrics {
  totalRecipes: number;
  publishedCount: number;
  draftCount: number;
  archivedCount: number;
  trashCount: number;
}

// ----------------------------------------------------------------------------
// QUERY PARAMS
// ----------------------------------------------------------------------------
export interface GetRecipesParams {
  page?: number;
  pageSize?: number;
  categoryId?: string;
  difficulty?: RecipeDifficulty;
  maxPrepTime?: number;
  maxCookTime?: number;
  maxTotalTime?: number;
  minCalories?: number;
  maxCalories?: number;
  sortBy?: "publishedAt" | "newest" | "totalTime" | "quickest" | "prepTime" | "cookTime" | "calories" | "title" | "servings" | "createdAt";
  sortOrder?: "asc" | "desc";
}

export interface SearchRecipesParams extends GetRecipesParams {
  q: string;
}

// ----------------------------------------------------------------------------
// HEALTH CHECK DTOS (FR-OBS-001)
// ----------------------------------------------------------------------------
export interface HealthReportEntry {
  status: string;
  description?: string;
  duration: string;
  data?: Record<string, unknown>;
}

export interface HealthReport {
  status: "Healthy" | "Degraded" | "Unhealthy";
  totalDuration: string;
  entries: Record<string, HealthReportEntry>;
}
