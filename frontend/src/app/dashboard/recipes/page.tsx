'use client';

import React, { useEffect, useState, useMemo } from 'react';
import Link from 'next/link';
import {
  PlusCircleIcon,
  TrashIcon,
  ArchiveIcon,
  CheckCircleIcon,
  RefreshCwIcon,
  SearchIcon,
  ClockIcon,
  UsersIcon,
  AlertTriangleIcon,
  SparklesIcon,
  EditIcon,
} from '@/components/common/Icons';
import { RecipeApi, ApiError } from '@/lib/api-client';
import { RecipeSummaryDto } from '@/lib/types';
import { useToast } from '@/components/common/Toast';
import { ConflictDialog } from '@/components/common/ConflictDialog';
import { ConfirmDialog } from '@/components/common/ConfirmDialog';

type StatusFilter = 'ALL' | 'DRAFT' | 'PUBLISHED' | 'ARCHIVED';

export default function MyRecipesLifecyclePage() {
  const { showSuccess, showError, showWarning } = useToast();

  const [recipes, setRecipes] = useState<RecipeSummaryDto[]>([]);
  const [loading, setLoading] = useState(true);
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
      const res = await RecipeApi.getMyRecipes({ pageSize: 50 });
      setRecipes(res.items || []);
    } catch (err) {
      showError('Không thể tải danh sách công thức', (err as Error).message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadRecipes();
  }, []);

  // Lọc bài viết theo Tab và Tìm kiếm
  const filteredRecipes = useMemo(() => {
    return recipes.filter(item => {
      const s = item.status.toString().toUpperCase();
      let matchTab = true;
      if (activeFilter === 'DRAFT') matchTab = s === 'DRAFT' || s === '0';
      else if (activeFilter === 'PUBLISHED') matchTab = s === 'PUBLISHED' || s === '1';
      else if (activeFilter === 'ARCHIVED') matchTab = s === 'ARCHIVED' || s === '2';

      const matchSearch =
        !searchQuery ||
        item.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
        item.description.toLowerCase().includes(searchQuery.toLowerCase());

      return matchTab && matchSearch;
    });
  }, [recipes, activeFilter, searchQuery]);

  // Xử lý chung các lỗi đặc thù (409 Conflict)
  const handleLifecycleError = (err: unknown, recipe: RecipeSummaryDto) => {
    if (err instanceof ApiError && err.status === 409) {
      // Kích hoạt Conflict Modal theo chuẩn PLAN_03
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
      showSuccess(`Đã xuất bản "${recipe.title}"`, 'Công thức hiện đã hiển thị công khai.');
      // Cập nhật trạng thái trong mảng và token mới
      setRecipes(prev =>
        prev.map(r => (r.id === recipe.id ? { ...r, ...updated, status: 'Published' } : r))
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
      showSuccess(`Đã lưu trữ "${recipe.title}"`, 'Công thức đã được ẩn khỏi danh sách công khai.');
      setRecipes(prev =>
        prev.map(r => (r.id === recipe.id ? { ...r, ...updated, status: 'Archived' } : r))
      );
    } catch (err) {
      handleLifecycleError(err, recipe);
    } finally {
      setActionInProgress(null);
    }
  };

  // 3. Bỏ lưu trữ công thức (Unarchive)
  const handleUnarchive = async (recipe: RecipeSummaryDto) => {
    try {
      setActionInProgress(recipe.id);
      const updated = await RecipeApi.unarchiveRecipe(recipe.id, recipe.xmin);
      showSuccess(`Đã chuyển "${recipe.title}" về Bản nháp`, 'Bạn có thể chỉnh sửa và xuất bản lại.');
      setRecipes(prev =>
        prev.map(r => (r.id === recipe.id ? { ...r, ...updated, status: 'Draft' } : r))
      );
    } catch (err) {
      handleLifecycleError(err, recipe);
    } finally {
      setActionInProgress(null);
    }
  };

  // 4. Xóa mềm vào thùng rác (Soft Delete)
  const executeDelete = async () => {
    const recipe = deleteConfirmState.recipe;
    if (!recipe) return;

    try {
      setActionInProgress(recipe.id);
      setDeleteConfirmState({ isOpen: false });
      await RecipeApi.deleteRecipe(recipe.id, recipe.xmin);
      showSuccess(`Đã chuyển "${recipe.title}" vào thùng rác`, 'Quản trị viên có thể khôi phục khi cần.');
      setRecipes(prev => prev.filter(r => r.id !== recipe.id));
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
        <span className="status-badge badge-published">
          <span className="status-dot"></span>
          Đã xuất bản
        </span>
      );
    }
    if (s === 'ARCHIVED' || s === '2') {
      return (
        <span className="status-badge badge-archived">
          <span className="status-dot"></span>
          Đã lưu trữ
        </span>
      );
    }
    return (
      <span className="status-badge badge-draft">
        <span className="status-dot"></span>
        Bản nháp
      </span>
    );
  };

  return (
    <>
      {/* HEADER */}
      <header className="dashboard-header">
        <div className="header-title-area">
          <h1>Quản lý Bài viết của tôi</h1>
          <p>Điều khiển trạng thái vòng đời công thức (Bản nháp ➔ Xuất bản ➔ Lưu trữ ➔ Xóa mềm)</p>
        </div>
        <div className="header-actions">
          <button
            onClick={loadRecipes}
            className="btn btn-secondary btn-sm"
            title="Làm mới danh sách"
            disabled={loading}
          >
            <RefreshCwIcon size={16} className={loading ? 'animate-spin' : ''} />
            <span>Đồng bộ</span>
          </button>
          <Link href="/dashboard/recipes/new" className="btn btn-primary btn-sm">
            <PlusCircleIcon size={16} />
            <span>Tạo công thức mới</span>
          </Link>
        </div>
      </header>

      <main className="dashboard-content">
        {/* FILTER & SEARCH BAR */}
        <div className="filter-bar">
          <div className="tab-buttons">
            <button
              onClick={() => setActiveFilter('ALL')}
              className={`tab-btn ${activeFilter === 'ALL' ? 'active' : ''}`}
            >
              Tất cả ({recipes.length})
            </button>
            <button
              onClick={() => setActiveFilter('PUBLISHED')}
              className={`tab-btn ${activeFilter === 'PUBLISHED' ? 'active' : ''}`}
            >
              Đã xuất bản ({recipes.filter(r => r.status.toString().toUpperCase() === 'PUBLISHED' || r.status === 1).length})
            </button>
            <button
              onClick={() => setActiveFilter('DRAFT')}
              className={`tab-btn ${activeFilter === 'DRAFT' ? 'active' : ''}`}
            >
              Bản nháp ({recipes.filter(r => r.status.toString().toUpperCase() === 'DRAFT' || r.status === 0).length})
            </button>
            <button
              onClick={() => setActiveFilter('ARCHIVED')}
              className={`tab-btn ${activeFilter === 'ARCHIVED' ? 'active' : ''}`}
            >
              Đã lưu trữ ({recipes.filter(r => r.status.toString().toUpperCase() === 'ARCHIVED' || r.status === 2).length})
            </button>
          </div>

          <div className="search-input-box">
            <SearchIcon size={16} />
            <input
              type="text"
              placeholder="Tìm kiếm theo tên hoặc mô tả..."
              value={searchQuery}
              onChange={e => setSearchQuery(e.target.value)}
            />
          </div>
        </div>

        {/* RECIPES TABLE */}
        <div className="section-card">
          <div className="table-responsive">
            <table className="custom-table">
              <thead>
                <tr>
                  <th>Tên công thức & URL</th>
                  <th>Danh mục</th>
                  <th>Trạng thái Vòng đời</th>
                  <th>Thông số nấu</th>
                  <th>xmin (Token)</th>
                  <th style={{ textAlign: 'right' }}>Hành động Vòng đời</th>
                </tr>
              </thead>
              <tbody>
                {loading ? (
                  <tr>
                    <td colSpan={6} style={{ textAlign: 'center', padding: '40px' }}>
                      Đang tải danh sách bài viết...
                    </td>
                  </tr>
                ) : filteredRecipes.length === 0 ? (
                  <tr>
                    <td colSpan={6}>
                      <div className="empty-state">
                        <div className="empty-icon-box">
                          <AlertTriangleIcon size={28} />
                        </div>
                        <div className="empty-title">Không tìm thấy công thức nào</div>
                        <div className="empty-desc">
                          Thử thay đổi bộ lọc trạng thái hoặc từ khóa tìm kiếm.
                        </div>
                      </div>
                    </td>
                  </tr>
                ) : (
                  filteredRecipes.map(recipe => {
                    const statusStr = recipe.status.toString().toUpperCase();
                    const isBusy = actionInProgress === recipe.id;

                    return (
                      <tr key={recipe.id}>
                        <td>
                          <div className="recipe-title-cell">
                            <span className="recipe-main-title">{recipe.title}</span>
                            <span className="recipe-slug-sub">slug: /{recipe.slug}</span>
                          </div>
                        </td>
                        <td>
                          <span style={{ fontWeight: 600, color: 'var(--primary)' }}>
                            {recipe.categoryName || 'Món ngon'}
                          </span>
                        </td>
                        <td>{renderStatusBadge(recipe.status)}</td>
                        <td>
                          <div style={{ display: 'flex', flexDirection: 'column', gap: '2px', fontSize: '0.8rem', color: 'var(--text-muted)' }}>
                            <span style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                              <ClockIcon size={13} />
                              {recipe.prepTimeMinutes + recipe.cookTimeMinutes} phút (Nấu {recipe.cookTimeMinutes}p)
                            </span>
                            <span style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                              <UsersIcon size={13} />
                              {recipe.servings} người ăn
                            </span>
                          </div>
                        </td>
                        <td>
                          <code
                            style={{
                              background: '#F5F5F4',
                              padding: '3px 8px',
                              borderRadius: '4px',
                              fontSize: '0.78rem',
                              border: '1px solid #E7E5E4',
                            }}
                            title="PostgreSQL xmin Concurrency Token"
                          >
                            {recipe.xmin ? `"${recipe.xmin}"` : '—'}
                          </code>
                        </td>
                        <td style={{ textAlign: 'right' }}>
                          <div className="table-actions" style={{ justifyContent: 'flex-end' }}>
                            {/* Nút Chỉnh sửa toàn diện (Wizard 4 bước của TV4) */}
                            <Link
                              href={`/dashboard/recipes/${recipe.id}/edit`}
                              className="btn btn-secondary btn-sm"
                              title="Chỉnh sửa toàn diện công thức (Nguyên liệu, Bước làm, Ảnh MinIO)"
                            >
                              <EditIcon size={14} />
                              <span>Sửa</span>
                            </Link>

                            {/* Nút Xuất bản (Dành cho Draft & Archived) */}
                            {(statusStr === 'DRAFT' || statusStr === '0') && (
                              <button
                                onClick={() => handlePublish(recipe)}
                                disabled={isBusy}
                                className="btn btn-primary btn-sm"
                                title="Xuất bản công thức công khai (PUT /recipes/{id}/publish)"
                              >
                                <CheckCircleIcon size={14} />
                                <span>Xuất bản</span>
                              </button>
                            )}

                            {/* Nút Lưu trữ (Dành cho Published) */}
                            {(statusStr === 'PUBLISHED' || statusStr === '1') && (
                              <button
                                onClick={() => handleArchive(recipe)}
                                disabled={isBusy}
                                className="btn btn-secondary btn-sm"
                                title="Ẩn bài viết vào kho lưu trữ (PUT /recipes/{id}/archive)"
                              >
                                <ArchiveIcon size={14} />
                                <span>Lưu trữ</span>
                              </button>
                            )}

                            {/* Nút Bỏ lưu trữ (Dành cho Archived) */}
                            {(statusStr === 'ARCHIVED' || statusStr === '2') && (
                              <button
                                onClick={() => handleUnarchive(recipe)}
                                disabled={isBusy}
                                className="btn btn-secondary btn-sm"
                                title="Chuyển về bản nháp để chỉnh sửa (PUT /recipes/{id}/unarchive)"
                              >
                                <SparklesIcon size={14} />
                                <span>Bản nháp</span>
                              </button>
                            )}

                            {/* Nút Xóa mềm vào Thùng rác */}
                            <button
                              onClick={() => setDeleteConfirmState({ isOpen: true, recipe })}
                              disabled={isBusy}
                              className="btn btn-icon-only"
                              title="Chuyển vào thùng rác (DELETE /recipes/{id})"
                            >
                              <TrashIcon size={16} />
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

      {/* 409 CONCURRENCY CONFLICT MODAL */}
      <ConflictDialog
        isOpen={conflictState.isOpen}
        recipeTitle={conflictState.recipeTitle}
        errorMessage={conflictState.errorMessage}
        currentXmin={conflictState.currentXmin}
        onReload={loadRecipes}
        onClose={() => setConflictState({ isOpen: false })}
      />

      {/* CONFIRM DELETE MODAL */}
      <ConfirmDialog
        isOpen={deleteConfirmState.isOpen}
        title="Chuyển vào thùng rác?"
        message={`Bạn có chắc muốn chuyển công thức "${deleteConfirmState.recipe?.title}" vào thùng rác không? Quản trị viên có thể phục hồi lại sau.`}
        confirmLabel="Chuyển vào thùng rác"
        isDestructive={true}
        onConfirm={executeDelete}
        onCancel={() => setDeleteConfirmState({ isOpen: false })}
      />
    </>
  );
}
