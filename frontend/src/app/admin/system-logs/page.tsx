"use client";

import { useCallback, useEffect, useState } from "react";
import { RequireRole } from "@/components/auth/RequireRole";
import { authApi } from "@/lib/auth/api";
import type { SystemLogDto } from "@/types/auth";

const PAGE_SIZE = 25;

function levelLabel(level: SystemLogDto["level"]) {
  return level === "Error" ? "Lỗi" : level === "Warning" ? "Cảnh báo" : "Thông tin";
}

function statusClass(statusCode: number) {
  if (statusCode >= 500) return "text-red-700";
  if (statusCode >= 400) return "text-amber-700";
  return "text-emerald-700";
}

function SystemLogsContent() {
  const [logs, setLogs] = useState<SystemLogDto[]>([]);
  const [search, setSearch] = useState("");
  const [level, setLevel] = useState<"Information" | "Warning" | "Error" | "">("");
  const [statusCode, setStatusCode] = useState("");
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadLogs = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await authApi.listSystemLogs({
        page,
        pageSize: PAGE_SIZE,
        search: search.trim() || undefined,
        level: level || undefined,
        statusCode: statusCode ? Number(statusCode) : undefined,
      });
      setLogs(response.data);
      setTotalPages(Math.max(1, response.meta.totalPages));
    } catch {
      setError("Không thể tải nhật ký hệ thống.");
    } finally {
      setLoading(false);
    }
  }, [level, page, search, statusCode]);

  useEffect(() => {
    const timer = window.setTimeout(loadLogs, 300);
    return () => window.clearTimeout(timer);
  }, [loadLogs]);

  return (
    <main className="p-6 md:p-10">
      <header className="admin-header">
        <div>
          <p className="admin-badge">Giám sát hệ thống</p>
          <h1>Nhật ký hệ thống</h1>
          <p>Theo dõi hoạt động API, lỗi và thao tác quản trị người dùng.</p>
        </div>
      </header>

      <section className="admin-toolbar" aria-label="Bộ lọc nhật ký">
        <input
          aria-label="Tìm kiếm đường dẫn hoặc thông báo"
          placeholder="Tìm đường dẫn hoặc thông báo..."
          value={search}
          onChange={(event) => { setSearch(event.target.value); setPage(1); }}
        />
        <select aria-label="Lọc mức độ" value={level} onChange={(event) => { setLevel(event.target.value as typeof level); setPage(1); }}>
          <option value="">Tất cả mức độ</option>
          <option value="Information">Thông tin</option>
          <option value="Warning">Cảnh báo</option>
          <option value="Error">Lỗi</option>
        </select>
        <select aria-label="Lọc mã trạng thái" value={statusCode} onChange={(event) => { setStatusCode(event.target.value); setPage(1); }}>
          <option value="">Tất cả mã trạng thái</option>
          <option value="200">200 - Thành công</option>
          <option value="401">401 - Chưa đăng nhập</option>
          <option value="403">403 - Bị từ chối</option>
          <option value="404">404 - Không tìm thấy</option>
          <option value="500">500 - Lỗi máy chủ</option>
        </select>
      </section>

      {error && <p role="alert" className="my-4 text-red-600">{error}</p>}
      <div className="admin-table-container">
        <table className="admin-table">
          <thead><tr><th>Thời gian</th><th>Mức độ</th><th>Request</th><th>Trạng thái</th><th>Thời gian xử lý</th><th>IP</th></tr></thead>
          <tbody>
            {loading ? <tr><td colSpan={6}>Đang tải...</td></tr> : logs.length === 0 ? <tr><td colSpan={6}>Chưa có nhật ký phù hợp.</td></tr> : logs.map((log) => (
              <tr key={log.id}>
                <td>{new Date(log.createdAt).toLocaleString("vi-VN")}</td>
                <td>{levelLabel(log.level)}</td>
                <td><strong>{log.method}</strong> {log.path}</td>
                <td className={statusClass(log.statusCode)}>{log.statusCode}</td>
                <td>{log.durationMs} ms</td>
                <td>{log.ipAddress ?? "-"}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <nav className="mt-5 flex items-center justify-between" aria-label="Phân trang nhật ký">
        <button type="button" disabled={page <= 1} onClick={() => setPage((value) => value - 1)}>Trang trước</button>
        <span>Trang {page} / {totalPages}</span>
        <button type="button" disabled={page >= totalPages} onClick={() => setPage((value) => value + 1)}>Trang sau</button>
      </nav>
    </main>
  );
}

export default function SystemLogsPage() {
  return <RequireRole role="Admin"><SystemLogsContent /></RequireRole>;
}
