"use client";

import { useCallback, useEffect, useState } from "react";
import { ClipboardList } from "lucide-react";
import { ManagementPageHeader } from "@/components/layout/ManagementPageHeader";
import { EmptyState } from "@/components/common/EmptyState";
import { ErrorState } from "@/components/common/ErrorState";
import { Loading } from "@/components/common/Loading";
import { Button } from "@/components/ui/button";
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
    <div className="management-page">
      <ManagementPageHeader title="Nhật ký hệ thống" description="Theo dõi hoạt động API, lỗi và thao tác quản trị người dùng." />

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

      {loading ? (
        <Loading className="h-auto w-full py-8 text-center" text="Đang tải dữ liệu..." />
      ) : error ? (
        <ErrorState className="w-full max-w-none" title="Không thể tải nhật ký hệ thống" onRetry={() => loadLogs()} showBackHome={false} />
      ) : logs.length === 0 ? (
        <EmptyState
          compact
          className="w-full max-w-none"
          title="Không có nhật ký phù hợp"
          description="Thử thay đổi bộ lọc hoặc từ khóa tìm kiếm."
          icon={<ClipboardList className="size-6" aria-hidden="true" />}
        />
      ) : (
        <div className="admin-table-container">
          <table className="admin-table" aria-label="Nhật ký hệ thống">
            <thead><tr><th>Thời gian</th><th>Mức độ</th><th>Request</th><th>Trạng thái</th><th>Thời gian xử lý</th><th>IP</th></tr></thead>
            <tbody>
              {logs.map((log) => (
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
      )}
      {!loading && !error && (totalPages > 1 || page > 1) && (
        <nav className="flex flex-wrap items-center justify-between gap-2" aria-label="Phân trang nhật ký">
            <Button type="button" size="sm" variant="outline" disabled={page <= 1} onClick={() => setPage((value) => value - 1)}>Trang trước</Button>
            <span>Trang {page} / {totalPages}</span>
            <Button type="button" size="sm" variant="outline" disabled={page >= totalPages} onClick={() => setPage((value) => value + 1)}>Trang sau</Button>
        </nav>
      )}
    </div>
  );
}

export default function SystemLogsPage() {
  return <RequireRole role="Admin"><SystemLogsContent /></RequireRole>;
}
