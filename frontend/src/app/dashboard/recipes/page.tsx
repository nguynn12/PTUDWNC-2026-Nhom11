'use client';

import React, { useEffect, useState, useMemo } from 'react';
import Link from 'next/link';
import {
  PlusCircle,
  Trash2,
  Archive,
  CheckCircle2,
  RefreshCw,
  Search,
  Clock,
  Users,
  Edit,
  ExternalLink,
} from 'lucide-react';
import { RecipeApi, ApiError } from '@/lib/api-client';
import { RecipeSummaryDto } from '@/lib/types';
import { useToast } from '@/components/common/Toast';
import { ConflictDialog } from '@/components/common/ConflictDialog';
import { ConfirmDialog } from '@/components/common/ConfirmDialog';
import { Loading } from '@/components/common/Loading';
import { EmptyState } from '@/components/common/EmptyState';
import { ErrorState } from '@/components/common/ErrorState';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent, CardHeader } from '@/components/ui/card';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';

type StatusFilter = 'ALL' | 'DRAFT' | 'PUBLISHED' | 'ARCHIVED';

/**
 * Trang Quản lý Công thức Cá nhân & Vòng đời (Lab 6 - TV3).
 * Thiết kế lại hoàn chỉnh bằng shadcn/ui, Tailwind CSS v4 và tích hợp bộ component dùng chung.
 */
