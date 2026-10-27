import {
  useQuery,
  keepPreviousData,
  type UseQueryOptions,
} from "@tanstack/react-query";
import {
  getRecipes,
  getRecipeBySlug,
  searchRecipes,
} from "@/lib/api/recipes";
import type {
  ApiResponse,
  RecipeSummaryDto,
  RecipeDetailDto,
  GetRecipesParams,
  SearchRecipesParams,
  AppError,
} from "@/types/api";

// ----------------------------------------------------------------------------
// QUERY KEY FACTORY: RECIPES
// ----------------------------------------------------------------------------
export const recipeKeys = {
  all: ["recipes"] as const,
  lists: () => [...recipeKeys.all, "list"] as const,
  list: (filters?: GetRecipesParams) => [...recipeKeys.lists(), filters ?? {}] as const,
  details: () => [...recipeKeys.all, "detail"] as const,
  detail: (slug: string) => [...recipeKeys.details(), slug] as const,
  searches: () => [...recipeKeys.all, "search"] as const,
  search: (params: SearchRecipesParams) => [...recipeKeys.searches(), params] as const,
};

// ----------------------------------------------------------------------------
// HOOKS: RECIPES QUERIES
// ----------------------------------------------------------------------------

/**
 * Hook duyệt danh sách công thức có bộ lọc đa tiêu chí và phân trang (FR-RCP-001)
 * Giữ nguyên dữ liệu cũ khi đổi trang (keepPreviousData) để trải nghiệm không bị nhấp nháy.
 */
export function useRecipes(
  params?: GetRecipesParams,
  options?: Omit<
    UseQueryOptions<ApiResponse<RecipeSummaryDto[]>, AppError>,
    "queryKey" | "queryFn"
  >
) {
  return useQuery<ApiResponse<RecipeSummaryDto[]>, AppError>({
    queryKey: recipeKeys.list(params),
    queryFn: () => getRecipes(params),
    placeholderData: keepPreviousData,
    staleTime: 60 * 1000,
    ...options,
  });
}

/**
 * Hook xem chi tiết công thức nấu ăn theo Slug (FR-RCP-002)
 */
export function useRecipe(
  slug: string,
  options?: Omit<UseQueryOptions<RecipeDetailDto, AppError>, "queryKey" | "queryFn">
) {
  return useQuery<RecipeDetailDto, AppError>({
    queryKey: recipeKeys.detail(slug),
    queryFn: () => getRecipeBySlug(slug),
    enabled: Boolean(slug),
    staleTime: 5 * 60 * 1000, // 5 phút cache
    ...options,
  });
}

/**
 * Hook tìm kiếm toàn văn bản tiếng Việt FTS (FR-SRCH-001)
 * Chỉ kích hoạt truy vấn khi từ khóa q đạt tối thiểu 2 ký tự theo chuẩn SRS.
 */
export function useSearchRecipes(
  params: SearchRecipesParams,
  options?: Omit<
    UseQueryOptions<ApiResponse<RecipeSummaryDto[]>, AppError>,
    "queryKey" | "queryFn"
  >
) {
  const isQueryValid = Boolean(params.q && params.q.trim().length >= 2);

  return useQuery<ApiResponse<RecipeSummaryDto[]>, AppError>({
    queryKey: recipeKeys.search(params),
    queryFn: () => searchRecipes(params),
    enabled: isQueryValid,
    placeholderData: keepPreviousData,
    staleTime: 60 * 1000,
    ...options,
  });
}
