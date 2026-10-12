import {
  ProblemDetails,
  RecipeSummaryDto,
  PaginatedList,
  DashboardMetrics,
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
} from './types';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL ?? 'http://localhost:5000/api/v1';

export class ApiError extends Error {
  public status: number;
  public problemDetails?: ProblemDetails;
  public errorCode?: string;

  constructor(status: number, message: string, problemDetails?: ProblemDetails) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.problemDetails = problemDetails;
    this.errorCode = problemDetails?.type ?? (status === 409 ? 'RECIPE_CONCURRENCY_CONFLICT' : undefined);
  }
}

// Lưu trữ token mô phỏng để demo nếu chưa đăng nhập qua Auth.js
let mockToken: string | null = 'mock-jwt-token';

export function setAuthToken(token: string | null) {
  mockToken = token;
}

export function getAuthToken(): string | null {
  return mockToken;
}

// Hàm fetch wrapper xử lý chuẩn RFC 7807 Problem Details và header If-Match
export async function fetchApi<T>(
  endpoint: string,
  options: RequestInit & { ifMatch?: string | number | null } = {}
): Promise<T> {
  const url = `${API_BASE_URL}${endpoint.startsWith('/') ? endpoint : `/${endpoint}`}`;
  const headers = new Headers(options.headers || {});

  headers.set('Accept', 'application/json');

  if (options.body && typeof options.body === 'string') {
    headers.set('Content-Type', 'application/json');
  }

  // Gắn Concurrency Token If-Match nếu có
  if (options.ifMatch !== undefined && options.ifMatch !== null) {
    headers.set('If-Match', `"${options.ifMatch}"`);
  }

  // Gắn Authorization Bearer
  const token = getAuthToken();
  if (token && !headers.has('Authorization')) {
    headers.set('Authorization', `Bearer ${token}`);
  }

  try {
    const response = await fetch(url, {
      ...options,
      headers,
    });

    if (response.status === 204) {
      return {} as T;
    }

    const contentType = response.headers.get('content-type');
    const isJson = contentType && (contentType.includes('application/json') || contentType.includes('application/problem+json'));

    if (!response.ok) {
      let problemDetails: ProblemDetails | undefined;
      if (isJson) {
        try {
          problemDetails = await response.json();
        } catch {
          // Bỏ qua lỗi parse JSON
        }
      }

      const errorMessage =
        problemDetails?.detail ||
        problemDetails?.title ||
        `Yêu cầu API thất bại với mã trạng thái ${response.status}`;

      throw new ApiError(response.status, errorMessage, problemDetails);
    }

    if (isJson) {
      return (await response.json()) as T;
    }

    return (await response.text()) as unknown as T;
  } catch (error) {
    if (error instanceof ApiError) {
      throw error;
    }
    // Lỗi mạng hoặc không kết nối được
    throw new ApiError(0, `Không thể kết nối đến máy chủ Backend (${(error as Error).message})`);
  }
}

// ==========================================
// CÁC HÀM GỌI API THEO BẢN PHÂN CÔNG TV3 & TV4
// ==========================================

