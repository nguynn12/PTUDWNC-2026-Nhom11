'use client';

import React, { useEffect, useState } from 'react';
import {
  Trash2,
  RefreshCw,
  Clock,
  RotateCcw,
  BookOpen,
} from 'lucide-react';
import { RecipeApi } from '@/lib/api-client';
import { RecipeSummaryDto } from '@/lib/types';
import { useToast } from '@/components/common/Toast';
import { ConfirmDialog } from '@/components/common/ConfirmDialog';
import { Loading } from '@/components/common/Loading';
import { EmptyState } from '@/components/common/EmptyState';
import { ErrorState } from '@/components/common/ErrorState';
import { Button } from '@/components/ui/button';
import { ManagementPageHeader } from '@/components/layout/ManagementPageHeader';
import { Card } from '@/components/ui/card';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';

/**
 * Trang Quản lý Thùng rác Quản trị viên (Lab 6 - TV3).
 * Thiết kế lại hoàn chỉnh bằng shadcn/ui, Tailwind CSS v4 và tích hợp EmptyState / ErrorState / Loading.
 */
export default function AdminTrashManagementPage() {
  const { showSuccess, showError } = useToast();

  const [trashedRecipes, setTrashedRecipes] = useState<RecipeSummaryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [actionInProgress, setActionInProgress] = useState<string | null>(null);

  // Phân trang Server-side
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  // State cho Purge Confirm Modal
  const [purgeConfirmState, setPurgeConfirmState] = useState<{
    isOpen: boolean;
    recipe?: RecipeSummaryDto;
  }>({ isOpen: false });

  const loadTrash = async (targetPage = page) => {
    try {
      setLoading(true);
      setLoadError(null);
      let res = await RecipeApi.getTrashedRecipes(targetPage, pageSize);
      const resolvedPage = Math.min(targetPage, Math.max(1, res.totalPages || 1));
      if (resolvedPage !== targetPage) res = await RecipeApi.getTrashedRecipes(resolvedPage, pageSize);
      setTrashedRecipes(res.items || []);
      setTotalPages(res.totalPages || 1);
      setTotalCount(res.totalCount || 0);
      setPage(resolvedPage);
    } catch (err) {
      const msg = (err as Error).message || 'Không thể tải danh sách thùng rác.';
      setLoadError(msg);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadTrash();
  }, []);

  // 1. Khôi phục công thức về bản nháp (Restore)
  const handleRestore = async (recipe: RecipeSummaryDto) => {
    try {
      setActionInProgress(recipe.id);
      await RecipeApi.restoreRecipe(recipe.id);
      showSuccess(`Đã khôi phục "${recipe.title}" thành công!`, 'Công thức đã được đưa về trạng thái Bản nháp.');
      await loadTrash();
    } catch (err) {
      showError('Khôi phục thất bại', (err as Error).message);
    } finally {
      setActionInProgress(null);
    }
  };

  // 2. Xóa vĩnh viễn khỏi Database (Purge)
  const executePurge = async () => {
    const recipe = purgeConfirmState.recipe;
    if (!recipe) return;

    try {
      setActionInProgress(recipe.id);
      setPurgeConfirmState({ isOpen: false });
      await RecipeApi.purgeRecipe(recipe.id);
      showSuccess(`Đã xóa vĩnh viễn "${recipe.title}"`, 'Dữ liệu đã được xóa hoàn toàn khỏi hệ thống.');
      await loadTrash();
    } catch (err) {
      showError('Xóa vĩnh viễn thất bại', (err as Error).message);
    } finally {
      setActionInProgress(null);
    }
  };

  return (
    <div className="management-page">
      <ManagementPageHeader
        title="Thùng rác công thức"
        description="Quản lý các công thức đã xóa mềm. Khôi phục hoặc xóa vĩnh viễn khi cần."
        actions={
          <Button variant="outline" size="sm" onClick={() => loadTrash()} disabled={loading} className="gap-2">
            <RefreshCw className={`size-4 ${loading ? 'animate-spin' : ''}`} aria-hidden="true" />
            Làm mới
          </Button>
        }
      />

      {loading ? (
        <Loading className="h-auto w-full py-8 text-center" text="Đang tải thùng rác..." />
      ) : loadError ? (
        <ErrorState
          className="w-full max-w-none"
          title="Không thể tải thùng rác"
          message={loadError}
          onRetry={() => loadTrash()}
          showBackHome={false}
        />
      ) : trashedRecipes.length === 0 ? (
        <EmptyState
          compact
          className="w-full max-w-none"
          title="Thùng rác đang trống"
          description="Hiện không có công thức nào trong thùng rác."
          icon={<Trash2 className="size-6" aria-hidden="true" />}
          action={{
            label: 'Quản lý công thức',
            href: '/dashboard/recipes',
            icon: <BookOpen className="size-4" aria-hidden="true" />,
          }}
        />
      ) : (
        <Card className="overflow-hidden border-border/80 py-0 shadow-xs">
          <Table className="min-w-[45rem]" aria-label="Công thức đã xóa">
            <TableHeader>
              <TableRow>
                <TableHead className="w-[35%]">Công thức</TableHead>
                <TableHead>Danh mục</TableHead>
                <TableHead>Tác giả</TableHead>
                <TableHead>Thời gian xóa</TableHead>
                <TableHead className="text-right">Thao tác</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {trashedRecipes.map((recipe) => {
                const isBusy = actionInProgress === recipe.id;

                return (
                  <TableRow key={recipe.id} className="hover:bg-muted/30 transition-colors">
                    <TableCell>
                      <div className="space-y-1">
                        <span className="font-bold text-foreground/70 line-through block">
                          {recipe.title}
                        </span>
                        <span className="text-xs text-muted-foreground font-mono block">
                          slug: /{recipe.slug}
                        </span>
                      </div>
                    </TableCell>

                    <TableCell>
                      <span className="font-medium text-sm text-foreground">
                        {recipe.categoryName || 'Ẩm thực'}
                      </span>
                    </TableCell>

                    <TableCell>
                      <span className="text-sm text-muted-foreground">
                        {recipe.authorName || recipe.authorId || 'Ẩn danh'}
                      </span>
                    </TableCell>

                    <TableCell>
                      <div className="flex items-center gap-1.5 text-xs text-muted-foreground">
                        <Clock className="w-3.5 h-3.5" />
                        <span>
                          {recipe.deletedAt
                            ? new Date(recipe.deletedAt).toLocaleDateString('vi-VN', {
                                day: '2-digit',
                                month: '2-digit',
                                year: 'numeric',
                                hour: '2-digit',
                                minute: '2-digit',
                              })
                            : 'Gần đây'}
                        </span>
                      </div>
                    </TableCell>

                    <TableCell className="text-right">
                      <div className="flex items-center justify-end gap-2">
                        {/* Nút Khôi phục (Restore) */}
                        <Button
                          type="button"
                          variant="outline"
                          size="sm"
                          onClick={() => handleRestore(recipe)}
                          disabled={isBusy}
                          title="Khôi phục công thức về trạng thái Bản nháp"
                          className="h-8 gap-1.5 text-emerald-700 hover:text-emerald-800 hover:bg-emerald-50 border-emerald-200"
                        >
                          <RotateCcw className="w-3.5 h-3.5" />
                          <span>Khôi phục</span>
                        </Button>

                        {/* Nút Xóa vĩnh viễn (Purge) */}
                        <Button
                          type="button"
                          variant="destructive"
                          size="sm"
                          onClick={() => setPurgeConfirmState({ isOpen: true, recipe })}
                          disabled={isBusy}
                          title="Xóa vĩnh viễn khỏi Database"
                          className="h-8 gap-1.5 shadow-xs"
                        >
                          <Trash2 className="w-3.5 h-3.5" />
                          <span>Xóa vĩnh viễn</span>
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                );
              })}
            </TableBody>
          </Table>
        </Card>
      )}

      {/* PHÂN TRANG SERVER-SIDE */}
      {!loading && !loadError && trashedRecipes.length > 0 && totalPages > 1 && (
        <div className="flex flex-col sm:flex-row items-center justify-between gap-4 pt-2">
          <p className="text-sm text-muted-foreground">
            Hiển thị trang <span className="font-medium text-foreground">{page}</span> / <span className="font-medium text-foreground">{totalPages}</span> (Tổng cộng <span className="font-medium text-foreground">{totalCount}</span> công thức đã xóa)
          </p>
          <div className="flex items-center gap-2">
            <Button
              type="button"
              variant="outline"
              size="sm"
              disabled={page <= 1 || loading}
              onClick={() => {
                const prev = page - 1;
                setPage(prev);
                loadTrash(prev);
              }}
            >
              Trang trước
            </Button>
            <Button
              type="button"
              variant="outline"
              size="sm"
              disabled={page >= totalPages || loading}
              onClick={() => {
                const next = page + 1;
                setPage(next);
                loadTrash(next);
              }}
            >
              Trang sau
            </Button>
          </div>
        </div>
      )}

      {/* MODAL XÁC NHẬN XÓA VĨNH VIỄN */}
      <ConfirmDialog
        isOpen={purgeConfirmState.isOpen}
        title="Xác nhận Xóa Vĩnh Viễn Công Thức?"
        message={`Hành động này KHÔNG THỂ HOÀN TÁC! Toàn bộ nội dung, nguyên liệu, hình ảnh và lịch sử của bài viết "${purgeConfirmState.recipe?.title}" sẽ bị xóa hoàn toàn khỏi cơ sở dữ liệu.`}
        confirmLabel="Xóa vĩnh viễn ngay"
        cancelLabel="Hủy bỏ"
        isDestructive={true}
        onConfirm={executePurge}
        onCancel={() => setPurgeConfirmState({ isOpen: false })}
      />
    </div>
  );
}
