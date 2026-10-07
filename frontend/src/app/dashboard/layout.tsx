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

export default function DashboardLayout({ children }: { children: ReactNode }) {
  const pathname = usePathname();

  const isNavActive = (href: string) => {
    if (href === '/dashboard') {
      return pathname === '/dashboard';
    }
    return pathname.startsWith(href);
  };

  return (
    <ToastProvider>
      <div className="dashboard-wrapper">
        {/* SIDEBAR NAVIGATION */}
        <aside className="dashboard-sidebar">
          <div className="sidebar-brand">
            <div className="sidebar-logo-icon">
              <FlameIcon size={22} />
            </div>
            <div>
              <div className="sidebar-brand-title">Culinary Blog</div>
              <div className="sidebar-brand-subtitle">Studio & Quản trị</div>
            </div>
          </div>

          <nav className="sidebar-nav">
            <div className="sidebar-nav-heading">Tác giả (Author)</div>
            
            <Link
              href="/dashboard"
              className={`sidebar-link ${pathname === '/dashboard' ? 'active' : ''}`}
            >
              <DashboardIcon size={18} />
              <span>Tổng quan</span>
            </Link>

            <Link
              href="/dashboard/recipes"
              className={`sidebar-link ${isNavActive('/dashboard/recipes') && !pathname.includes('/new') ? 'active' : ''}`}
            >
              <BookOpenIcon size={18} />
              <span>Bài viết của tôi</span>
            </Link>

            <Link
              href="/dashboard/recipes/new"
              className={`sidebar-link ${pathname === '/dashboard/recipes/new' ? 'active' : ''}`}
            >
              <PlusCircleIcon size={18} />
              <span>Tạo công thức mới</span>
            </Link>

            <div className="sidebar-nav-heading">Hệ thống & Admin</div>

            <Link
              href="/dashboard/categories"
              className={`sidebar-link ${pathname.startsWith('/dashboard/categories') ? 'active' : ''}`}
            >
              <BookOpenIcon size={18} />
              <span>Quản lý Danh mục</span>
              <span className="sidebar-badge">Admin</span>
            </Link>

            <Link
              href="/admin/recipes/trash"
              className={`sidebar-link ${pathname.startsWith('/admin/recipes/trash') ? 'active' : ''}`}
            >
              <TrashIcon size={18} />
              <span>Thùng rác</span>
              <span className="sidebar-badge">Admin</span>
            </Link>

            <div className="sidebar-nav-heading">Khám phá</div>

            <Link href="/recipes" className="sidebar-link">
              <ArrowUpRightIcon size={18} />
              <span>Kho công thức</span>
            </Link>

            <Link href="/" className="sidebar-link">
              <ArrowUpRightIcon size={18} />
              <span>Xem trang web</span>
            </Link>
          </nav>

          <div className="sidebar-footer">
            <div className="user-profile-widget">
              <div className="user-avatar">TN</div>
              <div style={{ overflow: 'hidden' }}>
                <div className="user-info-name">Tạ Nhật Nguyên</div>
                <div className="user-info-role">Nhóm trưởng • Admin</div>
              </div>
            </div>
          </div>
        </aside>

        {/* MAIN CONTENT CONTAINER */}
        <div className="dashboard-main">{children}</div>
      </div>
    </ToastProvider>
  );
}