export const RecipeApi = {
  // 1. Lấy danh sách công thức của tôi (Author / Admin)
  async getMyRecipes(params: {
    page?: number;
    pageSize?: number;
    status?: string | number;
    search?: string;
  } = {}): Promise<PaginatedList<RecipeSummaryDto>> {
    const query = new URLSearchParams();
    if (params.page) query.set('page', params.page.toString());
    if (params.pageSize) query.set('pageSize', params.pageSize.toString());
    if (params.status !== undefined && params.status !== '') query.set('status', params.status.toString());
    if (params.search) query.set('q', params.search);

    const queryString = query.toString();
    const endpoint = `/recipes${queryString ? `?${queryString}` : ''}`;
    
    try {
      return await fetchApi<PaginatedList<RecipeSummaryDto>>(endpoint);
    } catch {
      // Dữ liệu mẫu (Demo Data Fallback) khi chưa kết nối backend trực tiếp
      return getMockRecipesList(params);
    }
  },

  // 2. Xuất bản công thức (Draft -> Published) kèm If-Match
  async publishRecipe(id: string, xmin?: string | number | null): Promise<RecipeSummaryDto> {
    return await fetchApi<RecipeSummaryDto>(`/recipes/${id}/publish`, {
      method: 'PUT',
      ifMatch: xmin,
    });
  },

  // 3. Lưu trữ công thức (Published -> Archived) kèm If-Match
  async archiveRecipe(id: string, xmin?: string | number | null): Promise<RecipeSummaryDto> {
    return await fetchApi<RecipeSummaryDto>(`/recipes/${id}/archive`, {
      method: 'PUT',
      ifMatch: xmin,
    });
  },

  // 4. Bỏ lưu trữ công thức (Archived -> Draft) kèm If-Match
  async unarchiveRecipe(id: string, xmin?: string | number | null): Promise<RecipeSummaryDto> {
    return await fetchApi<RecipeSummaryDto>(`/recipes/${id}/unarchive`, {
      method: 'PUT',
      ifMatch: xmin,
    });
  },

  // 5. Xóa mềm công thức vào thùng rác (Soft Delete) kèm If-Match
  async deleteRecipe(id: string, xmin?: string | number | null): Promise<void> {
    return await fetchApi<void>(`/recipes/${id}`, {
      method: 'DELETE',
      ifMatch: xmin,
    });
  },

  // 6. Lấy danh sách thùng rác (Admin)
  async getTrashedRecipes(page = 1, pageSize = 10): Promise<PaginatedList<RecipeSummaryDto>> {
    try {
      return await fetchApi<PaginatedList<RecipeSummaryDto>>(`/admin/recipes/trash?page=${page}&pageSize=${pageSize}`);
    } catch {
      return getMockTrashList(page, pageSize);
    }
  },

  // 7. Khôi phục công thức từ thùng rác (Admin)
  async restoreRecipe(id: string): Promise<RecipeSummaryDto> {
    return await fetchApi<RecipeSummaryDto>(`/admin/recipes/${id}/restore`, {
      method: 'PUT',
    });
  },

  // 8. Xóa vĩnh viễn công thức khỏi DB (Admin Purge)
  async purgeRecipe(id: string): Promise<void> {
    return await fetchApi<void>(`/admin/recipes/${id}/purge`, {
      method: 'DELETE',
    });
  },

  // 9. Lấy dữ liệu thống kê Dashboard
  async getDashboardMetrics(): Promise<DashboardMetrics> {
    try {
      const all = await this.getMyRecipes({ pageSize: 100 });
      const trash = await this.getTrashedRecipes(1, 100);

      const items = all.items || [];
      const published = items.filter(r => r.status === 'Published' || r.status === 1).length;
      const draft = items.filter(r => r.status === 'Draft' || r.status === 0).length;
      const archived = items.filter(r => r.status === 'Archived' || r.status === 2).length;

      return {
        totalRecipes: items.length,
        publishedCount: published,
        draftCount: draft,
        archivedCount: archived,
        trashCount: trash.totalCount || trash.items?.length || 0,
      };
    } catch {
      return {
        totalRecipes: 12,
        publishedCount: 8,
        draftCount: 3,
        archivedCount: 1,
        trashCount: 2,
      };
    }
  },

  // =========================================================
  // PHẦN CỦA THÀNH VIÊN 4 (2312731 - NGUYỄN PHÚ QUÝ):
  // WIZARD TẠO/SỬA CÔNG THỨC & 10 API NGUYÊN LIỆU, BƯỚC NẤU, ẢNH MINIO
  // =========================================================

  // Lấy danh sách Danh mục (Categories)
  async getCategories(): Promise<CategoryOptionDto[]> {
    try {
      return await fetchApi<CategoryOptionDto[]>('/categories');
    } catch {
      return mockCategories;
    }
  },

  // Lấy chi tiết đầy đủ công thức (kèm Ingredients, Steps, Images, Nutrition, xmin)
  async getRecipeFullDetail(id: string): Promise<RecipeFullDetailDto> {
    try {
      return await fetchApi<RecipeFullDetailDto>(`/recipes/${id}`);
    } catch (error) {
      if (error instanceof ApiError && error.status !== 0) {
        throw error;
      }
      return getMockRecipeFullDetail(id);
    }
  },

  // Tạo mới thông tin chung của công thức (Bước 1 Wizard)
  async createRecipe(payload: CreateRecipePayload): Promise<RecipeSummaryDto> {
    try {
      return await fetchApi<RecipeSummaryDto>('/recipes', {
        method: 'POST',
        body: JSON.stringify(payload),
      });
    } catch (error) {
      if (error instanceof ApiError && error.status !== 0) {
        throw error;
      }
      return createMockRecipe(payload);
    }
  },

  // Cập nhật thông tin chung của công thức kèm Concurrency Token If-Match: "{xmin}"
  async updateRecipe(
    id: string,
    payload: UpdateRecipePayload,
    xmin?: string | number | null
  ): Promise<RecipeSummaryDto> {
    try {
      return await fetchApi<RecipeSummaryDto>(`/recipes/${id}`, {
        method: 'PUT',
        body: JSON.stringify(payload),
        ifMatch: xmin,
      });
    } catch (error) {
      if (error instanceof ApiError && error.status !== 0) {
        throw error;
      }
      return updateMockRecipe(id, payload, xmin);
    }
  },

  // --- [API 1/10 TV4] POST /api/v1/recipes/{id}/ingredients ---
  async addIngredient(
    recipeId: string,
    request: CreateRecipeIngredientRequest
  ): Promise<RecipeIngredientDto> {
    try {
      return await fetchApi<RecipeIngredientDto>(`/recipes/${recipeId}/ingredients`, {
        method: 'POST',
        body: JSON.stringify(request),
      });
    } catch (error) {
      if (error instanceof ApiError && error.status !== 0) {
        throw error;
      }
      return addMockIngredient(recipeId, request);
    }
  },

  // --- [API 2/10 TV4] PUT /api/v1/recipes/{id}/ingredients/{ingredientId} ---
  async updateIngredient(
    recipeId: string,
    ingredientId: string,
    request: UpdateRecipeIngredientRequest
  ): Promise<RecipeIngredientDto> {
    try {
      return await fetchApi<RecipeIngredientDto>(
        `/recipes/${recipeId}/ingredients/${ingredientId}`,
        {
          method: 'PUT',
          body: JSON.stringify(request),
        }
      );
    } catch (error) {
      if (error instanceof ApiError && error.status !== 0) {
        throw error;
      }
      return updateMockIngredient(recipeId, ingredientId, request);
    }
  },

  // --- [API 3/10 TV4] DELETE /api/v1/recipes/{id}/ingredients/{ingredientId} ---
  async deleteIngredient(recipeId: string, ingredientId: string): Promise<void> {
    try {
      await fetchApi<void>(`/recipes/${recipeId}/ingredients/${ingredientId}`, {
        method: 'DELETE',
      });
    } catch (error) {
      if (error instanceof ApiError && error.status !== 0) {
        throw error;
      }
      deleteMockIngredient(recipeId, ingredientId);
    }
  },

  // --- [API 4/10 TV4] POST /api/v1/recipes/{id}/steps ---
  async addStep(recipeId: string, request: CreateRecipeStepRequest): Promise<RecipeStepDto> {
    try {
      return await fetchApi<RecipeStepDto>(`/recipes/${recipeId}/steps`, {
        method: 'POST',
        body: JSON.stringify(request),
      });
    } catch (error) {
      if (error instanceof ApiError && error.status !== 0) {
        throw error;
      }
      return addMockStep(recipeId, request);
    }
  },

  // --- [API 5/10 TV4] PUT /api/v1/recipes/{id}/steps/{stepId} ---
  async updateStep(
    recipeId: string,
    stepId: string,
    request: UpdateRecipeStepRequest
  ): Promise<RecipeStepDto> {
    try {
      return await fetchApi<RecipeStepDto>(`/recipes/${recipeId}/steps/${stepId}`, {
        method: 'PUT',
        body: JSON.stringify(request),
      });
    } catch (error) {
      if (error instanceof ApiError && error.status !== 0) {
        throw error;
      }
      return updateMockStep(recipeId, stepId, request);
    }
  },

  // --- [API 6/10 TV4] PUT /api/v1/recipes/{id}/steps/reorder ---
  async reorderSteps(
    recipeId: string,
    request: ReorderRecipeStepsRequest
  ): Promise<RecipeStepDto[]> {
    try {
      return await fetchApi<RecipeStepDto[]>(`/recipes/${recipeId}/steps/reorder`, {
        method: 'PUT',
        body: JSON.stringify(request),
      });
    } catch (error) {
      if (error instanceof ApiError && error.status !== 0) {
        throw error;
      }
      return reorderMockSteps(recipeId, request.stepIds);
    }
  },

  // --- [API 7/10 TV4] DELETE /api/v1/recipes/{id}/steps/{stepId} ---
  async deleteStep(recipeId: string, stepId: string): Promise<void> {
    try {
      await fetchApi<void>(`/recipes/${recipeId}/steps/${stepId}`, {
        method: 'DELETE',
      });
    } catch (error) {
      if (error instanceof ApiError && error.status !== 0) {
        throw error;
      }
      deleteMockStep(recipeId, stepId);
    }
  },

  // --- [API 8/10 TV4] POST /api/v1/recipes/{id}/images (Multipart Form-Data lên MinIO) ---
  async uploadImage(
    recipeId: string,
    file: File,
    altText?: string | null,
    isPrimary = false
  ): Promise<RecipeImageDto> {
    const formData = new FormData();
    formData.append('file', file);
    if (altText && altText.trim()) {
      formData.append('altText', altText.trim());
    }
    formData.append('isPrimary', isPrimary ? 'true' : 'false');

    try {
      return await fetchApi<RecipeImageDto>(`/recipes/${recipeId}/images`, {
        method: 'POST',
        body: formData,
      });
    } catch (error) {
      if (error instanceof ApiError && error.status !== 0) {
        throw error;
      }
      return uploadMockImage(recipeId, file, altText ?? null, isPrimary);
    }
  },

  // --- [API 9/10 TV4] PATCH /api/v1/recipes/{id}/images/{imageId} ---
  async updateImage(
    recipeId: string,
    imageId: string,
    request: UpdateRecipeImageRequest
  ): Promise<RecipeImageDto> {
    try {
      return await fetchApi<RecipeImageDto>(`/recipes/${recipeId}/images/${imageId}`, {
        method: 'PATCH',
        body: JSON.stringify(request),
      });
    } catch (error) {
      if (error instanceof ApiError && error.status !== 0) {
        throw error;
      }
      return updateMockImage(recipeId, imageId, request);
    }
  },

  // --- [API 10/10 TV4] DELETE /api/v1/recipes/{id}/images/{imageId} ---
  async deleteImage(recipeId: string, imageId: string): Promise<void> {
    try {
      await fetchApi<void>(`/recipes/${recipeId}/images/${imageId}`, {
        method: 'DELETE',
      });
    } catch (error) {
      if (error instanceof ApiError && error.status !== 0) {
        throw error;
      }
      deleteMockImage(recipeId, imageId);
    }
  },
};

