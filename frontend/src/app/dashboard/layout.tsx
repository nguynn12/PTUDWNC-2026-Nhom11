'use client';

import React, { ReactNode, useRef, useState, useSyncExternalStore } from 'react';
import { Menu, X } from 'lucide-react';
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
import { useCurrentUser, useHasRole } from '@/store/auth-store';

function subscribeToViewport(onChange: () => void) {
  const media = window.matchMedia('(max-width: 900px)');
  media.addEventListener('change', onChange);
  return () => media.removeEventListener('change', onChange);
}

export default function DashboardLayout({ children }: { children: ReactNode }) {
  const pathname = usePathname();
  const user = useCurrentUser();
  const isMobile = useSyncExternalStore(subscribeToViewport, () => window.matchMedia('(max-width: 900px)').matches, () => false);
  const [sidebarOverride, setSidebarOverride] = useState<boolean | null>(null);
  const sidebarOpen = sidebarOverride ?? !isMobile;
  const toggleRef = useRef<HTMLButtonElement>(null);
  const closeSidebar = () => {
    setSidebarOverride(false);
    toggleRef.current?.focus();
  };
  const isAdmin = useHasRole('Admin');

  const isNavActive = (href: string) => {
    if (href === '/dashboard') {
      return pathname === '/dashboard';
    }
    return pathname.startsWith(href);
  };

  return (
    <ToastProvider>
      <RequireRole role="Author">
        <div className="dashboard-wrapper">
        {isMobile && sidebarOpen && <button className="sidebar-backdrop" aria-label="Đóng menu quản trị" onClick={closeSidebar} />}
        {/* SIDEBAR NAVIGATION */}
        <aside id="dashboard-sidebar" className="dashboard-sidebar" hidden={!sidebarOpen} onKeyDown={(event) => { if (event.key === 'Escape') closeSidebar(); }}>
          <div className="sidebar-brand">
            <div className="sidebar-logo-icon">
              <FlameIcon size={22} />
            </div>
            <div>
              <div className="sidebar-brand-title">Culinary Blog</div>
              <div className="sidebar-brand-subtitle">Studio & Quản trị</div>
            </div>
            <button type="button" className="sidebar-close" onClick={closeSidebar} aria-label="Đóng thanh điều hướng" title="Đóng thanh điều hướng"><X size={20} /></button>
          </div>

          <nav className="sidebar-nav" aria-label="Điều hướng quản trị" onClick={(event) => { if (isMobile && (event.target as HTMLElement).closest('a')) closeSidebar(); }}>
            <div className="sidebar-nav-heading">{isAdmin ? 'Quản trị & Tác giả' : 'Không gian Tác giả'}</div>
            
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
              <span>Công thức của tôi</span>
            </Link>

            <Link
              href="/dashboard/recipes/new"
              className={`sidebar-link ${pathname === '/dashboard/recipes/new' ? 'active' : ''}`}
            >
              <PlusCircleIcon size={18} />
              <span>Tạo công thức mới</span>
            </Link>

            {isAdmin && (
              <>
                <div className="sidebar-nav-heading">Quản trị hệ thống</div>
                <Link
                  href="/dashboard/categories"
                  className={`sidebar-link ${pathname.startsWith('/dashboard/categories') ? 'active' : ''}`}
                >
                  <BookOpenIcon size={18} />
                  <span>Danh mục món ăn</span>
                  <span className="sidebar-badge">Admin</span>
                </Link>
                <Link
                  href="/admin/recipes/trash"
                  className={`sidebar-link ${pathname.startsWith('/admin/recipes/trash') ? 'active' : ''}`}
                >
                  <TrashIcon size={18} />
                  <span>Thùng rác công thức</span>
                  <span className="sidebar-badge">Admin</span>
                </Link>
              </>
            )}

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
              <div className="user-avatar">{(user?.displayName ?? 'U').slice(0, 2).toUpperCase()}</div>
              <div style={{ overflow: 'hidden' }}>
                <div className="user-info-name">{user?.displayName ?? 'Người dùng'}</div>
                <div className="user-info-role">{isAdmin ? 'Quản trị viên' : 'Tác giả'}</div>
              </div>
            </div>
          </div>
        </aside>

        {/* MAIN CONTENT CONTAINER */}
        <div className="dashboard-main">
          <div className="dashboard-toolbar">
            <button ref={toggleRef} type="button" className="btn btn-secondary btn-sm" aria-expanded={sidebarOpen} aria-controls="dashboard-sidebar" onClick={() => setSidebarOverride(!sidebarOpen)}>
              <Menu size={18} /><span>{sidebarOpen ? 'Ẩn menu' : 'Mở menu quản trị'}</span>
            </button>
          </div>
          {children}
        </div>
        </div>
      </RequireRole>
    </ToastProvider>
  );
}
