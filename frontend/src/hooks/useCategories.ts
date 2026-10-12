import {
  useQuery,
  useMutation,
  useQueryClient,
  type UseQueryOptions,
} from "@tanstack/react-query";
import {
  getCategories,
  getCategoryBySlug,
  createCategory,
  updateCategory,
  deleteCategory,
} from "@/lib/api/categories";
import type {
  CategoryDto,
  CategoryDetailDto,
  CreateCategoryRequest,
  UpdateCategoryRequest,
  AppError,
} from "@/types/api";

// ----------------------------------------------------------------------------
// QUERY KEY FACTORY: CATEGORIES
// ----------------------------------------------------------------------------
export const categoryKeys = {
  all: ["categories"] as const,
  lists: () => [...categoryKeys.all, "list"] as const,
  detail: (slug: string) => [...categoryKeys.all, "detail", slug] as const,
};

// ----------------------------------------------------------------------------
// HOOKS: READ (QUERIES)
// ----------------------------------------------------------------------------

/**
 * Hook lấy danh sách tất cả danh mục (FR-CAT-001)
 */
export function useCategories(
  options?: Omit<UseQueryOptions<CategoryDto[], AppError>, "queryKey" | "queryFn">
) {
  return useQuery<CategoryDto[], AppError>({
    queryKey: categoryKeys.lists(),
    queryFn: getCategories,
    staleTime: 60 * 1000, // 60s
    ...options,
  });
}

/**
 * Hook xem chi tiết danh mục theo Slug (FR-CAT-002)
 */
export function useCategory(
  slug: string,
  options?: Omit<UseQueryOptions<CategoryDetailDto, AppError>, "queryKey" | "queryFn">
) {
  return useQuery<CategoryDetailDto, AppError>({
    queryKey: categoryKeys.detail(slug),
    queryFn: () => getCategoryBySlug(slug),
    enabled: Boolean(slug),
    staleTime: 60 * 1000,
    ...options,
  });
}

// ----------------------------------------------------------------------------
// HOOKS: WRITE (MUTATIONS - ADMIN)
// ----------------------------------------------------------------------------

/**
 * Hook tạo mới danh mục (FR-CAT-003)
 */
export function useCreateCategory() {
  const queryClient = useQueryClient();

  return useMutation<CategoryDto, AppError, CreateCategoryRequest>({
    mutationFn: (data: CreateCategoryRequest) => createCategory(data),
    onSuccess: () => {
      // Invalidate toàn bộ cache categories
      queryClient.invalidateQueries({ queryKey: categoryKeys.all });
    },
  });
}

/**
 * Hook cập nhật thông tin danh mục (FR-CAT-004)
 */
export function useUpdateCategory() {
  const queryClient = useQueryClient();

  return useMutation<
    CategoryDto,
    AppError,
    { id: string; data: UpdateCategoryRequest }
  >({
    mutationFn: ({ id, data }: { id: string; data: UpdateCategoryRequest }) => updateCategory(id, data),
    onSuccess: (updatedCategory: CategoryDto) => {
      queryClient.invalidateQueries({ queryKey: categoryKeys.all });
      if (updatedCategory.slug) {
        queryClient.invalidateQueries({
          queryKey: categoryKeys.detail(updatedCategory.slug),
        });
      }
    },
  });
}

/**
 * Hook xóa danh mục an toàn (FR-CAT-005)
 * Bắt lỗi 409 Conflict (CATEGORY_HAS_RECIPES) nếu danh mục còn công thức.
 */
export function useDeleteCategory() {
  const queryClient = useQueryClient();

  return useMutation<void, AppError, string>({
    mutationFn: (id: string) => deleteCategory(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: categoryKeys.all });
    },
  });
}

