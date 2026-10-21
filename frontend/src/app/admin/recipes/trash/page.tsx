'use client';

import React, { useEffect, useState } from 'react';
import Link from 'next/link';
import {
  Trash2,
  RefreshCw,
  Clock,
  RotateCcw,
  ShieldAlert,
  ArrowLeft,
  Sparkles,
} from 'lucide-react';
import { RecipeApi } from '@/lib/api-client';
import { RecipeSummaryDto } from '@/lib/types';
import { useToast } from '@/components/common/Toast';
import { ConfirmDialog } from '@/components/common/ConfirmDialog';
import { Loading } from '@/components/common/Loading';
import { EmptyState } from '@/components/common/EmptyState';
import { ErrorState } from '@/components/common/ErrorState';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent } from '@/components/ui/card';
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

  // State cho Purge Confirm Modal
  const [purgeConfirmState, setPurgeConfirmState] = useState<{
    isOpen: boolean;
    recipe?: RecipeSummaryDto;
  }>({ isOpen: false });

  const loadTrash = async () => {
    try {
      setLoading(true);
      setLoadError(null);
      const res = await RecipeApi.getTrashedRecipes(1, 50);
      setTrashedRecipes(res.items || []);
    } catch (err) {
      const msg = (err as Error).message || 'Không thể tải danh sách thùng rác.';
      setLoadError(msg);
      showError('Không thể tải danh sách thùng rác', msg);
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
      setTrashedRecipes((prev) => prev.filter((r) => r.id !== recipe.id));
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
      setTrashedRecipes((prev) => prev.filter((r) => r.id !== recipe.id));
    } catch (err) {
      showError('Xóa vĩnh viễn thất bại', (err as Error).message);
    } finally {
      setActionInProgress(null);
    }
  };

  return (
    <div className="space-y-8">
      {/* HEADER QUẢN TRỊ VIÊN */}
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 pb-6 border-b border-border">
        <div className="space-y-1">
          <div className="flex items-center gap-2.5">
            <h1 className="text-2xl md:text-3xl font-extrabold tracking-tight text-foreground">
              Thùng rác Quản trị viên
            </h1>
            <Badge variant="outline" className="bg-destructive/10 text-destructive border-destructive/30 font-semibold gap-1">
              <ShieldAlert className="w-3.5 h-3.5" />
              Admin Only
            </Badge>
          </div>
          <p className="text-sm text-muted-foreground">
            Quản lý các công thức đã bị xóa mềm: Phục hồi về Bản nháp hoặc Xóa vĩnh viễn khỏi Database
          </p>
        </div>

        <div className="flex items-center gap-3">
          <Button asChild variant="outline" size="sm" className="gap-2 shadow-xs">
            <Link href="/dashboard">
              <ArrowLeft className="w-4 h-4" />
              Bảng điều khiển
            </Link>
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={loadTrash}
            disabled={loading}
            className="gap-2 shadow-xs"
          >
            <RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
            Làm mới
          </Button>
        </div>
      </div>

      {/* XỬ LÝ LỖI TOÀN TRANG NẾU CÓ */}
      {loadError && !loading && (
        <ErrorState
          title="Không thể tải thùng rác"
          message={loadError}
          onRetry={loadTrash}
          retryLabel="Thử nạp lại"
          showBackHome={false}
        />
      )}

      {/* BẢNG DANH SÁCH BÀI VIẾT BỊ XÓA */}
      <Card className="border-border/80 shadow-xs overflow-hidden">
        <CardContent className="p-0">
          <Table>
            <TableHeader className="bg-muted/40">
              <TableRow>
                <TableHead className="w-[35%]">Tên công thức & Slug</TableHead>
                <TableHead>Danh mục</TableHead>
                <TableHead>Tác giả</TableHead>
                <TableHead>Thời gian xóa</TableHead>
                <TableHead className="text-right">Thao tác Quản trị</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {loading ? (
                <TableRow>
                  <TableCell colSpan={5} className="h-64 text-center">
                    <Loading size="lg" text="Đang tải danh sách bài viết trong thùng rác..." />
                  </TableCell>
                </TableRow>
              ) : trashedRecipes.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={5} className="h-72">
                    <EmptyState
                      title="Thùng rác hoàn toàn trống!"
                      description="Tuyệt vời! Không có bài viết nào đang bị xóa mềm. Tất cả công thức nấu ăn đều đang hoạt động ổn định."
                      icon={<Sparkles className="w-10 h-10 text-emerald-600" />}
                      action={{
                        label: 'Về quản lý công thức',
                        href: '/dashboard/recipes',
                      }}
                    />
                  </TableCell>
                </TableRow>
              ) : (
                trashedRecipes.map((recipe) => {
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
                })
              )}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

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
