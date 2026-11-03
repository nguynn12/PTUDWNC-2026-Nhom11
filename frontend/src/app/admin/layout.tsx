'use client';

import React, { ReactNode } from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import {
  DashboardIcon,
  BookOpenIcon,
  PlusCircleIcon,
  TrashIcon,
  ArrowUpRightIcon,
  FlameIcon,
} from '@/components/common/Icons';
import { ToastProvider } from '@/components/common/Toast';
import { RequireRole } from '@/components/auth/RequireRole';
import { useCurrentUser } from '@/store/auth-store';

export default function AdminLayout({ children }: { children: ReactNode }) {
  const pathname = usePathname();
  const user = useCurrentUser();

  return (
    <ToastProvider>
      <RequireRole role="Admin">
        <div className="dashboard-wrapper">
        {/* SIDEBAR NAVIGATION */}
        <aside className="dashboard-sidebar">
          <div className="sidebar-brand">
            <div className="sidebar-logo-icon">
              <FlameIcon size={22} />
            </div>
            <div>
              <div className="sidebar-brand-title">Culinary Blog</div>
              <div className="sidebar-brand-subtitle">Admin Hub</div>
            </div>
          </div>

          <nav className="sidebar-nav">
            <div className="sidebar-nav-heading">Quản trị Hệ thống</div>
            
            <Link
              href="/dashboard"
              className="sidebar-link"
            >
              <DashboardIcon size={18} />
              <span>Về Dashboard</span>
            </Link>

            <Link
              href="/dashboard/recipes"
              className="sidebar-link"
            >
              <BookOpenIcon size={18} />
              <span>Bài viết của tôi</span>
            </Link>

            <div className="sidebar-nav-heading">Nội dung</div>
            <Link
              href="/admin/recipes/trash"
              className={`sidebar-link ${pathname.startsWith('/admin/recipes/trash') ? 'active' : ''}`}
            >
              <TrashIcon size={18} />
              <span>Thùng rác công thức</span>
              <span className="sidebar-badge">Admin</span>
            </Link>

            <div className="sidebar-nav-heading">Quản lý người dùng</div>
            <Link
              href="/admin/users"
              className={`sidebar-link ${pathname.startsWith('/admin/users') ? 'active' : ''}`}
            >
              <span aria-hidden="true">👥</span>
              <span>Người dùng</span>
              <span className="sidebar-badge">Admin</span>
            </Link>

            <div className="sidebar-nav-heading">Giám sát hệ thống</div>
            <Link
              href="/admin/system-logs"
              className={`sidebar-link ${pathname.startsWith('/admin/system-logs') ? 'active' : ''}`}
            >
              <span aria-hidden="true">📝</span>
              <span>Nhật ký hệ thống</span>
              <span className="sidebar-badge">Admin</span>
            </Link>

            <div className="sidebar-nav-heading">Liên kết</div>

            <Link href="/" className="sidebar-link" target="_blank">
              <ArrowUpRightIcon size={18} />
              <span>Xem trang web chính</span>
            </Link>
          </nav>

          <div className="sidebar-footer">
            <div className="user-profile-widget">
              <div className="user-avatar">{(user?.displayName ?? 'U').slice(0, 2).toUpperCase()}</div>
              <div style={{ overflow: 'hidden' }}>
                  <div className="user-info-name">{user?.displayName ?? 'Quản trị viên'}</div>
                  <div className="user-info-role">Quản trị viên</div>
              </div>
            </div>
          </div>
        </aside>

        {/* MAIN CONTENT CONTAINER */}
          <div className="dashboard-main">{children}</div>
        </div>
      </RequireRole>
    </ToastProvider>
  );
}