// ==========================================
// DỮ LIỆU MẪU DỰ PHÒNG (MOCK FALLBACK)
// ==========================================

const mockCategories: CategoryOptionDto[] = [
  { id: 'c1', name: 'Món nước', slug: 'mon-nuoc' },
  { id: 'c2', name: 'Món ăn nhanh', slug: 'mon-an-nhanh' },
  { id: 'c3', name: 'Món chính', slug: 'mon-chinh' },
  { id: 'c4', name: 'Khai vị', slug: 'khai-vi' },
  { id: 'c5', name: 'Tráng miệng', slug: 'trang-mieng' },
];

const mockRecipesStorage: RecipeSummaryDto[] = [
  {
    id: 'a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d',
    title: 'Phở Bò Truyền Thống Hà Nội',
    slug: 'pho-bo-truyen-thong-ha-noi',
    description: 'Nước dùng trong veo, đậm đà vị ngọt từ xương bò hầm 8 tiếng cùng hoa hồi và thảo quả thơm lừng.',
    prepTimeMinutes: 45,
    cookTimeMinutes: 180,
    servings: 4,
    difficulty: 'Medium',
    status: 'Published',
    categoryId: 'c1',
    categoryName: 'Món nước',
    authorId: 'user-1',
    authorName: 'Tạ Nhật Nguyên (Bếp Trưởng)',
    publishedAt: '2026-09-28T08:30:00Z',
    createdAt: '2026-09-20T10:00:00Z',
    updatedAt: '2026-09-28T08:30:00Z',
    xmin: '10245',
  },
  {
    id: 'b2c3d4e5-f6a7-8b9c-0d1e-2f3a4b5c6d7e',
    title: 'Bánh Mì Kẹp Thịt Nướng Giòn Rụm',
    slug: 'banh-mi-kep-thit-nuong-gion-rum',
    description: 'Bánh mì vỏ giòn tan, nhân thịt ướp sả ớt nướng than hoa, kèm pate gan béo ngậy và đồ chua thanh mát.',
    prepTimeMinutes: 30,
    cookTimeMinutes: 20,
    servings: 2,
    difficulty: 'Easy',
    status: 'Published',
    categoryId: 'c2',
    categoryName: 'Món ăn nhanh',
    authorId: 'user-1',
    authorName: 'Tạ Nhật Nguyên (Bếp Trưởng)',
    publishedAt: '2026-09-29T14:15:00Z',
    createdAt: '2026-09-22T09:00:00Z',
    updatedAt: '2026-09-29T14:15:00Z',
    xmin: '10246',
  },
  {
    id: 'c3d4e5f6-a7b8-9c0d-1e2f-3a4b5c6d7e8f',
    title: 'Cơm Tấm Sườn Bì Chả Sài Gòn',
    slug: 'com-tam-suon-bi-cha-sai-gon',
    description: 'Sườn cốt lết ướp mật ong nướng óng ánh, ăn kèm chả trứng hấp, bì heo dai giòn và nước mắm kẹo chua ngọt.',
    prepTimeMinutes: 40,
    cookTimeMinutes: 35,
    servings: 3,
    difficulty: 'Hard',
    status: 'Draft',
    categoryId: 'c3',
    categoryName: 'Món chính',
    authorId: 'user-1',
    authorName: 'Tạ Nhật Nguyên (Bếp Trưởng)',
    createdAt: '2026-10-01T11:20:00Z',
    updatedAt: '2026-10-01T11:20:00Z',
    xmin: '10247',
  },
  {
    id: 'd4e5f6a7-b8c9-0d1e-2f3a-4b5c6d7e8f9a',
    title: 'Gỏi Cuốn Tôm Thịt Sốt Tương Đậu Phộng',
    slug: 'goi-cuon-tom-thit-sot-tuong-dau-phong',
    description: 'Tôm sú tươi ngọt cuộn cùng thịt ba chỉ luộc, bún tươi và rau thơm, chấm sốt tương đen đậm đà.',
    prepTimeMinutes: 25,
    cookTimeMinutes: 10,
    servings: 4,
    difficulty: 'Easy',
    status: 'Archived',
    categoryId: 'c4',
    categoryName: 'Khai vị',
    authorId: 'user-1',
    authorName: 'Tạ Nhật Nguyên (Bếp Trưởng)',
    publishedAt: '2026-09-15T16:00:00Z',
    createdAt: '2026-09-10T12:00:00Z',
    updatedAt: '2026-09-25T10:00:00Z',
    xmin: '10248',
  },
];

