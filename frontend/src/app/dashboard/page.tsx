'use client';

import React, { useEffect, useState } from 'react';
import Link from 'next/link';
import {
  BookOpenIcon,
  PlusCircleIcon,
  TrashIcon,
  ArchiveIcon,
  CheckCircleIcon,
  FlameIcon,
  SparklesIcon,
  ArrowUpRightIcon,
  ClockIcon,
  UsersIcon,
  RefreshCwIcon,
} from '@/components/common/Icons';
import { RecipeApi } from '@/lib/api-client';
import { DashboardMetrics, RecipeSummaryDto } from '@/lib/types';
import { useToast } from '@/components/common/Toast';

export default function DashboardOverviewPage() {
  const { showError } = useToast();
  const [metrics, setMetrics] = useState<DashboardMetrics>({
    totalRecipes: 0,
    publishedCount: 0,
    draftCount: 0,
    archivedCount: 0,
    trashCount: 0,
  });
  const [recentRecipes, setRecentRecipes] = useState<RecipeSummaryDto[]>([]);
  const [loading, setLoading] = useState(true);

  const loadData = async () => {
    try {
      setLoading(true);
      const [m, rec] = await Promise.all([
        RecipeApi.getDashboardMetrics(),
        RecipeApi.getMyRecipes({ pageSize: 5 }),
      ]);
      setMetrics(m);
      setRecentRecipes(rec.items || []);
    } catch (err) {
      showError('Không thể tải dữ liệu tổng quan', (err as Error).message);
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
    <>
      {/* HEADER */}
      <header className="dashboard-header">
        <div className="header-title-area">
          <h1>Tổng quan Studio</h1>
          <p>Bảng điều khiển và theo dõi trạng thái công thức của bạn</p>
        </div>
        <div className="header-actions">
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
        </div>
      </header>

      <main className="dashboard-content">
        {/* STATS METRICS GRID */}
        <div className="metrics-grid">
          <div className="metric-card">
            <div className="metric-icon-box metric-icon-primary">
              <BookOpenIcon size={24} />
            </div>
            <div>
              <div className="metric-value">{loading ? '...' : metrics.totalRecipes}</div>
              <div className="metric-label">Tổng số công thức</div>
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

          <div className="metric-card">
            <div className="metric-icon-box metric-icon-danger">
              <TrashIcon size={24} />
            </div>
            <div>
              <div className="metric-value">{loading ? '...' : metrics.trashCount}</div>
              <div className="metric-label">Thùng rác (Admin)</div>
            </div>
          </div>
        </div>

        {/* QUICK ACTIONS SECTION */}
        <h2 className="section-title" style={{ marginBottom: '16px' }}>
          Lối tắt hành động nhanh
        </h2>
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
              Khởi tạo món ăn mới với Multi-step Wizard, thêm nguyên liệu, các bước nấu và tải ảnh.
            </div>
          </Link>

          <Link href="/dashboard/recipes" className="action-card">
            <div className="action-card-header">
              <div className="metric-icon-box metric-icon-success">
                <BookOpenIcon size={22} />
              </div>
              <ArrowUpRightIcon size={18} color="#A8A29E" />
            </div>
            <div className="action-card-title">Quản lý bài viết của tôi</div>
            <div className="action-card-desc">
              Xem toàn bộ danh sách công thức cá nhân, chuyển đổi trạng thái Xuất bản / Lưu trữ / Xóa mềm.
            </div>
          </Link>

          <Link href="/admin/recipes/trash" className="action-card">
            <div className="action-card-header">
              <div className="metric-icon-box metric-icon-danger">
                <TrashIcon size={22} />
              </div>
              <ArrowUpRightIcon size={18} color="#A8A29E" />
            </div>
            <div className="action-card-title">Quản trị Thùng rác</div>
            <div className="action-card-desc">
              Phục hồi các công thức đã xóa nhầm hoặc xóa vĩnh viễn (Purge) giải phóng bộ nhớ.
            </div>
          </Link>
        </div>

        {/* RECENT RECIPES SECTION */}
        <div className="section-card">
          <div className="section-header">
            <div>
              <h2 className="section-title">Công thức gần đây</h2>
              <p style={{ fontSize: '0.82rem', color: 'var(--text-muted)' }}>
                Các món ăn được cập nhật hoặc chỉnh sửa mới nhất
              </p>
            </div>
            <Link href="/dashboard/recipes" className="btn btn-secondary btn-sm">
              <span>Xem tất cả</span>
              <ArrowUpRightIcon size={14} />
            </Link>
          </div>

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
                {loading ? (
                  <tr>
                    <td colSpan={6} style={{ textAlign: 'center', padding: '32px' }}>
                      Đang tải dữ liệu...
                    </td>
                  </tr>
                ) : recentRecipes.length === 0 ? (
                  <tr>
                    <td colSpan={6} style={{ textAlign: 'center', padding: '32px', color: 'var(--text-muted)' }}>
                      Chưa có công thức nào. Hãy bắt đầu bằng cách tạo công thức đầu tiên!
                    </td>
                  </tr>
                ) : (
                  recentRecipes.map(recipe => (
                    <tr key={recipe.id}>
                      <td>
                        <div className="recipe-title-cell">
                          <span className="recipe-main-title">{recipe.title}</span>
                          <span className="recipe-slug-sub">/{recipe.slug}</span>
                        </div>
                      </td>
                      <td>
                        <span style={{ fontWeight: 600, color: 'var(--primary-dark)' }}>
                          {recipe.categoryName || 'Ẩm thực'}
                        </span>
                      </td>
                      <td>{renderStatusBadge(recipe.status)}</td>
                      <td>
                        <div style={{ display: 'flex', alignItems: 'center', gap: '12px', fontSize: '0.8rem', color: 'var(--text-muted)' }}>
                          <span style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                            <ClockIcon size={14} />
                            {recipe.prepTimeMinutes + recipe.cookTimeMinutes} phút
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
                  ))
                )}
              </tbody>
            </table>
          </div>
        </div>
      </main>
    </>
  );
}
