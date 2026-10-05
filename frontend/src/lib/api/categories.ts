import apiClient from "./axios";
import type {
  ApiResponse,
  CategoryDto,
  CategoryDetailDto,
  CreateCategoryRequest,
  UpdateCategoryRequest,
} from "@/types/api";

/**
 * FR-CAT-001: Lấy danh sách tất cả danh mục công thức (Public)
 * Kết quả sắp xếp theo OrderIndex và Name, kèm số lượng công thức Published (recipeCount).
 */
export async function getCategories(): Promise<CategoryDto[]> {
  const response = await apiClient.get<never, ApiResponse<CategoryDto[]>>("/categories");
  return response.data;
}

/**
 * FR-CAT-002: Lấy thông tin chi tiết danh mục theo Slug chuẩn SEO (Public)
 */
export async function getCategoryBySlug(slug: string): Promise<CategoryDetailDto> {
  const response = await apiClient.get<never, ApiResponse<CategoryDetailDto>>(`/categories/${slug}`);
  return response.data;
}

/**
 * FR-CAT-003: Tạo danh mục mới (Yêu cầu quyền Admin)
 */
export async function createCategory(data: CreateCategoryRequest): Promise<CategoryDto> {
  const response = await apiClient.post<never, ApiResponse<CategoryDto>>("/categories", data);
  return response.data;
}

/**
 * FR-CAT-004: Cập nhật danh mục (Yêu cầu quyền Admin, Slug giữ nguyên)
 */
export async function updateCategory(id: string, data: UpdateCategoryRequest): Promise<CategoryDto> {
  const response = await apiClient.put<never, ApiResponse<CategoryDto>>(`/categories/${id}`, data);
  return response.data;
}

/**
 * FR-CAT-005: Xóa danh mục an toàn (Yêu cầu quyền Admin)
 * Bắt lỗi 409 Conflict (CATEGORY_HAS_RECIPES) nếu danh mục vẫn còn công thức liên kết.
 */
export async function deleteCategory(id: string): Promise<void> {
  await apiClient.delete(`/categories/${id}`);
}