// Lưu trữ nguyên liệu, bước làm, hình ảnh theo từng recipeId trong bộ nhớ giả lập
const mockIngredientsMap: Record<string, RecipeIngredientDto[]> = {
  'a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d': [
    {
      id: 'ing-1',
      recipeId: 'a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d',
      name: 'Xương ống bò tươi',
      quantity: 1.5,
      unit: 'kg',
      notes: 'Chặt khúc, ngâm nước muối loãng 30 phút',
      orderIndex: 1,
    },
    {
      id: 'ing-2',
      recipeId: 'a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d',
      name: 'Thịt thăn bò mềm',
      quantity: 500,
      unit: 'g',
      notes: 'Thái lát mỏng ngang thớ',
      orderIndex: 2,
    },
    {
      id: 'ing-3',
      recipeId: 'a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d',
      name: 'Bánh phở tươi',
      quantity: 800,
      unit: 'g',
      notes: 'Trần sơ qua nước sôi trước khi ăn',
      orderIndex: 3,
    },
    {
      id: 'ing-4',
      recipeId: 'a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d',
      name: 'Muối hầm, tiêu sọ, hành ngò',
      quantity: null,
      unit: null,
      notes: 'Gia vị vừa đủ theo khẩu vị gia đình',
      orderIndex: 4,
    },
  ],
};

