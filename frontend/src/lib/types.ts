// Định nghĩa các types và DTOs phục vụ Frontend phân hệ Recipe Lifecycle (TV3 - Tạ Nhật Nguyên)

export type RecipeStatus = 'Draft' | 'Published' | 'Archived' | 'Deleted';
export type RecipeDifficulty = 'Easy' | 'Medium' | 'Hard';

export interface RecipeNutritionDto {
  calories?: number | null;
  protein?: number | null;
  carbohydrates?: number | null;
  fat?: number | null;
  fiber?: number | null;
  sodium?: number | null;
  source?: number;
}

export interface RecipeSummaryDto {
  id: string;
  title: string;
  slug: string;
  description: string;
  prepTimeMinutes: number;
  cookTimeMinutes: number;
  servings: number;
  difficulty: RecipeDifficulty | number;
  status: RecipeStatus | number;
  categoryId: string;
  categoryName?: string | null;
  authorId: string;
  authorName?: string | null;
  publishedAt?: string | null;
  createdAt: string;
  updatedAt?: string | null;
  primaryImageUrl?: string | null;
  xmin?: string | number | null; // Concurrency token phục vụ If-Match
  isDeleted?: boolean;
  deletedAt?: string | null;
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

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
}

export interface DashboardMetrics {
  totalRecipes: number;
  publishedCount: number;
  draftCount: number;
  archivedCount: number;
  trashCount: number;
}
