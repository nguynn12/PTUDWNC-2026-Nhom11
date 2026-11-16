import apiClient from "./axios";
import type {
  ApiResponse,
  RecipeSummaryDto,
  RecipeDetailDto,
  GetRecipesParams,
  SearchRecipesParams,
} from "@/types/api";

/**
 * FR-RCP-001: Lấy danh sách công thức nấu ăn phân trang, lọc đa tiêu chí và sắp xếp (Public)
 */
export async function getRecipes(
  params?: GetRecipesParams
): Promise<ApiResponse<RecipeSummaryDto[]>> {
  return await apiClient.get<never, ApiResponse<RecipeSummaryDto[]>>("/recipes", {
    params,
  });
}

/**
 * FR-RCP-002: Xem chi tiết công thức nấu ăn theo Slug (Public)
 * Trả về đầy đủ nguyên liệu, các bước làm, bảng dinh dưỡng, hình ảnh và tác giả.
 */
export async function getRecipeBySlug(slug: string): Promise<RecipeDetailDto> {
  const response = await apiClient.get<never, ApiResponse<RecipeDetailDto>>(
    `/recipes/${slug}`
  );
  const recipe = response.data;
  return {
    ...recipe,
    categoryId: recipe.category?.id ?? recipe.categoryId,
    categoryName: recipe.category?.name ?? recipe.categoryName,
    categorySlug: recipe.category?.slug ?? recipe.categorySlug,
    prepTime: recipe.prepTimeMinutes ?? recipe.prepTime,
    cookTime: recipe.cookTimeMinutes ?? recipe.cookTime,
    totalTime: (recipe.prepTimeMinutes ?? recipe.prepTime ?? 0) + (recipe.cookTimeMinutes ?? recipe.cookTime ?? 0),
    images: recipe.images.map(image => ({ ...image, imageUrl: image.mediumUrl ?? image.originalUrl ?? image.imageUrl })),
    steps: recipe.steps.map(step => ({ ...step, timerMinutes: step.durationMinutes ?? step.timerMinutes })),
    nutrition: recipe.nutrition ? {
      ...recipe.nutrition,
      proteinGrams: recipe.nutrition.protein ?? recipe.nutrition.proteinGrams,
      carbsGrams: recipe.nutrition.carbohydrates ?? recipe.nutrition.carbsGrams,
      fatGrams: recipe.nutrition.fat ?? recipe.nutrition.fatGrams,
      fiberGrams: recipe.nutrition.fiber ?? recipe.nutrition.fiberGrams,
      sodiumMilligrams: recipe.nutrition.sodium ?? recipe.nutrition.sodiumMilligrams,
    } : null,
  };
}

/**
 * FR-SRCH-001: Tìm kiếm toàn văn bản (PostgreSQL Full-Text Search)
 * Tìm kiếm theo từ khóa q (có dấu/không dấu), xếp hạng theo độ liên quan ts_rank.
 */
export async function searchRecipes(
  params: SearchRecipesParams
): Promise<ApiResponse<RecipeSummaryDto[]>> {
  return await apiClient.get<never, ApiResponse<RecipeSummaryDto[]>>(
    "/recipes/search",
    {
      params,
    }
  );
}