const mockStepsMap: Record<string, RecipeStepDto[]> = {
  'a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d': [
    {
      id: 'step-1',
      recipeId: 'a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d',
      stepNumber: 1,
      title: 'Sơ chế & chần xương bò',
      description: 'Rửa sạch xương ống bò, cho vào nồi nước lạnh đun sôi khoảng 5 phút cùng chút gừng đập dập để khử mùi hôi, sau đó vớt ra rửa lại bằng nước ấm.',
      durationMinutes: 15,
    },
    {
      id: 'step-2',
      recipeId: 'a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d',
      stepNumber: 2,
      title: 'Nướng gia vị & ninh nước dùng',
      description: 'Nướng thơm hành tím, gừng, thảo quả, hoa hồi và thanh quế. Cho vào túi vải thả cùng xương ống bò, ninh lửa nhỏ liu riu và thường xuyên hớt bọt.',
      durationMinutes: 150,
    },
    {
      id: 'step-3',
      recipeId: 'a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d',
      stepNumber: 3,
      title: 'Trình bày & thưởng thức',
      description: 'Chần bánh phở vào bát, xếp thịt bò thái mỏng, hành lá và rau mùi lên trên. Chan nước dùng đang sôi sục để làm chín tái thịt bò và thưởng thức ngay.',
      durationMinutes: 10,
    },
  ],
};

const mockImagesMap: Record<string, RecipeImageDto[]> = {
  'a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d': [
    {
      id: 'img-1',
      imageId: 'img-1',
      recipeId: 'a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d',
      originalUrl: 'https://images.unsplash.com/photo-1582878826629-29b7ad1cdc43?auto=format&fit=crop&w=900&q=80',
      altText: 'Tô phở bò truyền thống Hà Nội nóng hổi',
      isPrimary: true,
      orderIndex: 0,
    },
    {
      id: 'img-2',
      imageId: 'img-2',
      recipeId: 'a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d',
      originalUrl: 'https://images.unsplash.com/photo-1555126634-323283e090fa?auto=format&fit=crop&w=900&q=80',
      altText: 'Nguyên liệu thảo mộc nấu nước dùng phở',
      isPrimary: false,
      orderIndex: 1,
    },
  ],
};

