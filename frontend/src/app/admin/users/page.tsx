"use client";

import { useCallback, useEffect, useState } from "react";
import { RequireRole } from "@/components/auth/RequireRole";
import { ConfirmActionDialog } from "@/components/auth/ConfirmActionDialog";
import { authApi } from "@/lib/auth/api";
import type { AdminUserDto } from "@/types/auth";

const PAGE_SIZE = 20;

function UsersContent() {
  const [users, setUsers] = useState<AdminUserDto[]>([]);
  const [search, setSearch] = useState("");
  const [role, setRole] = useState<"Admin" | "Author" | "">("");
  const [status, setStatus] = useState<"active" | "inactive" | "">("");
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selected, setSelected] = useState<AdminUserDto | null>(null);

  const loadUsers = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await authApi.listUsers({
        page,
        pageSize: PAGE_SIZE,
        search: search.trim() || undefined,
        role: role || undefined,
        status: status || undefined,
        sortBy: "createdAt",
      });
      setUsers(response.data);
      setTotalPages(Math.max(1, response.meta.totalPages));
    } catch {
      setError("Không thể tải danh sách người dùng.");
    } finally {
      setLoading(false);
    }
  }, [page, role, search, status]);

  useEffect(() => {
    const timer = window.setTimeout(loadUsers, 300);
    return () => window.clearTimeout(timer);
  }, [loadUsers]);

  async function confirmToggle() {
    if (!selected) return;
    try {
      await authApi.toggleUserStatus(selected.id, !selected.isActive);
      setSelected(null);
      await loadUsers();
    } catch {
      setError("Không thể cập nhật trạng thái tài khoản.");
    }
  }

  return (
    <main className="p-6 md:p-10">
      <header className="admin-header">
        <div>
          <p className="admin-badge">Quản trị</p>
          <h1>Danh sách người dùng</h1>
          <p>Quản lý tài khoản Admin và Tác giả.</p>
        </div>
      </header>

      <section className="admin-toolbar" aria-label="Bộ lọc người dùng">
        <input
          aria-label="Tìm kiếm theo email hoặc tên"
          placeholder="Tìm email hoặc tên..."
          value={search}
          onChange={(event) => {
            setSearch(event.target.value);
            setPage(1);
          }}
        />
        <select aria-label="Lọc vai trò" value={role} onChange={(event) => { setRole(event.target.value as typeof role); setPage(1); }}>
          <option value="">Tất cả vai trò</option>
          <option value="Admin">Admin</option>
          <option value="Author">Tác giả</option>
        </select>
        <select aria-label="Lọc trạng thái" value={status} onChange={(event) => { setStatus(event.target.value as typeof status); setPage(1); }}>
          <option value="">Tất cả trạng thái</option>
          <option value="active">Đang hoạt động</option>
          <option value="inactive">Đã khóa</option>
        </select>
      </section>

      {error && <p role="alert" className="my-4 text-red-600">{error}</p>}
      <div className="admin-table-container">
        <table className="admin-table">
          <thead><tr><th>Người dùng</th><th>Vai trò</th><th>Email</th><th>Trạng thái</th><th>Ngày tạo</th><th>Thao tác</th></tr></thead>
          <tbody>
            {loading ? <tr><td colSpan={6}>Đang tải...</td></tr> : users.length === 0 ? <tr><td colSpan={6}>Không có người dùng phù hợp.</td></tr> : users.map((user) => (
              <tr key={user.id}>
                <td>{user.displayName}</td>
                <td>{user.roles.join(", ")}</td>
                <td>{user.email}</td>
                <td>{user.isActive ? "Đang hoạt động" : "Đã khóa"}</td>
                <td>{new Date(user.createdAt).toLocaleDateString("vi-VN")}</td>
                <td><button type="button" onClick={() => setSelected(user)}>{user.isActive ? "Khóa" : "Mở khóa"}</button></td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <nav className="mt-5 flex items-center justify-between" aria-label="Phân trang">
        <button type="button" disabled={page <= 1} onClick={() => setPage((value) => value - 1)}>Trang trước</button>
        <span>Trang {page} / {totalPages}</span>
        <button type="button" disabled={page >= totalPages} onClick={() => setPage((value) => value + 1)}>Trang sau</button>
      </nav>
      <ConfirmActionDialog
        open={selected !== null}
        onOpenChange={(open) => { if (!open) setSelected(null); }}
        title={selected?.isActive ? "Khóa tài khoản?" : "Mở khóa tài khoản?"}
        description={`Bạn có chắc muốn ${selected?.isActive ? "khóa" : "mở khóa"} ${selected?.email ?? "tài khoản này"} không?`}
        confirmLabel={selected?.isActive ? "Khóa tài khoản" : "Mở khóa"}
        tone={selected?.isActive ? "danger" : "default"}
        onConfirm={confirmToggle}
      />
    </main>
  );
}

export default function AdminUsersPage() {
  return <RequireRole role="Admin"><UsersContent /></RequireRole>;
}