export default function MyRecipesLifecyclePage() {
  const { showSuccess, showError, showWarning } = useToast();

  const [recipes, setRecipes] = useState<RecipeSummaryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [activeFilter, setActiveFilter] = useState<StatusFilter>('ALL');
  const [searchQuery, setSearchQuery] = useState('');
  const [actionInProgress, setActionInProgress] = useState<string | null>(null);

  // State cho 409 Conflict Modal
  const [conflictState, setConflictState] = useState<{
    isOpen: boolean;
    recipeTitle?: string;
    errorMessage?: string;
    currentXmin?: string | number | null;
  }>({ isOpen: false });

  // State cho Confirm Delete Modal
  const [deleteConfirmState, setDeleteConfirmState] = useState<{
    isOpen: boolean;
    recipe?: RecipeSummaryDto;
  }>({ isOpen: false });

  const loadRecipes = async () => {
    try {
      setLoading(true);
      setLoadError(null);
      const res = await RecipeApi.getMyRecipes({ pageSize: 50 });
      setRecipes(res.items || []);
    } catch (err) {
      const msg = (err as Error).message || 'Không thể tải danh sách công thức.';
      setLoadError(msg);
      showError('Không thể tải danh sách công thức', msg);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadRecipes();
  }, []);

  // Lọc bài viết theo Tab trạng thái và Tìm kiếm từ khóa
  const filteredRecipes = useMemo(() => {
    return recipes.filter((item) => {
      const s = (item.status ?? 'DRAFT').toString().toUpperCase();
      let matchTab = true;
      if (activeFilter === 'DRAFT') matchTab = s === 'DRAFT' || s === '0';
      else if (activeFilter === 'PUBLISHED') matchTab = s === 'PUBLISHED' || s === '1';
      else if (activeFilter === 'ARCHIVED') matchTab = s === 'ARCHIVED' || s === '2';

      const query = searchQuery.trim().toLowerCase();
      const matchSearch =
        !query ||
        item.title.toLowerCase().includes(query) ||
        (item.description && item.description.toLowerCase().includes(query));

      return matchTab && matchSearch;
    });
  }, [recipes, activeFilter, searchQuery]);

  // Xử lý lỗi đặc thù (HTTP 409 Conflict token)
  const handleLifecycleError = (err: unknown, recipe: RecipeSummaryDto) => {
    if (err instanceof ApiError && err.status === 409) {
      setConflictState({
        isOpen: true,
        recipeTitle: recipe.title,
        errorMessage: err.message,
        currentXmin: recipe.xmin,
      });
      showWarning('Xung đột đồng thời (HTTP 409)', 'Dữ liệu đã bị thay đổi bởi phiên khác.');
    } else {
      showError('Thao tác không thành công', (err as Error).message);
    }
  };

  // 1. Xuất bản công thức (Publish)
  const handlePublish = async (recipe: RecipeSummaryDto) => {
    try {
      setActionInProgress(recipe.id);
      const updated = await RecipeApi.publishRecipe(recipe.id, recipe.xmin);
      showSuccess(`Đã xuất bản "${recipe.title}" thành công!`, 'Công thức hiện đã hiển thị công khai.');
      setRecipes((prev) =>
        prev.map((r) => (r.id === recipe.id ? { ...r, status: 'Published', xmin: updated?.xmin ?? r.xmin } : r))
      );
    } catch (err) {
      handleLifecycleError(err, recipe);
    } finally {
      setActionInProgress(null);
    }
  };

  // 2. Lưu trữ công thức (Archive)
  const handleArchive = async (recipe: RecipeSummaryDto) => {
    try {
      setActionInProgress(recipe.id);
      const updated = await RecipeApi.archiveRecipe(recipe.id, recipe.xmin);
      showSuccess(`Đã chuyển "${recipe.title}" vào lưu trữ`, 'Công thức tạm thời ẩn khỏi trang công khai.');
      setRecipes((prev) =>
        prev.map((r) => (r.id === recipe.id ? { ...r, status: 'Archived', xmin: updated?.xmin ?? r.xmin } : r))
      );
    } catch (err) {
      handleLifecycleError(err, recipe);
    } finally {
      setActionInProgress(null);
    }
  };

  // 3. Xóa tạm công thức vào thùng rác (Soft Delete)
  const executeDelete = async () => {
    const recipe = deleteConfirmState.recipe;
    if (!recipe) return;

    try {
      setActionInProgress(recipe.id);
      setDeleteConfirmState({ isOpen: false });
      await RecipeApi.deleteRecipe(recipe.id, recipe.xmin);
      showSuccess(`Đã chuyển "${recipe.title}" vào thùng rác`, 'Quản trị viên có thể khôi phục khi cần.');
      setRecipes((prev) => prev.filter((r) => r.id !== recipe.id));
    } catch (err) {
      handleLifecycleError(err, recipe);
    } finally {
      setActionInProgress(null);
    }
  };

  const renderStatusBadge = (status: string | number) => {
    const s = status.toString().toUpperCase();
    if (s === 'PUBLISHED' || s === '1') {
      return (
        <Badge variant="outline" className="bg-emerald-500/10 text-emerald-700 border-emerald-500/30 gap-1.5 font-medium">
          <span className="w-1.5 h-1.5 rounded-full bg-emerald-600 animate-pulse" />
          Đã xuất bản
        </Badge>
      );
    }
    if (s === 'ARCHIVED' || s === '2') {
      return (
        <Badge variant="outline" className="bg-slate-500/10 text-slate-700 border-slate-500/30 gap-1.5 font-medium">
          <span className="w-1.5 h-1.5 rounded-full bg-slate-500" />
          Đã lưu trữ
        </Badge>
      );
    }
    return (
      <Badge variant="outline" className="bg-amber-500/10 text-amber-700 border-amber-500/30 gap-1.5 font-medium">
        <span className="w-1.5 h-1.5 rounded-full bg-amber-500" />
        Bản nháp
      </Badge>
    );
  };

  return (
    <div className="space-y-8">
      {/* HEADER KHU VỰC QUẢN LÝ */}
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 pb-6 border-b border-border">
        <div>
          <h1 className="text-2xl md:text-3xl font-extrabold tracking-tight text-foreground">
            Quản lý Công thức của tôi
          </h1>
          <p className="text-sm text-muted-foreground mt-1">
            Điều khiển trạng thái vòng đời công thức (Bản nháp ➔ Xuất bản ➔ Lưu trữ ➔ Xóa mềm)
          </p>
        </div>
        <div className="flex items-center gap-3">
          <Button
            variant="outline"
            size="sm"
            onClick={loadRecipes}
            disabled={loading}
            className="gap-2 shadow-xs"
          >
            <RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
            Đồng bộ
          </Button>
          <Button asChild size="sm" className="gap-2 shadow-xs">
            <Link href="/dashboard/recipes/new">
              <PlusCircle className="w-4 h-4" />
              Tạo công thức mới
            </Link>
          </Button>
        </div>
      </div>

      {/* XỬ LÝ LỖI TOÀN TRANG NẾU CÓ */}
      {loadError && !loading && (
        <ErrorState
          title="Không thể tải danh sách công thức"
          message={loadError}
          onRetry={loadRecipes}
          retryLabel="Thử nạp lại"
          showBackHome={false}
        />
      )}

      {/* FILTER & SEARCH TABS */}
      <div className="flex flex-col lg:flex-row items-stretch lg:items-center justify-between gap-4">
        <div className="flex items-center gap-1.5 p-1 bg-muted/60 rounded-xl border border-border/60 overflow-x-auto">
          <Button
            type="button"
            variant={activeFilter === 'ALL' ? 'default' : 'ghost'}
            size="sm"
            onClick={() => setActiveFilter('ALL')}
            className="rounded-lg text-xs md:text-sm font-medium"
          >
            Tất cả ({recipes.length})
          </Button>
          <Button
            type="button"
            variant={activeFilter === 'PUBLISHED' ? 'default' : 'ghost'}
            size="sm"
            onClick={() => setActiveFilter('PUBLISHED')}
            className="rounded-lg text-xs md:text-sm font-medium"
          >
            Đã xuất bản ({recipes.filter((r) => (r.status ?? '').toString().toUpperCase() === 'PUBLISHED' || r.status === 1).length})
          </Button>
          <Button
            type="button"
            variant={activeFilter === 'DRAFT' ? 'default' : 'ghost'}
            size="sm"
            onClick={() => setActiveFilter('DRAFT')}
            className="rounded-lg text-xs md:text-sm font-medium"
          >
            Bản nháp ({recipes.filter((r) => (r.status ?? '').toString().toUpperCase() === 'DRAFT' || r.status === 0).length})
          </Button>
          <Button
            type="button"
            variant={activeFilter === 'ARCHIVED' ? 'default' : 'ghost'}
            size="sm"
            onClick={() => setActiveFilter('ARCHIVED')}
            className="rounded-lg text-xs md:text-sm font-medium"
          >
            Đã lưu trữ ({recipes.filter((r) => (r.status ?? '').toString().toUpperCase() === 'ARCHIVED' || r.status === 2).length})
          </Button>
        </div>

        <div className="relative w-full lg:w-80">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
          <Input
            type="text"
            placeholder="Tìm theo tên hoặc mô tả..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="pl-9 bg-card"
          />
        </div>
      </div>

      {/* BẢNG DANH SÁCH CÔNG THỨC */}
      <Card className="border-border/80 shadow-xs overflow-hidden">
        <CardContent className="p-0">
          <Table>
            <TableHeader className="bg-muted/40">
              <TableRow>
                <TableHead className="w-[30%]">Tên công thức & URL</TableHead>
                <TableHead>Danh mục</TableHead>
                <TableHead>Trạng thái Vòng đời</TableHead>
                <TableHead>Thông số nấu</TableHead>
                <TableHead>xmin (Token)</TableHead>
                <TableHead className="text-right">Hành động Vòng đời</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {loading ? (
                <TableRow>
                  <TableCell colSpan={6} className="h-64 text-center">
                    <Loading size="lg" text="Đang tải danh sách bài viết..." />
                  </TableCell>
                </TableRow>
              ) : filteredRecipes.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={6} className="h-72">
                    <EmptyState
                      title="Không tìm thấy công thức nào"
                      description={
                        searchQuery
                          ? 'Không có kết quả nào khớp với từ khóa tìm kiếm của bạn.'
                          : activeFilter !== 'ALL'
                          ? 'Chưa có công thức nào thuộc trạng thái này.'
                          : 'Bạn chưa tạo công thức nấu ăn nào. Hãy bắt đầu chia sẻ bí quyết ẩm thực ngay!'
                      }
                      action={
                        !searchQuery && activeFilter === 'ALL'
                          ? {
                              label: 'Tạo công thức đầu tiên',
                              href: '/dashboard/recipes/new',
                            }
                          : undefined
                      }
                    />
                  </TableCell>
                </TableRow>
              ) : (
                filteredRecipes.map((recipe) => {
                  const statusStr = (recipe.status ?? 'DRAFT').toString().toUpperCase();
                  const isBusy = actionInProgress === recipe.id;

                  return (
                    <TableRow key={recipe.id} className="hover:bg-muted/30 transition-colors">
                      <TableCell>
                        <div className="space-y-1">
                          <Link
                            href={`/recipes/${recipe.slug}`}
                            className="font-bold text-foreground hover:text-primary transition-colors flex items-center gap-1.5 group"
                          >
                            <span>{recipe.title}</span>
                            <ExternalLink className="w-3.5 h-3.5 opacity-0 group-hover:opacity-100 transition-opacity text-muted-foreground" />
                          </Link>
                          <div className="text-xs text-muted-foreground font-mono">
                            slug: /{recipe.slug}
                          </div>
                        </div>
                      </TableCell>

                      <TableCell>
                        <span className="font-medium text-sm text-foreground">
                          {recipe.categoryName || 'Món chính'}
                        </span>
                      </TableCell>

                      <TableCell>
                        {renderStatusBadge(recipe.status ?? 'DRAFT')}
                      </TableCell>

                      <TableCell>
                        <div className="flex items-center gap-3 text-xs text-muted-foreground">
                          <span className="flex items-center gap-1" title="Thời gian nấu">
                            <Clock className="w-3.5 h-3.5" />
                            {(recipe.cookTimeMinutes || recipe.cookTime || 30)}p
                          </span>
                          <span className="flex items-center gap-1" title="Khẩu phần">
                            <Users className="w-3.5 h-3.5" />
                            {recipe.servings || 4} người
                          </span>
                        </div>
                      </TableCell>

                      <TableCell>
                        {recipe.xmin ? (
                          <span className="font-mono text-xs bg-muted px-2 py-0.5 rounded text-muted-foreground border border-border/50">
                            {recipe.xmin}
                          </span>
                        ) : (
                          <span className="text-xs text-muted-foreground italic">mặc định</span>
                        )}
                      </TableCell>

                      <TableCell className="text-right">
                        <div className="flex items-center justify-end gap-1.5">
                          {/* Nút Xuất bản (Publish) */}
                          {(statusStr === 'DRAFT' || statusStr === '0' || statusStr === 'ARCHIVED' || statusStr === '2') && (
                            <Button
                              type="button"
                              variant="outline"
                              size="sm"
                              onClick={() => handlePublish(recipe)}
                              disabled={isBusy}
                              title="Xuất bản công thức"
                              className="h-8 gap-1 text-emerald-700 hover:text-emerald-800 hover:bg-emerald-50 border-emerald-200"
                            >
                              <CheckCircle2 className="w-3.5 h-3.5" />
                              <span className="hidden md:inline">Xuất bản</span>
                            </Button>
                          )}

                          {/* Nút Lưu trữ (Archive) */}
                          {(statusStr === 'PUBLISHED' || statusStr === '1') && (
                            <Button
                              type="button"
                              variant="outline"
                              size="sm"
                              onClick={() => handleArchive(recipe)}
                              disabled={isBusy}
                              title="Lưu trữ công thức"
                              className="h-8 gap-1 text-slate-700 hover:text-slate-800 hover:bg-slate-50 border-slate-200"
                            >
                              <Archive className="w-3.5 h-3.5" />
                              <span className="hidden md:inline">Lưu trữ</span>
                            </Button>
                          )}

                          {/* Nút Chỉnh sửa */}
                          <Button
                            asChild
                            variant="ghost"
                            size="sm"
                            className="h-8 gap-1 text-muted-foreground hover:text-foreground"
                          >
                            <Link href={`/dashboard/recipes/${recipe.id}/edit`}>
                              <Edit className="w-3.5 h-3.5" />
                              <span className="hidden md:inline">Sửa</span>
                            </Link>
                          </Button>

                          {/* Nút Xóa tạm (Soft Delete) */}
                          <Button
                            type="button"
                            variant="ghost"
                            size="sm"
                            onClick={() => setDeleteConfirmState({ isOpen: true, recipe })}
                            disabled={isBusy}
                            title="Chuyển vào thùng rác"
                            className="h-8 gap-1 text-destructive hover:bg-destructive/10"
                          >
                            <Trash2 className="w-3.5 h-3.5" />
                            <span className="hidden md:inline">Xóa</span>
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  );
                })
              )}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      {/* 409 CONFLICT MODAL */}
      <ConflictDialog
        isOpen={conflictState.isOpen}
        recipeTitle={conflictState.recipeTitle}
        errorMessage={conflictState.errorMessage}
        currentXmin={conflictState.currentXmin}
        onReload={() => {
          setConflictState({ isOpen: false });
          loadRecipes();
        }}
        onClose={() => setConflictState({ isOpen: false })}
      />

      {/* CONFIRM DELETE MODAL */}
      <ConfirmDialog
        isOpen={deleteConfirmState.isOpen}
        title="Chuyển công thức vào Thùng rác?"
        message={`Bạn có chắc chắn muốn chuyển công thức "${deleteConfirmState.recipe?.title}" vào thùng rác? Công thức sẽ bị ẩn và chỉ Quản trị viên mới có thể khôi phục hoặc xóa vĩnh viễn.`}
        confirmLabel="Đồng ý xóa tạm"
        cancelLabel="Hủy bỏ"
        isDestructive={true}
        onConfirm={executeDelete}
        onCancel={() => setDeleteConfirmState({ isOpen: false })}
      />
    </div>
  );
}