const mockTrashStorage: RecipeSummaryDto[] = [
  {
    id: 'e5f6a7b8-c9d0-1e2f-3a4b-5c6d7e8f9a0b',
    title: 'Canh Chua Cá Hú Bông Điên Điển (Bản thử nghiệm)',
    slug: 'canh-chua-ca-hu-bong-dien-dien-thu-nghiem',
    description: 'Món canh chua đậm vị miền Tây sông nước, vị chua thanh từ me và ngọt béo của cá hú.',
    prepTimeMinutes: 20,
    cookTimeMinutes: 25,
    servings: 4,
    difficulty: 'Medium',
    status: 'Deleted',
    categoryId: 'c1',
    categoryName: 'Món canh',
    authorId: 'user-2',
    authorName: 'Nguyễn Văn Minh',
    createdAt: '2026-09-05T08:00:00Z',
    deletedAt: '2026-09-27T15:30:00Z',
    isDeleted: true,
  },
  {
    id: 'f6a7b8c9-d0e1-2f3a-4b5c-6d7e8f9a0b1c',
    title: 'Chè Bưởi An Giang Giòn Sần Sật',
    slug: 'che-buoi-an-giang-gion-san-sat',
    description: 'Cùi bưởi sơ chế kỹ giòn ngọt không đắng, quyện cùng đậu xanh bùi bùi và nước cốt dừa thơm béo.',
    prepTimeMinutes: 60,
    cookTimeMinutes: 30,
    servings: 6,
    difficulty: 'Hard',
    status: 'Deleted',
    categoryId: 'c5',
    categoryName: 'Tráng miệng',
    authorId: 'user-3',
    authorName: 'Lê Thị Thu',
    createdAt: '2026-08-30T10:00:00Z',
    deletedAt: '2026-09-26T09:10:00Z',
    isDeleted: true,
  },
];

function getMockRecipesList(params: {
  page?: number;
  pageSize?: number;
  status?: string | number;
  search?: string;
}): PaginatedList<RecipeSummaryDto> {
  let filtered = [...mockRecipesStorage];

  if (params.status !== undefined && params.status !== '') {
    const s = params.status.toString().toLowerCase();
    filtered = filtered.filter(r => r.status.toString().toLowerCase() === s);
  }

  if (params.search) {
    const q = params.search.toLowerCase();
    filtered = filtered.filter(r => r.title.toLowerCase().includes(q) || r.description.toLowerCase().includes(q));
  }

  const page = params.page || 1;
  const pageSize = params.pageSize || 10;
  const totalCount = filtered.length;
  const totalPages = Math.ceil(totalCount / pageSize) || 1;
  const start = (page - 1) * pageSize;
  const items = filtered.slice(start, start + pageSize);

  return {
    items,
    page,
    pageIndex: page,
    totalPages,
    totalCount,
    hasPreviousPage: page > 1,
    hasNextPage: page < totalPages,
  };
}

function getMockTrashList(page = 1, pageSize = 10): PaginatedList<RecipeSummaryDto> {
  const totalCount = mockTrashStorage.length;
  const totalPages = Math.ceil(totalCount / pageSize) || 1;
  const start = (page - 1) * pageSize;
  const items = mockTrashStorage.slice(start, start + pageSize);

  return {
    items,
    page,
    pageIndex: page,
    totalPages,
    totalCount,
    hasPreviousPage: page > 1,
    hasNextPage: page < totalPages,
  };
}

function getMockRecipeFullDetail(id: string): RecipeFullDetailDto {
  const found = mockRecipesStorage.find(r => r.id === id) || mockRecipesStorage[0];
  const recipeId = found.id;

  if (!mockIngredientsMap[recipeId]) {
    mockIngredientsMap[recipeId] = [
      {
        id: `ing-default-${recipeId}`,
        recipeId,
        name: 'Nguyên liệu chính tươi ngon',
        quantity: 500,
        unit: 'g',
        notes: 'Sơ chế sạch để ráo',
        orderIndex: 1,
      },
    ];
  }

  if (!mockStepsMap[recipeId]) {
    mockStepsMap[recipeId] = [
      {
        id: `step-default-${recipeId}`,
        recipeId,
        stepNumber: 1,
        title: 'Sơ chế nguyên liệu',
        description: 'Rửa sạch nguyên liệu và ướp gia vị trong 20 phút cho thấm đều.',
        durationMinutes: 20,
      },
    ];
  }

  if (!mockImagesMap[recipeId]) {
    const imgId = `img-default-${recipeId}`;
    mockImagesMap[recipeId] = [
      {
        id: imgId,
        imageId: imgId,
        recipeId,
        originalUrl: 'https://images.unsplash.com/photo-1504674900247-0877df9cc836?auto=format&fit=crop&w=900&q=80',
        altText: found.title,
        isPrimary: true,
        orderIndex: 0,
      },
    ];
  }

  return {
    ...found,
    nutrition: {
      calories: 520,
      protein: 32,
      fat: 18,
      carbohydrates: 58,
      fiber: 4.5,
      sodium: 680,
    },
    ingredients: [...mockIngredientsMap[recipeId]].sort((a, b) => a.orderIndex - b.orderIndex),
    steps: [...mockStepsMap[recipeId]].sort((a, b) => a.stepNumber - b.stepNumber),
    images: [...mockImagesMap[recipeId]].sort((a, b) => a.orderIndex - b.orderIndex),
  };
}

