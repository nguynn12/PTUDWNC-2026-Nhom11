"use client";

import React, { useState, useMemo } from "react";
import Link from "next/link";
import Image from "next/image";
import {
  useCategories,
  useCreateCategory,
  useUpdateCategory,
  useDeleteCategory,
} from "@/hooks/useCategories";
import type { CategoryDto, CreateCategoryRequest, UpdateCategoryRequest, AppError } from "@/types/api";

// ----------------------------------------------------------------------------
// HELPER: Slugify tiếng Việt chuẩn SEO (Preview thời gian thực)
// ----------------------------------------------------------------------------
function generateSlug(text: string): string {
  return text
    .toLowerCase()
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/[đĐ]/g, "d")
    .replace(/[^a-z0-9\s-]/g, "")
    .trim()
    .replace(/\s+/g, "-")
    .replace(/-+/g, "-");
}

export default function AdminCategoriesPage() {
  // Queries & Mutations
  const { data: categories = [], isLoading, isError, error, refetch } = useCategories();
  const createMutation = useCreateCategory();
  const updateMutation = useUpdateCategory();
  const deleteMutation = useDeleteCategory();

  // Local State: Tìm kiếm & Sắp xếp
  const [searchTerm, setSearchTerm] = useState("");
  const [sortBy, setSortBy] = useState<"orderIndex" | "name" | "recipeCount">("orderIndex");

  // Local State: Modals
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [editingCategory, setEditingCategory] = useState<CategoryDto | null>(null);
  const [deletingCategory, setDeletingCategory] = useState<CategoryDto | null>(null);

  // Local State: Toast Notifications
  const [toast, setToast] = useState<{
    id: number;
    message: string;
    type: "success" | "error" | "info";
  } | null>(null);

  const showToast = (message: string, type: "success" | "error" | "info" = "success") => {
    const id = Date.now();
    setToast({ id, message, type });
    setTimeout(() => {
      setToast((current) => (current?.id === id ? null : current));
    }, 4000);
  };

  // Lọc và Sắp xếp Danh mục trên Client
  const filteredCategories = useMemo(() => {
    let result = [...categories];

    if (searchTerm.trim()) {
      const q = searchTerm.toLowerCase().trim();
      result = result.filter(
        (c) =>
          c.name.toLowerCase().includes(q) ||
          c.slug.toLowerCase().includes(q) ||
          (c.description && c.description.toLowerCase().includes(q))
      );
    }

    result.sort((a, b) => {
      if (sortBy === "orderIndex") {
        return a.orderIndex - b.orderIndex || a.name.localeCompare(b.name, "vi");
      }
      if (sortBy === "recipeCount") {
        return b.recipeCount - a.recipeCount;
      }
      return a.name.localeCompare(b.name, "vi");
    });

    return result;
  }, [categories, searchTerm, sortBy]);

  // Thống kê Metrics
  const totalCategories = categories.length;
  const activeCategoriesWithRecipes = categories.filter((c: CategoryDto) => c.recipeCount > 0).length;
  const totalRecipesLinked = categories.reduce((sum: number, c: CategoryDto) => sum + c.recipeCount, 0);

  return (
    <div className="container">
      {/* Toast Notification */}
      {toast && (
        <div className="toast-container">
          <div className={`toast-item toast-${toast.type}`}>
            <span>
              {toast.type === "success" && "✅"}
              {toast.type === "error" && "⚠️"}
              {toast.type === "info" && "ℹ️"}
            </span>
            <span>{toast.message}</span>
            <button
              onClick={() => setToast(null)}
              style={{
                background: "transparent",
                border: "none",
                color: "inherit",
                cursor: "pointer",
                marginLeft: "auto",
                fontWeight: "bold",
              }}
              aria-label="Đóng thông báo"
            >
              ✕
            </button>
          </div>
        </div>
      )}

      {/* Header & Tiêu đề trang */}
      <div style={{ marginBottom: "28px" }}>
        <div style={{ display: "flex", alignItems: "center", gap: "8px", marginBottom: "8px" }}>
          <Link href="/" style={{ color: "var(--text-tertiary)", fontSize: "0.85rem", textDecoration: "none" }}>
            Trang chủ
          </Link>
          <span style={{ color: "var(--text-tertiary)", fontSize: "0.85rem" }}>/</span>
          <span style={{ color: "var(--text-tertiary)", fontSize: "0.85rem" }}>Bảng điều khiển</span>
          <span style={{ color: "var(--text-tertiary)", fontSize: "0.85rem" }}>/</span>
          <span style={{ color: "var(--primary)", fontSize: "0.85rem", fontWeight: 600 }}>Quản lý Danh mục</span>
        </div>
        <div style={{ display: "flex", flexWrap: "wrap", alignItems: "center", justifyContent: "space-between", gap: "16px" }}>
          <div>
            <h1 style={{ fontSize: "1.85rem", fontWeight: 800, color: "var(--text-primary)", marginBottom: "4px" }}>
              Quản trị Danh mục Món ăn
            </h1>
            <p style={{ color: "var(--text-secondary)", fontSize: "0.95rem" }}>
              Quản lý danh sách, cấu hình thứ tự hiển thị và duy trì các chuyên mục ẩm thực (FR-CAT-001 đến FR-CAT-005).
            </p>
          </div>
          <button
            onClick={() => setIsCreateOpen(true)}
            className="btn btn-primary"
            style={{ display: "inline-flex", alignItems: "center", gap: "8px", fontWeight: 600 }}
          >
            <span>➕</span>
            <span>Thêm danh mục mới</span>
          </button>
        </div>
      </div>

      {/* Metrics Grid */}
      <div className="metrics-grid">
        <div className="metric-card">
          <div className="metric-icon-box" style={{ backgroundColor: "hsla(24, 80%, 50%, 0.12)", color: "var(--primary)" }}>
            📁
          </div>
          <div>
            <div className="metric-value">{totalCategories}</div>
            <div className="metric-label">Tổng số danh mục</div>
          </div>
        </div>

        <div className="metric-card">
          <div className="metric-icon-box" style={{ backgroundColor: "hsla(145, 60%, 45%, 0.12)", color: "var(--success)" }}>
            🍲
          </div>
          <div>
            <div className="metric-value">{activeCategoriesWithRecipes}</div>
            <div className="metric-label">Danh mục có bài viết</div>
          </div>
        </div>

        <div className="metric-card">
          <div className="metric-icon-box" style={{ backgroundColor: "hsla(210, 80%, 55%, 0.12)", color: "#2563eb" }}>
            🔗
          </div>
          <div>
            <div className="metric-value">{totalRecipesLinked}</div>
            <div className="metric-label">Tổng liên kết công thức</div>
          </div>
        </div>
      </div>

      {/* Toolbar: Tìm kiếm & Sắp xếp */}
      <div className="admin-toolbar">
        <div style={{ position: "relative", flex: 1, maxWidth: "420px" }}>
          <span
            style={{
              position: "absolute",
              left: "14px",
              top: "50%",
              transform: "translateY(-50%)",
              color: "var(--text-tertiary)",
              pointerEvents: "none",
            }}
          >
            🔍
          </span>
          <input
            type="text"
            className="input-search"
            placeholder="Lọc nhanh danh mục theo tên hoặc slug..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            style={{ paddingLeft: "42px", height: "42px", fontSize: "0.9rem" }}
          />
          {searchTerm && (
            <button
              onClick={() => setSearchTerm("")}
              style={{
                position: "absolute",
                right: "12px",
                top: "50%",
                transform: "translateY(-50%)",
                background: "transparent",
                border: "none",
                color: "var(--text-tertiary)",
                cursor: "pointer",
              }}
            >
              ✕
            </button>
          )}
        </div>

        <div style={{ display: "flex", alignItems: "center", gap: "12px" }}>
          <label htmlFor="sortSelect" style={{ fontSize: "0.85rem", color: "var(--text-secondary)", fontWeight: 600 }}>
            Sắp xếp theo:
          </label>
          <select
            id="sortSelect"
            value={sortBy}
            onChange={(e) => setSortBy(e.target.value as "orderIndex" | "name" | "recipeCount")}
            className="input-select"
            style={{ height: "42px", minWidth: "180px", fontSize: "0.9rem" }}
          >
            <option value="orderIndex">Thứ tự hiển thị (Mặc định)</option>
            <option value="name">Tên danh mục (A - Z)</option>
            <option value="recipeCount">Số lượng công thức (Nhiều nhất)</option>
          </select>
        </div>
      </div>

      {/* Main Data Table */}
      {isLoading ? (
        <div className="admin-table-container" style={{ padding: "32px", textAlign: "center" }}>
          <div className="skeleton" style={{ height: "48px", marginBottom: "16px" }} />
          <div className="skeleton" style={{ height: "48px", marginBottom: "16px" }} />
          <div className="skeleton" style={{ height: "48px", marginBottom: "16px" }} />
          <div className="skeleton" style={{ height: "48px" }} />
        </div>
      ) : isError ? (
        <div
          className="admin-table-container"
          style={{ padding: "48px 24px", textAlign: "center", color: "var(--danger)" }}
        >
          <div style={{ fontSize: "2.5rem", marginBottom: "12px" }}>⚠️</div>
          <h3 style={{ marginBottom: "8px" }}>Không thể nạp dữ liệu danh mục</h3>
          <p style={{ color: "var(--text-secondary)", marginBottom: "20px" }}>
            {error?.detail || error?.message || "Đã xảy ra sự cố khi kết nối tới máy chủ API."}
          </p>
          <button onClick={() => refetch()} className="btn btn-outline btn-sm">
            🔄 Tải lại dữ liệu
          </button>
        </div>
      ) : filteredCategories.length === 0 ? (
        <div
          className="admin-table-container"
          style={{ padding: "64px 24px", textAlign: "center" }}
        >
          <div style={{ fontSize: "3rem", marginBottom: "16px" }}>📁</div>
          <h3 style={{ marginBottom: "8px", color: "var(--text-primary)" }}>
            {searchTerm ? "Không tìm thấy danh mục phù hợp" : "Chưa có danh mục nào"}
          </h3>
          <p style={{ color: "var(--text-secondary)", marginBottom: "24px" }}>
            {searchTerm
              ? `Không có kết quả nào khớp với từ khóa "${searchTerm}". Vui lòng thử từ khóa khác.`
              : "Bắt đầu thiết lập hệ thống chuyên mục ẩm thực bằng cách tạo danh mục đầu tiên."}
          </p>
          {searchTerm ? (
            <button onClick={() => setSearchTerm("")} className="btn btn-outline btn-sm">
              Xóa bộ lọc tìm kiếm
            </button>
          ) : (
            <button onClick={() => setIsCreateOpen(true)} className="btn btn-primary btn-sm">
              ➕ Tạo danh mục đầu tiên
            </button>
          )}
        </div>
      ) : (
        <div className="admin-table-container">
          <table className="admin-table">
            <thead>
              <tr>
                <th style={{ width: "70px", textAlign: "center" }}>Ảnh</th>
                <th>Tên danh mục</th>
                <th>Đường dẫn (Slug)</th>
                <th style={{ width: "100px", textAlign: "center" }}>Thứ tự</th>
                <th style={{ width: "120px", textAlign: "center" }}>Công thức</th>
                <th style={{ width: "160px", textAlign: "right" }}>Thao tác</th>
              </tr>
            </thead>
            <tbody>
              {filteredCategories.map((cat) => (
                <tr key={cat.id}>
                  {/* Thumbnail */}
                  <td style={{ textAlign: "center" }}>
                    <div
                      style={{
                        width: "44px",
                        height: "44px",
                        borderRadius: "var(--radius-md)",
                        overflow: "hidden",
                        backgroundColor: "var(--bg-surface-alt)",
                        margin: "0 auto",
                        display: "flex",
                        alignItems: "center",
                        justifyContent: "center",
                        position: "relative",
                      }}
                    >
                      {cat.imageUrl ? (
                        <Image
                          src={cat.imageUrl}
                          alt={cat.name}
                          fill
                          sizes="44px"
                          style={{ objectFit: "cover" }}
                        />
                      ) : (
                        <span style={{ fontSize: "1.25rem" }}>🍽️</span>
                      )}
                    </div>
                  </td>

                  {/* Tên & Mô tả */}
                  <td>
                    <div style={{ fontWeight: 700, color: "var(--text-primary)", marginBottom: "2px" }}>
                      {cat.name}
                    </div>
                    {cat.description ? (
                      <div
                        style={{
                          fontSize: "0.825rem",
                          color: "var(--text-secondary)",
                          maxWidth: "360px",
                          whiteSpace: "nowrap",
                          overflow: "hidden",
                          textOverflow: "ellipsis",
                        }}
                      >
                        {cat.description}
                      </div>
                    ) : (
                      <span style={{ fontSize: "0.8rem", color: "var(--text-tertiary)", fontStyle: "italic" }}>
                        Chưa có mô tả
                      </span>
                    )}
                  </td>

                  {/* Slug */}
                  <td>
                    <div style={{ display: "inline-flex", alignItems: "center", gap: "6px" }}>
                      <code
                        style={{
                          fontSize: "0.825rem",
                          backgroundColor: "var(--bg-surface-alt)",
                          padding: "2px 8px",
                          borderRadius: "var(--radius-sm)",
                          color: "var(--primary)",
                          fontFamily: "monospace",
                        }}
                      >
                        {cat.slug}
                      </code>
                      <Link
                        href={`/categories/${cat.slug}`}
                        target="_blank"
                        title="Xem trang công khai"
                        style={{
                          color: "var(--text-tertiary)",
                          textDecoration: "none",
                          fontSize: "0.85rem",
                        }}
                      >
                        ↗
                      </Link>
                    </div>
                  </td>

                  {/* Thứ tự hiển thị */}
                  <td style={{ textAlign: "center" }}>
                    <span
                      style={{
                        display: "inline-block",
                        padding: "2px 8px",
                        backgroundColor: "var(--bg-surface-alt)",
                        borderRadius: "var(--radius-sm)",
                        fontWeight: 600,
                        fontSize: "0.85rem",
                      }}
                    >
                      {cat.orderIndex}
                    </span>
                  </td>

                  {/* Số bài viết */}
                  <td style={{ textAlign: "center" }}>
                    <span
                      style={{
                        display: "inline-flex",
                        alignItems: "center",
                        gap: "4px",
                        padding: "4px 10px",
                        borderRadius: "var(--radius-full)",
                        fontSize: "0.8rem",
                        fontWeight: 700,
                        backgroundColor:
                          cat.recipeCount > 0 ? "hsla(145, 60%, 45%, 0.12)" : "var(--bg-surface-alt)",
                        color: cat.recipeCount > 0 ? "var(--success)" : "var(--text-tertiary)",
                      }}
                    >
                      {cat.recipeCount} bài
                    </span>
                  </td>

                  {/* Actions */}
                  <td style={{ textAlign: "right", whiteSpace: "nowrap" }}>
                    <div style={{ display: "inline-flex", alignItems: "center", gap: "8px" }}>
                      <button
                        onClick={() => setEditingCategory(cat)}
                        className="btn btn-outline btn-sm"
                        style={{ padding: "6px 12px", fontSize: "0.8rem" }}
                        title="Chỉnh sửa thông tin danh mục (FR-CAT-004)"
                      >
                        ✏️ Sửa
                      </button>
                      <button
                        onClick={() => setDeletingCategory(cat)}
                        className="btn btn-outline btn-sm"
                        style={{
                          padding: "6px 12px",
                          fontSize: "0.8rem",
                          color: "var(--danger)",
                          borderColor: "hsla(0, 70%, 55%, 0.3)",
                        }}
                        title="Xóa danh mục (FR-CAT-005)"
                      >
                        🗑️ Xóa
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* ====================================================================
          MODAL 1: THÊM MỚI DANH MỤC (CreateCategoryModal)
          ==================================================================== */}
      {isCreateOpen && (
        <CreateCategoryModal
          onClose={() => setIsCreateOpen(false)}
          onSuccess={(newCat) => {
            setIsCreateOpen(false);
            showToast(`Đã tạo thành công danh mục "${newCat.name}"!`, "success");
          }}
          onError={(msg) => {
            showToast(msg, "error");
          }}
          createMutation={createMutation}
        />
      )}

      {/* ====================================================================
          MODAL 2: CHỈNH SỬA DANH MỤC (EditCategoryModal)
          ==================================================================== */}
      {editingCategory && (
        <EditCategoryModal
          category={editingCategory}
          onClose={() => setEditingCategory(null)}
          onSuccess={(updatedCat) => {
            setEditingCategory(null);
            showToast(`Đã cập nhật danh mục "${updatedCat.name}"!`, "success");
          }}
          onError={(msg) => {
            showToast(msg, "error");
          }}
          updateMutation={updateMutation}
        />
      )}

      {/* ====================================================================
          MODAL 3: XÓA AN TOÀN DANH MỤC (DeleteCategoryModal)
          ==================================================================== */}
      {deletingCategory && (
        <DeleteCategoryModal
          category={deletingCategory}
          onClose={() => setDeletingCategory(null)}
          onSuccess={() => {
            const name = deletingCategory.name;
            setDeletingCategory(null);
            showToast(`Đã xóa danh mục "${name}" thành công!`, "success");
          }}
          onError={(msg) => {
            showToast(msg, "error");
          }}
          deleteMutation={deleteMutation}
        />
      )}
    </div>
  );
}

// ============================================================================
// SUB-COMPONENT: CreateCategoryModal
// ============================================================================
interface CreateCategoryModalProps {
  onClose: () => void;
  onSuccess: (created: CategoryDto) => void;
  onError: (errorMessage: string) => void;
  createMutation: ReturnType<typeof useCreateCategory>;
}

function CreateCategoryModal({
  onClose,
  onSuccess,
  onError,
  createMutation,
}: CreateCategoryModalProps) {
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [imageUrl, setImageUrl] = useState("");
  const [orderIndex, setOrderIndex] = useState(0);
  const [clientError, setClientError] = useState("");

  const slugPreview = generateSlug(name);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setClientError("");

    if (!name.trim()) {
      setClientError("Vui lòng nhập tên danh mục.");
      return;
    }
    if (name.trim().length < 2 || name.trim().length > 50) {
      setClientError("Tên danh mục phải có độ dài từ 2 đến 50 ký tự theo quy định SRS.");
      return;
    }

    const payload: CreateCategoryRequest = {
      name: name.trim(),
      description: description.trim() || undefined,
      imageUrl: imageUrl.trim() || undefined,
      orderIndex: Number(orderIndex) || 0,
    };

    createMutation.mutate(payload, {
      onSuccess: (data: CategoryDto) => {
        onSuccess(data);
      },
      onError: (err: AppError) => {
        onError(err.detail || err.message || "Tạo danh mục thất bại.");
      },
    });
  };

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal-dialog" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2 className="modal-title">Thêm Danh Mục Mới</h2>
          <button onClick={onClose} className="modal-close-btn" aria-label="Đóng">
            ✕
          </button>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="modal-body" style={{ display: "flex", flexDirection: "column", gap: "16px" }}>
            {clientError && (
              <div
                style={{
                  padding: "10px 14px",
                  backgroundColor: "hsla(0, 70%, 55%, 0.12)",
                  color: "var(--danger)",
                  borderRadius: "var(--radius-sm)",
                  fontSize: "0.85rem",
                  fontWeight: 600,
                }}
              >
                ⚠️ {clientError}
              </div>
            )}

            {/* Tên danh mục */}
            <div>
              <label className="form-label" style={{ display: "block", marginBottom: "6px", fontWeight: 600, fontSize: "0.875rem" }}>
                Tên danh mục <span style={{ color: "var(--danger)" }}>*</span>
              </label>
              <input
                type="text"
                className="input-search"
                style={{ height: "42px", fontSize: "0.95rem" }}
                placeholder="VD: Món Tráng Miệng, Hải Sản Tươi..."
                value={name}
                onChange={(e) => setName(e.target.value)}
                maxLength={50}
                required
                autoFocus
              />
              {/* Preview Slug */}
              <div style={{ marginTop: "6px", fontSize: "0.8rem", color: "var(--text-tertiary)" }}>
                Slug SEO dự kiến:{" "}
                <code style={{ color: "var(--primary)", fontWeight: 600 }}>
                  {slugPreview || "..."}
                </code>
              </div>
            </div>

            {/* Mô tả */}
            <div>
              <label className="form-label" style={{ display: "block", marginBottom: "6px", fontWeight: 600, fontSize: "0.875rem" }}>
                Mô tả giới thiệu
              </label>
              <textarea
                className="input-search"
                style={{ height: "80px", padding: "10px 14px", resize: "vertical", fontSize: "0.9rem" }}
                placeholder="Mô tả tóm tắt nội dung các công thức trong danh mục..."
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                maxLength={500}
              />
              <div style={{ textAlign: "right", fontSize: "0.75rem", color: "var(--text-tertiary)" }}>
                {description.length}/500 ký tự
              </div>
            </div>

            {/* Ảnh đại diện URL */}
            <div>
              <label className="form-label" style={{ display: "block", marginBottom: "6px", fontWeight: 600, fontSize: "0.875rem" }}>
                Đường dẫn hình ảnh (Image URL)
              </label>
              <input
                type="url"
                className="input-search"
                style={{ height: "42px", fontSize: "0.9rem" }}
                placeholder="https://images.unsplash.com/... hoặc MinIO S3 URL"
                value={imageUrl}
                onChange={(e) => setImageUrl(e.target.value)}
              />
              {imageUrl && (
                <div style={{ marginTop: "8px", display: "flex", alignItems: "center", gap: "10px" }}>
                  <span style={{ fontSize: "0.75rem", color: "var(--text-secondary)" }}>Xem trước:</span>
                  <div
                    style={{
                      width: "36px",
                      height: "36px",
                      borderRadius: "var(--radius-sm)",
                      overflow: "hidden",
                      position: "relative",
                    }}
                  >
                    <Image src={imageUrl} alt="Preview" fill sizes="36px" style={{ objectFit: "cover" }} />
                  </div>
                </div>
              )}
            </div>

            {/* Thứ tự hiển thị */}
            <div>
              <label className="form-label" style={{ display: "block", marginBottom: "6px", fontWeight: 600, fontSize: "0.875rem" }}>
                Thứ tự sắp xếp (Order Index)
              </label>
              <input
                type="number"
                className="input-search"
                style={{ height: "42px", width: "120px", fontSize: "0.95rem" }}
                value={orderIndex}
                onChange={(e) => setOrderIndex(parseInt(e.target.value, 10) || 0)}
                min={0}
                max={9999}
              />
              <span style={{ marginLeft: "10px", fontSize: "0.8rem", color: "var(--text-tertiary)" }}>
                Số nhỏ hơn sẽ được ưu tiên hiển thị trước.
              </span>
            </div>
          </div>

          <div className="modal-footer">
            <button type="button" onClick={onClose} className="btn btn-outline btn-sm">
              Hủy bỏ
            </button>
            <button
              type="submit"
              disabled={createMutation.isPending}
              className="btn btn-primary btn-sm"
              style={{ minWidth: "120px" }}
            >
              {createMutation.isPending ? "Đang xử lý..." : "➕ Tạo danh mục"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

// ============================================================================
// SUB-COMPONENT: EditCategoryModal (SRS C7: Slug bất biến)
// ============================================================================
interface EditCategoryModalProps {
  category: CategoryDto;
  onClose: () => void;
  onSuccess: (updated: CategoryDto) => void;
  onError: (errorMessage: string) => void;
  updateMutation: ReturnType<typeof useUpdateCategory>;
}

function EditCategoryModal({
  category,
  onClose,
  onSuccess,
  onError,
  updateMutation,
}: EditCategoryModalProps) {
  const [name, setName] = useState(category.name);
  const [description, setDescription] = useState(category.description || "");
  const [imageUrl, setImageUrl] = useState(category.imageUrl || "");
  const [orderIndex, setOrderIndex] = useState(category.orderIndex);
  const [clientError, setClientError] = useState("");

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setClientError("");

    if (!name.trim()) {
      setClientError("Vui lòng nhập tên danh mục.");
      return;
    }
    if (name.trim().length < 2 || name.trim().length > 50) {
      setClientError("Tên danh mục phải có độ dài từ 2 đến 50 ký tự.");
      return;
    }

    const payload: UpdateCategoryRequest = {
      name: name.trim(),
      description: description.trim() || undefined,
      imageUrl: imageUrl.trim() || undefined,
      orderIndex: Number(orderIndex) || 0,
    };

    updateMutation.mutate(
      { id: category.id, data: payload },
      {
        onSuccess: (data: CategoryDto) => {
          onSuccess(data);
        },
        onError: (err: AppError) => {
          onError(err.detail || err.message || "Cập nhật danh mục thất bại.");
        },
      }
    );
  };

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal-dialog" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2 className="modal-title">Chỉnh Sửa Danh Mục</h2>
          <button onClick={onClose} className="modal-close-btn" aria-label="Đóng">
            ✕
          </button>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="modal-body" style={{ display: "flex", flexDirection: "column", gap: "16px" }}>
            {clientError && (
              <div
                style={{
                  padding: "10px 14px",
                  backgroundColor: "hsla(0, 70%, 55%, 0.12)",
                  color: "var(--danger)",
                  borderRadius: "var(--radius-sm)",
                  fontSize: "0.85rem",
                  fontWeight: 600,
                }}
              >
                ⚠️ {clientError}
              </div>
            )}

            {/* Slug (Read-only C7) */}
            <div>
              <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", marginBottom: "6px" }}>
                <label className="form-label" style={{ fontWeight: 600, fontSize: "0.875rem" }}>
                  Đường dẫn cố định (Slug)
                </label>
                <span
                  style={{
                    fontSize: "0.75rem",
                    color: "var(--primary)",
                    backgroundColor: "hsla(24, 80%, 50%, 0.1)",
                    padding: "2px 6px",
                    borderRadius: "var(--radius-sm)",
                    fontWeight: 600,
                  }}
                >
                  🔒 Bất biến (SRS C7)
                </span>
              </div>
              <input
                type="text"
                className="input-search"
                style={{
                  height: "42px",
                  fontSize: "0.9rem",
                  backgroundColor: "var(--bg-surface-alt)",
                  color: "var(--text-tertiary)",
                  cursor: "not-allowed",
                }}
                value={category.slug}
                readOnly
                disabled
              />
              <div style={{ marginTop: "4px", fontSize: "0.75rem", color: "var(--text-tertiary)" }}>
                Slug được giữ nguyên bất biến khi đổi tên để bảo toàn liên kết công khai và chỉ mục SEO.
              </div>
            </div>

            {/* Tên danh mục */}
            <div>
              <label className="form-label" style={{ display: "block", marginBottom: "6px", fontWeight: 600, fontSize: "0.875rem" }}>
                Tên danh mục <span style={{ color: "var(--danger)" }}>*</span>
              </label>
              <input
                type="text"
                className="input-search"
                style={{ height: "42px", fontSize: "0.95rem" }}
                value={name}
                onChange={(e) => setName(e.target.value)}
                maxLength={50}
                required
              />
            </div>

            {/* Mô tả */}
            <div>
              <label className="form-label" style={{ display: "block", marginBottom: "6px", fontWeight: 600, fontSize: "0.875rem" }}>
                Mô tả giới thiệu
              </label>
              <textarea
                className="input-search"
                style={{ height: "80px", padding: "10px 14px", resize: "vertical", fontSize: "0.9rem" }}
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                maxLength={500}
              />
              <div style={{ textAlign: "right", fontSize: "0.75rem", color: "var(--text-tertiary)" }}>
                {description.length}/500 ký tự
              </div>
            </div>

            {/* Ảnh đại diện URL */}
            <div>
              <label className="form-label" style={{ display: "block", marginBottom: "6px", fontWeight: 600, fontSize: "0.875rem" }}>
                Đường dẫn hình ảnh (Image URL)
              </label>
              <input
                type="url"
                className="input-search"
                style={{ height: "42px", fontSize: "0.9rem" }}
                value={imageUrl}
                onChange={(e) => setImageUrl(e.target.value)}
              />
              {imageUrl && (
                <div style={{ marginTop: "8px", display: "flex", alignItems: "center", gap: "10px" }}>
                  <span style={{ fontSize: "0.75rem", color: "var(--text-secondary)" }}>Xem trước:</span>
                  <div
                    style={{
                      width: "36px",
                      height: "36px",
                      borderRadius: "var(--radius-sm)",
                      overflow: "hidden",
                      position: "relative",
                    }}
                  >
                    <Image src={imageUrl} alt="Preview" fill sizes="36px" style={{ objectFit: "cover" }} />
                  </div>
                </div>
              )}
            </div>

            {/* Thứ tự hiển thị */}
            <div>
              <label className="form-label" style={{ display: "block", marginBottom: "6px", fontWeight: 600, fontSize: "0.875rem" }}>
                Thứ tự sắp xếp (Order Index)
              </label>
              <input
                type="number"
                className="input-search"
                style={{ height: "42px", width: "120px", fontSize: "0.95rem" }}
                value={orderIndex}
                onChange={(e) => setOrderIndex(parseInt(e.target.value, 10) || 0)}
                min={0}
                max={9999}
              />
            </div>
          </div>

          <div className="modal-footer">
            <button type="button" onClick={onClose} className="btn btn-outline btn-sm">
              Hủy bỏ
            </button>
            <button
              type="submit"
              disabled={updateMutation.isPending}
              className="btn btn-primary btn-sm"
              style={{ minWidth: "120px" }}
            >
              {updateMutation.isPending ? "Đang lưu..." : "💾 Lưu thay đổi"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

// ============================================================================
// SUB-COMPONENT: DeleteCategoryModal (FR-CAT-005 Ràng buộc toàn vẹn)
// ============================================================================
interface DeleteCategoryModalProps {
  category: CategoryDto;
  onClose: () => void;
  onSuccess: () => void;
  onError: (errorMessage: string) => void;
  deleteMutation: ReturnType<typeof useDeleteCategory>;
}

function DeleteCategoryModal({
  category,
  onClose,
  onSuccess,
  onError,
  deleteMutation,
}: DeleteCategoryModalProps) {
  const hasRecipes = category.recipeCount > 0;

  const handleDelete = () => {
    deleteMutation.mutate(category.id, {
      onSuccess: () => {
        onSuccess();
      },
      onError: (err: AppError) => {
        if (err.status === 409 || err.errorCode === "CATEGORY_HAS_RECIPES") {
          onError("Không thể xóa danh mục vì vẫn còn công thức liên kết (Quy tắc FR-CAT-005).");
        } else {
          onError(err.detail || err.message || "Xóa danh mục thất bại.");
        }
      },
    });
  };

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal-dialog" onClick={(e) => e.stopPropagation()} style={{ maxWidth: "480px" }}>
        <div className="modal-header">
          <h2 className="modal-title" style={{ color: "var(--danger)" }}>
            Xác Nhận Xóa Danh Mục
          </h2>
          <button onClick={onClose} className="modal-close-btn" aria-label="Đóng">
            ✕
          </button>
        </div>

        <div className="modal-body">
          <p style={{ color: "var(--text-primary)", marginBottom: "16px", fontSize: "0.95rem" }}>
            Bạn có chắc chắn muốn xóa danh mục <strong>&ldquo;{category.name}&rdquo;</strong> (<code>{category.slug}</code>)?
          </p>

          {hasRecipes ? (
            <div
              style={{
                padding: "14px 16px",
                backgroundColor: "hsla(0, 70%, 55%, 0.12)",
                border: "1px solid hsla(0, 70%, 55%, 0.3)",
                borderRadius: "var(--radius-md)",
                color: "var(--danger)",
                fontSize: "0.875rem",
                lineHeight: 1.5,
              }}
            >
              <div style={{ fontWeight: 700, marginBottom: "4px", display: "flex", alignItems: "center", gap: "6px" }}>
                <span>⚠️ Chặn xóa theo quy tắc hệ thống (FR-CAT-005)</span>
              </div>
              Danh mục này hiện đang có <strong>{category.recipeCount} công thức nấu ăn</strong> liên kết. Hệ thống không cho phép xóa danh mục khi chưa chuyển các công thức này sang danh mục khác.
            </div>
          ) : (
            <p style={{ fontSize: "0.85rem", color: "var(--text-tertiary)" }}>
              Hành động này sẽ thực hiện xóa mềm (Soft-delete) danh mục khỏi hệ thống.
            </p>
          )}
        </div>

        <div className="modal-footer">
          <button type="button" onClick={onClose} className="btn btn-outline btn-sm">
            Hủy bỏ
          </button>
          <button
            type="button"
            disabled={hasRecipes || deleteMutation.isPending}
            onClick={handleDelete}
            className="btn btn-sm"
            style={{
              backgroundColor: hasRecipes ? "var(--bg-surface-alt)" : "var(--danger)",
              color: hasRecipes ? "var(--text-tertiary)" : "#ffffff",
              cursor: hasRecipes ? "not-allowed" : "pointer",
              minWidth: "120px",
              fontWeight: 600,
            }}
          >
            {deleteMutation.isPending ? "Đang xóa..." : "🗑️ Xác nhận xóa"}
          </button>
        </div>
      </div>
    </div>
  );
}
