'use client';

import React, { useEffect, useState } from 'react';
import Link from 'next/link';
import {
  BookOpenIcon,
  PlusCircleIcon,
  TrashIcon,
  ArchiveIcon,
  CheckCircleIcon,
  SparklesIcon,
  ArrowUpRightIcon,
  ClockIcon,
  UsersIcon,
  RefreshCwIcon,
} from '@/components/common/Icons';
import { RecipeApi } from '@/lib/api-client';
import { DashboardMetrics, RecipeSummaryDto } from '@/lib/types';
import { ManagementPageHeader } from '@/components/layout/ManagementPageHeader';
import { EmptyState } from '@/components/common/EmptyState';
import { ErrorState } from '@/components/common/ErrorState';
import { Loading } from '@/components/common/Loading';
import { useCurrentUser, useHasRole } from '@/store/auth-store';

export default function DashboardOverviewPage() {
  const user = useCurrentUser();
  const isAdmin = useHasRole('Admin');
  const [metrics, setMetrics] = useState<DashboardMetrics>({
    totalRecipes: 0,
    publishedCount: 0,
    draftCount: 0,
    archivedCount: 0,
    trashCount: 0,
  });
  const [recentRecipes, setRecentRecipes] = useState<RecipeSummaryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);

  const loadData = async () => {
    try {
      setLoading(true);
      setLoadError(null);
      const [m, rec] = await Promise.all([
        RecipeApi.getDashboardMetrics(isAdmin),
        RecipeApi.getMyRecipes({ pageSize: 5 }),
      ]);
      setMetrics(m);
      setRecentRecipes(rec.items || []);
    } catch (err) {
      setLoadError((err as Error).message || 'Vui lòng thử lại.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const renderStatusBadge = (status: string | number) => {
    const s = status.toString().toLowerCase();
    if (s === 'published' || s === '1') {
      return (
        <span className="status-badge badge-published">
          <span className="status-dot"></span>
          Đã xuất bản
        </span>
      );
    }
    if (s === 'archived' || s === '2') {
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
    <div className="management-page">
      <ManagementPageHeader
        title="Tổng quan"
        description={isAdmin ? 'Thống kê công thức của bạn và thùng rác toàn hệ thống.' : 'Theo dõi công thức của bạn theo trạng thái.'}
        actions={
          <>
            <button
              onClick={loadData}
              className="btn btn-secondary btn-sm"
              title="Làm mới dữ liệu"
              disabled={loading}
            >
              <RefreshCwIcon size={16} className={loading ? 'animate-spin' : ''} />
              <span>Làm mới</span>
            </button>
            <Link href="/dashboard/recipes/new" className="btn btn-primary btn-sm">
              <PlusCircleIcon size={16} />
              <span>Tạo công thức mới</span>
            </Link>
          </>
        }
      />

      {loadError && !loading ? (
        <ErrorState className="w-full max-w-none" title="Không thể tải dữ liệu tổng quan" message={loadError} onRetry={() => loadData()} showBackHome={false} />
      ) : (
        <div className="metrics-grid">
          <div className="metric-card">
            <div className="metric-icon-box metric-icon-primary">
              <BookOpenIcon size={24} />
            </div>

            <div>
              <div className="metric-value">{loading ? '...' : metrics.totalRecipes}</div>
              <div className="metric-label">Tổng công thức của bạn</div>
            </div>
          </div>

          <div className="metric-card">
            <div className="metric-icon-box metric-icon-success">
              <CheckCircleIcon size={24} />
            </div>
            <div>
              <div className="metric-value">{loading ? '...' : metrics.publishedCount}</div>
              <div className="metric-label">Đã xuất bản (Công khai)</div>
            </div>
          </div>

          <div className="metric-card">
            <div className="metric-icon-box">
              <SparklesIcon size={24} />
            </div>
            <div>
              <div className="metric-value">{loading ? '...' : metrics.draftCount}</div>
              <div className="metric-label">Bản nháp (Đang viết)</div>
            </div>
          </div>

          <div className="metric-card">
            <div className="metric-icon-box metric-icon-warning">
              <ArchiveIcon size={24} />
            </div>
            <div>
              <div className="metric-value">{loading ? '...' : metrics.archivedCount}</div>
              <div className="metric-label">Đã lưu trữ (Ẩn)</div>
            </div>
          </div>

          {isAdmin && <div className="metric-card">
            <div className="metric-icon-box metric-icon-danger">
              <TrashIcon size={24} />
            </div>
            <div>
              <div className="metric-value">{loading ? '...' : metrics.trashCount}</div>
              <div className="metric-label">Thùng rác (Admin)</div>
            </div>
          </div>}
        </div>
      )}

      <section className="section-card dashboard-guide">
        <div className="section-header">
          <div>
            <h2 className="section-title">Từ tác giả đến món ăn</h2>
            <p className="action-card-desc">{user?.displayName ?? 'Tác giả'} tạo công thức, gắn vào danh mục và chia sẻ món ăn với cộng đồng.</p>
          </div>
        </div>
        <div className="quick-actions-grid">
          <Link href="/dashboard/recipes/new" className="action-card">
            <div className="action-card-title">1. Tạo công thức</div>
            <div className="action-card-desc">Nhập tên món ăn, mô tả, nguyên liệu, các bước làm và hình ảnh.</div>
          </Link>
          <Link href="/categories" className="action-card">
            <div className="action-card-title">2. Chọn danh mục món ăn</div>
            <div className="action-card-desc">Mỗi công thức thuộc một danh mục để người đọc dễ khám phá.</div>
          </Link>
          <Link href="/recipes" className="action-card">
            <div className="action-card-title">3. Xem món ăn công khai</div>
            <div className="action-card-desc">Công thức đã xuất bản sẽ xuất hiện trong kho món ăn và trang danh mục.</div>
          </Link>
        </div>
      </section>

      {/* QUICK ACTIONS SECTION */}
      <section>
        <h2 className="section-title mb-3">Lối tắt hành động nhanh</h2>
        <div className="quick-actions-grid">
          <Link href="/dashboard/recipes/new" className="action-card">
            <div className="action-card-header">
              <div className="metric-icon-box metric-icon-primary">
                <PlusCircleIcon size={22} />
              </div>
              <ArrowUpRightIcon size={18} color="#A8A29E" />
            </div>
            <div className="action-card-title">Tạo công thức mới</div>
            <div className="action-card-desc">
              Nhập thông tin, nguyên liệu, các bước nấu và ảnh món ăn.
            </div>
          </Link>

          <Link href="/dashboard/recipes" className="action-card">
            <div className="action-card-header">
              <div className="metric-icon-box metric-icon-success">
                <BookOpenIcon size={22} />
              </div>
              <ArrowUpRightIcon size={18} color="#A8A29E" />
            </div>
            <div className="action-card-title">Công thức của tôi</div>
            <div className="action-card-desc">
              Xem toàn bộ danh sách công thức cá nhân, chuyển đổi trạng thái Xuất bản / Lưu trữ / Xóa mềm.
            </div>
          </Link>

          {isAdmin && (
            <Link href="/dashboard/categories" className="action-card">
              <div className="action-card-header">
                <div className="metric-icon-box metric-icon-warning">
                  <BookOpenIcon size={22} />
                </div>
                <ArrowUpRightIcon size={18} color="#A8A29E" />
              </div>
              <div className="action-card-title">Quản lý danh mục món ăn</div>
              <div className="action-card-desc">
                Tạo và sắp xếp danh mục; số lượng công thức được liên kết trực tiếp từ dữ liệu thực tế.
              </div>
            </Link>
          )}

          {isAdmin && <Link href="/admin/recipes/trash" className="action-card">
            <div className="action-card-header">
              <div className="metric-icon-box metric-icon-danger">
                <TrashIcon size={22} />
              </div>
              <ArrowUpRightIcon size={18} color="#A8A29E" />
            </div>
            <div className="action-card-title">Thùng rác công thức</div>
            <div className="action-card-desc">
              Khôi phục hoặc xóa vĩnh viễn các công thức trong thùng rác.
            </div>
          </Link>}
        </div>
      </section>

      {/* RECENT RECIPES SECTION */}
      {!loadError && (
        <section className="section-card">
            <div className="section-header">
              <div>
                <h2 className="section-title">Công thức gần đây của bạn</h2>
                <p style={{ fontSize: '0.82rem', color: 'var(--text-muted)' }}>
                  Các món ăn được cập nhật hoặc chỉnh sửa mới nhất
                </p>
              </div>
              <Link href="/dashboard/recipes" className="btn btn-secondary btn-sm">
                <span>Xem tất cả</span>
                <ArrowUpRightIcon size={14} />
              </Link>
            </div>

            {loading ? (
              <Loading className="h-auto w-full py-8 text-center" text="Đang tải công thức gần đây..." />
            ) : recentRecipes.length === 0 ? (
              <EmptyState compact className="mx-4 my-4 max-w-none" title="Chưa có công thức nào" description="Tạo công thức đầu tiên để bắt đầu." />
            ) : (
            <div className="table-responsive">
              <table className="custom-table">
                <thead>
                  <tr>
                    <th>Tên công thức</th>
                    <th>Danh mục</th>
                    <th>Trạng thái</th>
                    <th>Thời gian & Khẩu phần</th>
                    <th>Concurrency Token</th>
                    <th style={{ textAlign: 'right' }}>Thao tác</th>
                  </tr>
                </thead>
                <tbody>
                  {recentRecipes.map(recipe => (
                      <tr key={recipe.id}>
                        <td>
                          <div className="recipe-title-cell">
                            <span className="recipe-main-title">{recipe.title}</span>
                            <span className="recipe-slug-sub">/{recipe.slug}</span>
                          </div>
                        </td>
                        <td>
                          <span style={{ fontWeight: 600, color: 'var(--primary-dark)' }}>
                            {recipe.categorySlug ? (
                              <Link href={`/categories/${recipe.categorySlug}`} className="hover:underline">
                                {recipe.categoryName || 'Ẩm thực'}
                              </Link>
                            ) : (
                              recipe.categoryName || 'Ẩm thực'
                            )}
                          </span>
                        </td>
                        <td>{renderStatusBadge(recipe.status || 'Draft')}</td>
                        <td>
                          <div style={{ display: 'flex', alignItems: 'center', gap: '12px', fontSize: '0.8rem', color: 'var(--text-muted)' }}>
                            <span style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                              <ClockIcon size={14} />
                              {(recipe.prepTimeMinutes ?? recipe.prepTime ?? 0) + (recipe.cookTimeMinutes ?? recipe.cookTime ?? 0)} phút
                            </span>
                            <span style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                              <UsersIcon size={14} />
                              {recipe.servings} người
                            </span>
                          </div>
                        </td>
                        <td>
                          <code style={{ background: '#F5F5F4', padding: '2px 6px', borderRadius: '4px', fontSize: '0.78rem' }}>
                            {recipe.xmin ? `"${recipe.xmin}"` : '—'}
                          </code>
                        </td>
                        <td style={{ textAlign: 'right' }}>
                          <Link
                            href={`/dashboard/recipes`}
                            className="btn btn-secondary btn-sm"
                          >
                            Quản lý
                          </Link>
                        </td>
                      </tr>
                    ))}
                </tbody>
              </table>
            </div>
            )}
        </section>
      )}
    </div>
  );
}