function createMockRecipe(payload: CreateRecipePayload): RecipeSummaryDto {
  const id = `rec-${Date.now()}`;
  const cat = mockCategories.find(c => c.id === payload.categoryId) || mockCategories[0];
  const newRecipe: RecipeSummaryDto = {
    id,
    title: payload.title,
    slug: payload.title
      .toLowerCase()
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/(^-|-$)/g, ''),
    description: payload.description,
    prepTimeMinutes: payload.prepTimeMinutes,
    cookTimeMinutes: payload.cookTimeMinutes,
    servings: payload.servings,
    difficulty: payload.difficulty,
    status: 'Draft',
    categoryId: cat.id,
    categoryName: cat.name,
    authorId: 'user-4',
    authorName: 'Nguyễn Phú Quý (TV4)',
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
    xmin: String(Math.floor(10000 + Math.random() * 90000)),
  };
  mockRecipesStorage.unshift(newRecipe);
  mockIngredientsMap[id] = [];
  mockStepsMap[id] = [];
  mockImagesMap[id] = [];
  return newRecipe;
}

function updateMockRecipe(
  id: string,
  payload: UpdateRecipePayload,
  xmin?: string | number | null
): RecipeSummaryDto {
  const idx = mockRecipesStorage.findIndex(r => r.id === id);
  if (idx === -1) {
    throw new ApiError(404, 'Không tìm thấy công thức cần cập nhật.');
  }

  const current = mockRecipesStorage[idx];
  if (xmin && current.xmin && String(xmin) !== String(current.xmin)) {
    throw new ApiError(409, 'Xung đột dữ liệu: Công thức đã bị thay đổi bởi một phiên làm việc khác.', {
      type: 'RECIPE_CONCURRENCY_CONFLICT',
      title: 'Concurrency Conflict',
      status: 409,
      detail: `Token If-Match "${xmin}" không khớp với phiên bản hiện tại "${current.xmin}".`,
    });
  }

  const cat = mockCategories.find(c => c.id === payload.categoryId);
  const updated: RecipeSummaryDto = {
    ...current,
    title: payload.title,
    description: payload.description,
    prepTimeMinutes: payload.prepTimeMinutes,
    cookTimeMinutes: payload.cookTimeMinutes,
    servings: payload.servings,
    difficulty: payload.difficulty,
    categoryId: payload.categoryId ?? current.categoryId,
    categoryName: cat ? cat.name : current.categoryName,
    updatedAt: new Date().toISOString(),
    xmin: String(Number(current.xmin || 10000) + 1),
  };
  mockRecipesStorage[idx] = updated;
  return updated;
}

function addMockIngredient(
  recipeId: string,
  request: CreateRecipeIngredientRequest
): RecipeIngredientDto {
  const list = mockIngredientsMap[recipeId] || [];
  const nextOrder =
    request.orderIndex && request.orderIndex > 0
      ? request.orderIndex
      : list.length > 0
        ? Math.max(...list.map(i => i.orderIndex)) + 1
        : 1;
  const item: RecipeIngredientDto = {
    id: `ing-${Date.now()}-${Math.random().toString(36).slice(2, 6)}`,
    recipeId,
    name: request.name,
    quantity: request.quantity ?? null,
    unit: request.unit ?? null,
    notes: request.notes ?? null,
    orderIndex: nextOrder,
  };
  mockIngredientsMap[recipeId] = [...list, item];
  return item;
}

function updateMockIngredient(
  recipeId: string,
  ingredientId: string,
  request: UpdateRecipeIngredientRequest
): RecipeIngredientDto {
  const list = mockIngredientsMap[recipeId] || [];
  const idx = list.findIndex(i => i.id === ingredientId);
  const updated: RecipeIngredientDto = {
    id: ingredientId,
    recipeId,
    name: request.name,
    quantity: request.quantity ?? null,
    unit: request.unit ?? null,
    notes: request.notes ?? null,
    orderIndex: request.orderIndex,
  };
  if (idx >= 0) {
    list[idx] = updated;
  } else {
    list.push(updated);
  }
  mockIngredientsMap[recipeId] = [...list];
  return updated;
}

function deleteMockIngredient(recipeId: string, ingredientId: string): void {
  const list = mockIngredientsMap[recipeId] || [];
  mockIngredientsMap[recipeId] = list.filter(i => i.id !== ingredientId);
}

