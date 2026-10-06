import { ProblemDetails, RecipeSummaryDto, PaginatedList, DashboardMetrics } from './types';

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
// CÁC HÀM GỌI API THEO BẢN PHÂN CÔNG TV3
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
};

// ==========================================
// DỮ LIỆU MẪU DỰ PHÒNG (MOCK FALLBACK)
// ==========================================

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
