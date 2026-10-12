'use client';

import React, { useEffect, useState } from 'react';
import Link from 'next/link';
import {
  TrashIcon,
  RefreshCwIcon,
  AlertTriangleIcon,
  CheckCircleIcon,
  DashboardIcon,
  ClockIcon,
} from '@/components/common/Icons';
import { RecipeApi } from '@/lib/api-client';
import { RecipeSummaryDto } from '@/lib/types';
import { useToast } from '@/components/common/Toast';
import { ConfirmDialog } from '@/components/common/ConfirmDialog';

export default function AdminTrashManagementPage() {
  const { showSuccess, showError } = useToast();

  const [trashedRecipes, setTrashedRecipes] = useState<RecipeSummaryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [actionInProgress, setActionInProgress] = useState<string | null>(null);

  // State cho Purge Confirm Modal
  const [purgeConfirmState, setPurgeConfirmState] = useState<{
    isOpen: boolean;
    recipe?: RecipeSummaryDto;
  }>({ isOpen: false });

  const loadTrash = async () => {
    try {
      setLoading(true);
      const res = await RecipeApi.getTrashedRecipes(1, 50);
      setTrashedRecipes(res.items || []);
    } catch (err) {
      showError('Không thể tải danh sách thùng rác', (err as Error).message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadTrash();
  }, []);

  // 1. Khôi phục công thức (Restore)
  const handleRestore = async (recipe: RecipeSummaryDto) => {
    try {
      setActionInProgress(recipe.id);
      await RecipeApi.restoreRecipe(recipe.id);
      showSuccess(`Đã khôi phục "${recipe.title}"`, 'Công thức đã được đưa về trạng thái Bản nháp.');
      setTrashedRecipes(prev => prev.filter(r => r.id !== recipe.id));
    } catch (err) {
      showError('Khôi phục thất bại', (err as Error).message);
    } finally {
      setActionInProgress(null);
    }
  };

  // 2. Xóa vĩnh viễn (Purge)
  const executePurge = async () => {
    const recipe = purgeConfirmState.recipe;
    if (!recipe) return;

    try {
      setActionInProgress(recipe.id);
      setPurgeConfirmState({ isOpen: false });
      await RecipeApi.purgeRecipe(recipe.id);
      showSuccess(`Đã xóa vĩnh viễn "${recipe.title}"`, 'Dữ liệu đã được xóa hoàn toàn khỏi cơ sở dữ liệu.');
      setTrashedRecipes(prev => prev.filter(r => r.id !== recipe.id));
    } catch (err) {
      showError('Xóa vĩnh viễn thất bại', (err as Error).message);
    } finally {
      setActionInProgress(null);
    }
  };

  return (
    <>
      {/* HEADER */}
      <header className="dashboard-header">
        <div className="header-title-area">
          <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '4px' }}>
            <h1>Thùng rác Quản trị viên</h1>
            <span className="sidebar-badge" style={{ background: '#FEE2E2', color: '#B91C1C' }}>
              Admin Only
            </span>
          </div>
          <p>Quản lý các công thức đã bị xóa mềm: Phục hồi hoặc Xóa vĩnh viễn khỏi Database</p>
        </div>
        <div className="header-actions">
          <button
            onClick={loadTrash}
            className="btn btn-secondary btn-sm"
            title="Làm mới thùng rác"
            disabled={loading}
          >
            <RefreshCwIcon size={16} className={loading ? 'animate-spin' : ''} />
            <span>Làm mới</span>
          </button>
          <Link href="/dashboard" className="btn btn-secondary btn-sm">
            <DashboardIcon size={16} />
            <span>Về Dashboard</span>
          </Link>
        </div>
      </header>

      <main className="dashboard-content">
        {/* WARNING ALERT BANNER */}
        <div
          style={{
            background: '#FFFBEB',
            border: '1px solid #FDE68A',
            borderRadius: 'var(--radius-lg)',
            padding: '16px 20px',
            marginBottom: '24px',
            display: 'flex',
            alignItems: 'flex-start',
            gap: '14px',
          }}
        >
          <AlertTriangleIcon size={24} color="#D97706" style={{ flexShrink: 0, marginTop: '2px' }} />
          <div>
            <div style={{ fontWeight: 700, color: '#92400E', fontSize: '0.92rem' }}>
              Lưu ý về cơ chế Xóa mềm (Soft Delete & Purge)
            </div>
            <div style={{ fontSize: '0.82rem', color: '#78350F', marginTop: '4px', lineHeight: 1.5 }}>
              Các bài viết tại đây đang có cờ <code>IsDeleted = true</code> và bị ẩn khỏi toàn bộ API công khai.
              Hành động <strong>Khôi phục (Restore)</strong> sẽ đưa bài viết về trạng thái Bản nháp. Hành động <strong>Xóa vĩnh viễn (Purge)</strong> sẽ xóa cứng toàn bộ nguyên liệu, bước nấu và ảnh liên quan.
            </div>
          </div>
        </div>

        {/* TRASH TABLE */}
        <div className="section-card">
          <div className="section-header">
            <div>
              <h2 className="section-title">Danh sách bài viết trong thùng rác</h2>
              <p style={{ fontSize: '0.82rem', color: 'var(--text-muted)' }}>
                Hiện có {trashedRecipes.length} bài viết cần xử lý
              </p>
            </div>
          </div>

          <div className="table-responsive">
            <table className="custom-table">
              <thead>
                <tr>
                  <th>Tên công thức & Slug</th>
                  <th>Danh mục</th>
                  <th>Tác giả</th>
                  <th>Thời gian xóa</th>
                  <th style={{ textAlign: 'right' }}>Thao tác Quản trị</th>
                </tr>
              </thead>
              <tbody>
                {loading ? (
                  <tr>
                    <td colSpan={5} style={{ textAlign: 'center', padding: '40px' }}>
                      Đang tải danh sách thùng rác...
                    </td>
                  </tr>
                ) : trashedRecipes.length === 0 ? (
                  <tr>
                    <td colSpan={5}>
                      <div className="empty-state">
                        <div className="empty-icon-box" style={{ background: '#DCFCE7', color: '#15803D' }}>
                          <CheckCircleIcon size={32} />
                        </div>
                        <div className="empty-title">Thùng rác hoàn toàn trống!</div>
                        <div className="empty-desc">
                          Không có bài viết nào đang bị xóa mềm. Tất cả công thức đều đang hoạt động bình thường.
                        </div>
                      </div>
                    </td>
                  </tr>
                ) : (
                  trashedRecipes.map(recipe => {
                    const isBusy = actionInProgress === recipe.id;

                    return (
                      <tr key={recipe.id}>
                        <td>
                          <div className="recipe-title-cell">
                            <span className="recipe-main-title" style={{ textDecoration: 'line-through', color: 'var(--text-muted)' }}>
                              {recipe.title}
                            </span>
                            <span className="recipe-slug-sub">slug: /{recipe.slug}</span>
                          </div>
                        </td>
                        <td>
                          <span style={{ fontWeight: 600 }}>{recipe.categoryName || 'Ẩm thực'}</span>
                        </td>
                        <td>
                          <span style={{ fontSize: '0.85rem', color: 'var(--text-main)' }}>
                            {recipe.authorName || recipe.authorId}
                          </span>
                        </td>
                        <td>
                          <div style={{ display: 'flex', alignItems: 'center', gap: '6px', fontSize: '0.8rem', color: 'var(--text-muted)' }}>
                            <ClockIcon size={14} />
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
                        </td>
                        <td style={{ textAlign: 'right' }}>
                          <div className="table-actions" style={{ justifyContent: 'flex-end' }}>
                            {/* Nút Khôi phục (Restore) */}
                            <button
                              onClick={() => handleRestore(recipe)}
                              disabled={isBusy}
                              className="btn btn-secondary btn-sm"
                              title="Khôi phục về bản nháp (PUT /admin/recipes/{id}/restore)"
                            >
                              <RefreshCwIcon size={14} />
                              <span>Khôi phục</span>
                            </button>

                            {/* Nút Xóa vĩnh viễn (Purge) */}
                            <button
                              onClick={() => setPurgeConfirmState({ isOpen: true, recipe })}
                              disabled={isBusy}
                              className="btn btn-danger btn-sm"
                              title="Xóa vĩnh viễn khỏi Database (DELETE /admin/recipes/{id}/purge)"
                            >
                              <TrashIcon size={14} />
                              <span>Xóa vĩnh viễn</span>
                            </button>
                          </div>
                        </td>
                      </tr>
                    );
                  })
                )}
              </tbody>
            </table>
          </div>
        </div>
      </main>

      {/* CONFIRM PURGE DIALOG */}
      <ConfirmDialog
        isOpen={purgeConfirmState.isOpen}
        title="⚠️ Xóa Vĩnh Viễn Không Thể Phục Hồi?"
        message={`Bạn có chắc chắn muốn xóa vĩnh viễn công thức "${purgeConfirmState.recipe?.title}" không? Mọi thông tin nguyên liệu, các bước nấu và album ảnh sẽ bị xóa sạch khỏi cơ sở dữ liệu.`}
        confirmLabel="Xóa vĩnh viễn (Purge)"
        isDestructive={true}
        onConfirm={executePurge}
        onCancel={() => setPurgeConfirmState({ isOpen: false })}
      />
    </>
  );
}