function addMockStep(recipeId: string, request: CreateRecipeStepRequest): RecipeStepDto {
  const list = mockStepsMap[recipeId] || [];
  const stepNumber = request.stepNumber && request.stepNumber > 0 ? request.stepNumber : list.length + 1;
  const item: RecipeStepDto = {
    id: `step-${Date.now()}-${Math.random().toString(36).slice(2, 6)}`,
    recipeId,
    stepNumber,
    title: request.title ?? `Bước ${stepNumber}`,
    description: request.description,
    durationMinutes: request.durationMinutes ?? null,
    imageUrl: request.imageUrl ?? null,
  };
  mockStepsMap[recipeId] = [...list, item].sort((a, b) => a.stepNumber - b.stepNumber);
  return item;
}

function updateMockStep(
  recipeId: string,
  stepId: string,
  request: UpdateRecipeStepRequest
): RecipeStepDto {
  const list = mockStepsMap[recipeId] || [];
  const idx = list.findIndex(s => s.id === stepId);
  const existingStepNumber = idx >= 0 ? list[idx].stepNumber : list.length + 1;
  const updated: RecipeStepDto = {
    id: stepId,
    recipeId,
    stepNumber: existingStepNumber,
    title: request.title ?? `Bước ${existingStepNumber}`,
    description: request.description,
    durationMinutes: request.durationMinutes ?? null,
    imageUrl: request.imageUrl ?? null,
  };
  if (idx >= 0) {
    list[idx] = updated;
  } else {
    list.push(updated);
  }
  mockStepsMap[recipeId] = [...list].sort((a, b) => a.stepNumber - b.stepNumber);
  return updated;
}

function reorderMockSteps(recipeId: string, stepIds: string[]): RecipeStepDto[] {
  const list = mockStepsMap[recipeId] || [];
  const reordered: RecipeStepDto[] = [];
  stepIds.forEach((id, idx) => {
    const found = list.find(s => s.id === id);
    if (found) {
      reordered.push({ ...found, stepNumber: idx + 1 });
    }
  });
  mockStepsMap[recipeId] = reordered;
  return reordered;
}

function deleteMockStep(recipeId: string, stepId: string): void {
  const list = (mockStepsMap[recipeId] || []).filter(s => s.id !== stepId);
  mockStepsMap[recipeId] = list
    .sort((a, b) => a.stepNumber - b.stepNumber)
    .map((s, idx) => ({ ...s, stepNumber: idx + 1 }));
}

function uploadMockImage(
  recipeId: string,
  file: File,
  altText: string | null,
  isPrimary: boolean
): RecipeImageDto {
  const list = mockImagesMap[recipeId] || [];
  const shouldBePrimary = isPrimary || list.length === 0;
  const updatedList = shouldBePrimary ? list.map(img => ({ ...img, isPrimary: false })) : [...list];
  const objectUrl = typeof URL !== 'undefined' && URL.createObjectURL ? URL.createObjectURL(file) : 'https://images.unsplash.com/photo-1546069901-ba9599a7e63c?auto=format&fit=crop&w=900&q=80';
  const genId = `img-${Date.now()}-${Math.random().toString(36).slice(2, 6)}`;

  const newImg: RecipeImageDto = {
    id: genId,
    imageId: genId,
    recipeId,
    originalUrl: objectUrl,
    altText: altText || file.name,
    isPrimary: shouldBePrimary,
    orderIndex: updatedList.length,
  };
  mockImagesMap[recipeId] = [...updatedList, newImg];
  return newImg;
}

function updateMockImage(
  recipeId: string,
  imageId: string,
  request: UpdateRecipeImageRequest
): RecipeImageDto {
  let list = mockImagesMap[recipeId] || [];
  if (request.isPrimary) {
    list = list.map(img => ({ ...img, isPrimary: img.id === imageId }));
  }
  const idx = list.findIndex(img => img.id === imageId);
  if (idx >= 0) {
    list[idx] = {
      ...list[idx],
      altText: request.altText !== undefined ? request.altText : list[idx].altText,
      isPrimary: request.isPrimary !== undefined && request.isPrimary !== null ? request.isPrimary : list[idx].isPrimary,
      orderIndex: request.orderIndex !== undefined && request.orderIndex !== null ? request.orderIndex : list[idx].orderIndex,
    };
    mockImagesMap[recipeId] = [...list].sort((a, b) => a.orderIndex - b.orderIndex);
    return list[idx];
  }
  throw new ApiError(404, 'Không tìm thấy hình ảnh.');
}

function deleteMockImage(recipeId: string, imageId: string): void {
  const list = mockImagesMap[recipeId] || [];
  const target = list.find(i => i.id === imageId);
  const remaining = list.filter(i => i.id !== imageId);
  if (target?.isPrimary && remaining.length > 0) {
    remaining[0] = { ...remaining[0], isPrimary: true };
  }
  mockImagesMap[recipeId] = remaining;
}


